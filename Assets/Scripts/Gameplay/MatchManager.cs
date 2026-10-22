using System.Collections.Generic;
using MedicalDeck.Data;
using MedicalDeck.UI;
using UnityEngine;

namespace MedicalDeck.Gameplay
{
    public enum MatchPhase
    {
        Idle,
        PlayerTurn,
        EnemyTurn,
        GameOver
    }

    public class MatchManager : MonoBehaviour
    {
        public const int TotalRounds = 15;
        public const int HandSize = 7;

        [Header("Decks")]
        [SerializeField] private DeckManager playerDeck;
        [SerializeField] private DeckManager enemyDeck;

        [Header("Families")]
        [SerializeField] private List<FamilyMemberData> playerFamilyData = new List<FamilyMemberData>();
        [SerializeField] private List<FamilyMemberData> enemyFamilyData = new List<FamilyMemberData>();
        [SerializeField] private FamilyMemberView[] playerFamilyViews;
        [SerializeField] private FamilyMemberView[] enemyFamilyViews;
        [SerializeField] private int infectionDamagePerStack = 1;

        [Header("UI")]
        [SerializeField] private BattleUIController battleUI;
        [SerializeField] private CardHandView playerHandView;

        private MatchPhase phase = MatchPhase.Idle;
        private int currentRound = 1;
        private CardData pendingCard;
        private FamilyMember pendingTarget;
        private bool waitingForTarget;

        private readonly List<FamilyMember> playerFamily = new List<FamilyMember>();
        private readonly List<FamilyMember> enemyFamily = new List<FamilyMember>();

        public MatchPhase Phase => phase;
        public int CurrentRound => currentRound;
        public bool WaitingForTarget => waitingForTarget;
        public DeckManager PlayerDeck => playerDeck;
        public DeckManager EnemyDeck => enemyDeck;
        public IReadOnlyList<FamilyMember> PlayerFamily => playerFamily;
        public IReadOnlyList<FamilyMember> EnemyFamily => enemyFamily;

        public event System.Action<MatchPhase, int> OnPhaseChanged;
        public event System.Action<FamilySide> OnMatchEnded;

        private void Start()
        {
            StartMatch();
        }

        public void StartMatch()
        {
            currentRound = 1;
            SetupFamilies();
            playerDeck.BuildDrawPile();
            enemyDeck.BuildDrawPile();
            playerDeck.DrawCards(HandSize);
            enemyDeck.DrawCards(HandSize);
            battleUI.RefreshAll();
            BeginTurn(FamilySide.Player);
        }

        private void SetupFamilies()
        {
            playerFamily.Clear();
            enemyFamily.Clear();

            int count = Mathf.Min(playerFamilyData.Count, playerFamilyViews.Length);
            for (int i = 0; i < count; i++)
            {
                var member = new FamilyMember(playerFamilyData[i], FamilySide.Player);
                playerFamily.Add(member);
                if (playerFamilyViews[i] != null) playerFamilyViews[i].Bind(member);
            }

            count = Mathf.Min(enemyFamilyData.Count, enemyFamilyViews.Length);
            for (int i = 0; i < count; i++)
            {
                var member = new FamilyMember(enemyFamilyData[i], FamilySide.Enemy);
                enemyFamily.Add(member);
                if (enemyFamilyViews[i] != null) enemyFamilyViews[i].Bind(member);
            }
        }

        private void BeginTurn(FamilySide side)
        {
            phase = side == FamilySide.Player ? MatchPhase.PlayerTurn : MatchPhase.EnemyTurn;
            OnPhaseChanged?.Invoke(phase, currentRound);

            if (phase == MatchPhase.PlayerTurn)
            {
                battleUI.SetPromptText($"Round {currentRound} — Your turn");
                playerHandView.RebuildHand();
                playerHandView.SetInteractable(true);
            }
            else
            {
                battleUI.SetPromptText("Enemy turn...");
                playerHandView.SetInteractable(false);
                GetComponent<EnemyAI>()?.TakeTurn(this);
            }
        }

        public void EndTurn(FamilySide side)
        {
            if (CheckVictoryConditions()) return;

            if (side == FamilySide.Player)
            {
                playerDeck.DiscardHand();
                playerHandView.RebuildHand();
                foreach (var member in playerFamily) member.TickInfection(infectionDamagePerStack);
                if (CheckVictoryConditions()) return;
                BeginTurn(FamilySide.Enemy);
            }
            else
            {
                enemyDeck.DiscardHand();
                foreach (var member in enemyFamily) member.TickInfection(infectionDamagePerStack);
                if (CheckVictoryConditions()) return;

                foreach (var member in playerFamily) member.TickBuffs();
                foreach (var member in enemyFamily) member.TickBuffs();

                currentRound++;
                if (currentRound > TotalRounds)
                {
                    EndMatch(DecideWinnerByHealth());
                    return;
                }

                playerDeck.DrawCards(HandSize);
                enemyDeck.DrawCards(HandSize);
                BeginTurn(FamilySide.Player);
            }
        }

