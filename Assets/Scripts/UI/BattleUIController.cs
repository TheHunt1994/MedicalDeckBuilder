using MedicalDeck.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace MedicalDeck.UI
{
    public class BattleUIController : MonoBehaviour
    {
        [Header("Top HUD")]
        [SerializeField] private Text roundLabel;
        [SerializeField] private Text phaseLabel;
        [SerializeField] private Text promptText;
        [SerializeField] private Text enemyDeckCountText;
        [SerializeField] private Text enemyDiscardCountText;
        [Header("Player piles")]
        [SerializeField] private Text playerDeckCountText;
        [SerializeField] private Text playerDiscardCountText;

        [Header("Action Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button discardButton;
        [SerializeField] private Button endTurnButton;

        [Header("Game Over")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text gameOverText;

        [Header("Scene Flow")]
        [SerializeField] private string menuSceneName = "MenuScene";
        [SerializeField] private string battleSceneName = "BattleScene";

        private MatchManager match;

        private void Start()
        {
            match = FindFirstObjectByType<MatchManager>();
            if (match != null)
            {
                match.OnPhaseChanged += HandlePhaseChanged;
            }
            if (playButton != null) playButton.onClick.AddListener(HandlePlayClicked);
            if (discardButton != null) discardButton.onClick.AddListener(HandleDiscardClicked);
            if (endTurnButton != null) endTurnButton.onClick.AddListener(HandleEndTurnClicked);
            RefreshAll();
        }

        private void OnDestroy()
        {
            if (match != null) match.OnPhaseChanged -= HandlePhaseChanged;
        }

        public void RefreshAll()
        {
            RefreshPiles();
            RefreshStats();
        }

        public void RefreshPiles()
        {
            if (match == null) return;
            if (roundLabel != null) roundLabel.text = $"Round {match.CurrentRound}/{MatchManager.TotalRounds}";
            if (enemyDeckCountText != null) enemyDeckCountText.text = $"Enemy Deck: {match.EnemyDeck.DrawPileCount}";
            if (enemyDiscardCountText != null) enemyDiscardCountText.text = $"Enemy Discard: {match.EnemyDeck.DiscardPileCount}";
            if (playerDeckCountText != null) playerDeckCountText.text = $"Deck: {match.PlayerDeck.DrawPileCount}";
            if (playerDiscardCountText != null) playerDiscardCountText.text = $"Discard: {match.PlayerDeck.DiscardPileCount}";
        }

        public void RefreshStats()
        {
            if (roundLabel != null) roundLabel.text = $"Round {match.CurrentRound}/{MatchManager.TotalRounds}";
        }

        public void RefreshStatsFor(FamilyMember member)
        {
            RefreshPiles();
        }

        public void SetPromptText(string text)
        {
            if (promptText != null) promptText.text = text;
        }

        private void HandlePhaseChanged(MatchPhase phase, int round)
        {
            RefreshPiles();
            bool playerCanAct = phase == MatchPhase.PlayerTurn;
            if (playButton != null) playButton.interactable = playerCanAct;
            if (discardButton != null) discardButton.interactable = playerCanAct;
            if (endTurnButton != null) endTurnButton.interactable = playerCanAct;
            if (phaseLabel != null)
            {
                phaseLabel.text = phase == MatchPhase.PlayerTurn ? "Your Turn"
                    : phase == MatchPhase.EnemyTurn ? "Enemy Turn"
                    : phase == MatchPhase.GameOver ? "Game Over" : "";
            }
        }

        private void HandlePlayClicked()
        {
            if (match != null) match.ConfirmPlay();
        }

        private void HandleDiscardClicked()
        {
            if (match != null) match.ConfirmDiscard();
        }

        private void HandleEndTurnClicked()
        {
            if (match != null) match.EndPlayerTurnRequested();
        }

        public void ShowGameOver(FamilySide winner)
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            if (gameOverText != null)
            {
                gameOverText.text = winner == FamilySide.Player ? "Victory! Your family survives." : "Defeat... your family has fallen.";
            }
            SetPromptText("");
        }

        public void LoadMenuScene()
        {
            if (!string.IsNullOrEmpty(menuSceneName)) UnityEngine.SceneManagement.SceneManager.LoadScene(menuSceneName);
        }

        public void LoadBattleScene()
        {
            if (!string.IsNullOrEmpty(battleSceneName)) UnityEngine.SceneManagement.SceneManager.LoadScene(battleSceneName);
        }
    }
}