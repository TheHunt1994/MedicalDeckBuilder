using System.Collections.Generic;
using MedicalDeck.Data;
using UnityEngine;

namespace MedicalDeck.Gameplay
{
    public class EnemyAI : MonoBehaviour
    {
        [SerializeField] private float turnDelaySeconds = 1f;

        public void TakeTurn(MatchManager match)
        {
            StartCoroutine(RunTurn(match));
        }

        private System.Collections.IEnumerator RunTurn(MatchManager match)
        {
            yield return new WaitForSeconds(turnDelaySeconds);

            int plays = Random.Range(1, 3);
            for (int i = 0; i < plays; i++)
            {
                if (match.Phase != MatchPhase.EnemyTurn) yield break;
                var hand = match.EnemyDeck.Hand;
                if (hand.Count == 0) break;

                CardData chosen = null;
                FamilyMember target = null;
                PickCardAndTarget(match, hand, ref chosen, ref target);
                if (chosen == null) break;

                match.PlayEnemyCard(chosen, target);
                yield return new WaitForSeconds(turnDelaySeconds);
            }

            if (match.Phase == MatchPhase.EnemyTurn)
            {
                match.EndTurn(FamilySide.Enemy);
            }
        }

        private void PickCardAndTarget(MatchManager match, IReadOnlyList<CardData> hand, ref CardData chosen, ref FamilyMember target)
        {
            var playerTargets = match.GetAliveMembers(FamilySide.Player);
            var enemyTargets = match.GetAliveMembers(FamilySide.Enemy);
            bool enemyNeedsHealing = false;
            FamilyMember wounded = null;
            foreach (var m in enemyTargets)
            {
                if (m.CurrentHealth < m.Data.maxHealth / 2) { wounded = m; enemyNeedsHealing = true; break; }
            }

            for (int pass = 0; pass < 2 && chosen == null; pass++)
            {
                for (int i = 0; i < hand.Count; i++)
                {
                    var card = hand[i];
                    bool aggressive = pass == 0;

                    if (card.cardType == CardType.Heal && enemyNeedsHealing && wounded != null)
                    {
                        chosen = card;
                        target = wounded;
                        return;
                    }

                    if (aggressive && card.cardType == CardType.Infection && playerTargets.Count > 0)
                    {
                        chosen = card;
                        target = playerTargets[Random.Range(0, playerTargets.Count)];
                        return;
                    }

                    if (!aggressive && card.validTarget != TargetSide.Ally && playerTargets.Count > 0)
                    {
                        chosen = card;
                        target = playerTargets[Random.Range(0, playerTargets.Count)];
                        return;
                    }

                    if (!aggressive && card.validTarget == TargetSide.Ally && enemyTargets.Count > 0)
                    {
                        chosen = card;
                        target = enemyTargets[Random.Range(0, enemyTargets.Count)];
                        return;
                    }
                }
            }
        }
    }
}