        private FamilySide DecideWinnerByHealth()
        {
            int playerTotal = 0, enemyTotal = 0;
            foreach (var m in playerFamily) if (!m.IsDead) playerTotal += m.CurrentHealth;
            foreach (var m in enemyFamily) if (!m.IsDead) enemyTotal += m.CurrentHealth;
            if (playerTotal == enemyTotal) return FamilySide.Player;
            return playerTotal > enemyTotal ? FamilySide.Player : FamilySide.Enemy;
        }

        private bool CheckVictoryConditions()
        {
            bool playerAlive = false, enemyAlive = false;
            foreach (var m in playerFamily) if (!m.IsDead) { playerAlive = true; break; }
            foreach (var m in enemyFamily) if (!m.IsDead) { enemyAlive = true; break; }

            if (!playerAlive && !enemyAlive) { EndMatch(FamilySide.Enemy); return true; }
            if (!playerAlive) { EndMatch(FamilySide.Enemy); return true; }
            if (!enemyAlive) { EndMatch(FamilySide.Player); return true; }
            return false;
        }

        private void EndMatch(FamilySide winner)
        {
            phase = MatchPhase.GameOver;
            playerHandView.SetInteractable(false);
            battleUI.ShowGameOver(winner);
            OnMatchEnded?.Invoke(winner);
        }

        public void EndPlayerTurnRequested()
        {
            if (phase != MatchPhase.PlayerTurn) return;
            CancelPendingCard();
            EndTurn(FamilySide.Player);
        }

        public bool TrySelectCard(CardData card)
        {
            if (phase != MatchPhase.PlayerTurn) return false;
            if (waitingForTarget) CancelPendingCard();
            pendingCard = card;
            waitingForTarget = true;
            pendingTarget = null;
            battleUI.SetPromptText(
                pendingCard.requiresTarget
                    ? $"Selected '{card.cardName}'. Choose a target."
                    : $"Selected '{card.cardName}'. Press Play or Discard.");
            return true;
        }

        public void SelectTarget(FamilyMember target)
        {
            if (!waitingForTarget || pendingCard == null) return;
            if (target == null || target.IsDead) return;
            if (!IsValidTarget(pendingCard, target)) return;
            pendingTarget = target;
            battleUI.SetPromptText($"Target: {target.Data.memberName}. Press Play or Discard.");
        }

        public bool ConfirmPlay()
        {
            if (!waitingForTarget || pendingCard == null) return false;
            if (pendingCard.requiresTarget && pendingTarget == null)
            {
                battleUI.SetPromptText("Select a target first.");
                return false;
            }

            foreach (var effect in pendingCard.effects)
            {
                EffectExecutor.Execute(effect, pendingTarget, PlayerRequestDraw);
            }

            playerDeck.DiscardCard(pendingCard);
            playerHandView.RebuildHand();
            battleUI.SetPromptText($"Played '{pendingCard.cardName}'.");
            pendingCard = null;
            pendingTarget = null;
            waitingForTarget = false;
            return true;
        }

        public bool ConfirmDiscard()
        {
            if (!waitingForTarget || pendingCard == null) return false;
            playerDeck.DiscardCard(pendingCard);
            playerHandView.RebuildHand();
            battleUI.SetPromptText($"Discarded '{pendingCard.cardName}'.");
            pendingCard = null;
            pendingTarget = null;
            waitingForTarget = false;
            return true;
        }

        public void CancelPendingCard()
        {
            pendingCard = null;
            pendingTarget = null;
            waitingForTarget = false;
        }

        public bool IsValidTarget(CardData card, FamilyMember target)
        {
            if (target == null || target.IsDead) return false;
            if (card.validTarget == TargetSide.Any) return true;
            bool targetIsAlly = target.Side == FamilySide.Player;
            if (card.validTarget == TargetSide.Ally) return targetIsAlly;
            return !targetIsAlly;
        }

        public void PlayerRequestDraw(int count)
        {
            playerDeck.DrawCards(count);
            playerHandView.RebuildHand();
        }

        public void EnemyRequestDraw(int count)
        {
            enemyDeck.DrawCards(count);
        }

        public void PlayEnemyCard(CardData card, FamilyMember target)
        {
            if (card == null) return;
            foreach (var effect in card.effects)
            {
                EffectExecutor.Execute(effect, target, EnemyRequestDraw);
            }
            enemyDeck.DiscardCard(card);
        }

        public FamilyMember GetRandomAliveMember(FamilySide side)
        {
            var list = side == FamilySide.Player ? playerFamily : enemyFamily;
            var alive = list.FindAll(m => !m.IsDead);
            if (alive.Count == 0) return null;
            return alive[Random.Range(0, alive.Count)];
        }

        public List<FamilyMember> GetAliveMembers(FamilySide side)
        {
            var list = side == FamilySide.Player ? playerFamily : enemyFamily;
            return list.FindAll(m => !m.IsDead);
        }
    }
}