using System.Collections.Generic;
using System.Linq;
using MedicalDeck.Data;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MedicalDeck.UI
{
    public class DeckEditorMenu : MonoBehaviour
    {
        [Header("Collection")]
        [SerializeField] private CardData[] allCards;
        [SerializeField] private RectTransform collectionGrid;
        [SerializeField] private DeckEditorCardRow tilePrefab;

        [Header("Deck Display")]
        [SerializeField] private RectTransform deckGrid;
        [SerializeField] private Text deckCountText;
        [SerializeField] private int minDeckSize = 10;
        [SerializeField] private int maxDeckSize = 30;

        [Header("Add Dialog")]
        [SerializeField] private GameObject addDialog;
        [SerializeField] private Text addDialogNameText;
        [SerializeField] private Text addDialogDescText;
        [SerializeField] private Image addDialogArtImage;

        [Header("Preview Panel")]
        [SerializeField] private GameObject previewPanel;
        [SerializeField] private Image previewArtImage;
        [SerializeField] private Text previewNameText;
        [SerializeField] private Text previewDescText;

        [Header("Battle")]
        [SerializeField] private string battleSceneName = "BattleScene";

        private List<CardData> workingDeck = new List<CardData>();
        private CardData pendingAdd;

        public static class DeckPersistence
        {
            private const string Key = "player_deck_ids";

            public static void Save(IEnumerable<CardData> deck)
            {
                var ids = string.Join(",", deck.Select(c => c.cardName));
                PlayerPrefs.SetString(Key, ids);
                PlayerPrefs.Save();
            }

            public static List<CardData> Load(IReadOnlyList<CardData> collection)
            {
                var result = new List<CardData>();
                string raw = PlayerPrefs.GetString(Key, "");
                if (string.IsNullOrEmpty(raw)) return result;
                var parts = raw.Split(',');
                foreach (var part in parts)
                {
                    var matchCard = collection.FirstOrDefault(c => c.cardName == part);
                    if (matchCard != null) result.Add(matchCard);
                }
                return result;
            }
        }

        private void Start()
        {
            workingDeck = DeckPersistence.Load(allCards);
            if (workingDeck.Count == 0)
            {
                foreach (var card in allCards) workingDeck.Add(card);
            }
            if (addDialog != null) addDialog.SetActive(false);
            if (previewPanel != null) previewPanel.SetActive(false);
            RebuildCollectionGrid();
            RebuildDeckGrid();
        }

        private void RebuildCollectionGrid()
        {
            ClearContainer(collectionGrid);
            if (collectionGrid == null || tilePrefab == null) return;
            foreach (var card in allCards)
            {
                var tile = Instantiate(tilePrefab, collectionGrid);
                int count = workingDeck.Count(c => c == card);
                tile.Bind(card, count, () => OpenAddDialog(card), c => ShowPreview(c), () => HidePreview());
            }
        }

        private void RebuildDeckGrid()
        {
            ClearContainer(deckGrid);
            if (deckGrid == null || tilePrefab == null) return;
            int index = 0;
            foreach (var card in workingDeck)
            {
                int capturedIndex = index;
                var tile = Instantiate(tilePrefab, deckGrid);
                tile.Bind(card, 1, () => RemoveFromDeck(capturedIndex), c => ShowPreview(c), () => HidePreview());
                index++;
            }
            if (deckCountText != null) deckCountText.text = $"Deck: {workingDeck.Count}/{maxDeckSize}";
        }

        private void OpenAddDialog(CardData card)
        {
            pendingAdd = card;
            ShowPreview(card);
            if (addDialogNameText != null) addDialogNameText.text = card.cardName;
            if (addDialogDescText != null) addDialogDescText.text = card.description;
            if (addDialogArtImage != null) addDialogArtImage.sprite = card.cardArt;
            if (addDialog != null) addDialog.SetActive(true);
        }

        public void ConfirmAdd()
        {
            if (pendingAdd == null) return;
            if (workingDeck.Count >= maxDeckSize)
            {
                Debug.Log($"[DeckEditor] Deck is full ({maxDeckSize}).");
            }
            else
            {
                workingDeck.Add(pendingAdd);
                DeckPersistence.Save(workingDeck);
                RebuildCollectionGrid();
                RebuildDeckGrid();
            }
            if (addDialog != null) addDialog.SetActive(false);
            pendingAdd = null;
        }

        public void CancelAdd()
        {
            if (addDialog != null) addDialog.SetActive(false);
            pendingAdd = null;
        }

        private void ShowPreview(CardData card)
        {
            if (previewPanel == null) return;
            previewPanel.SetActive(true);
            if (previewArtImage != null) previewArtImage.sprite = card.cardArt;
            if (previewNameText != null) previewNameText.text = card.cardName;
            if (previewDescText != null) previewDescText.text = card.description;
        }

        private void HidePreview()
        {
            if (previewPanel != null) previewPanel.SetActive(false);
        }

        private void RemoveFromDeck(int index)
        {
            if (index < 0 || index >= workingDeck.Count) return;
            workingDeck.RemoveAt(index);
            DeckPersistence.Save(workingDeck);
            RebuildCollectionGrid();
            RebuildDeckGrid();
        }

        private void ClearContainer(RectTransform container)
        {
            if (container == null) return;
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                var child = container.GetChild(i);
                if (child != null) Destroy(child.gameObject);
            }
        }

        public void StartMatch()
        {
            if (workingDeck.Count < minDeckSize)
            {
                Debug.Log($"[DeckEditor] Deck needs at least {minDeckSize} cards.");
                return;
            }
            DeckPersistence.Save(workingDeck);
            SceneManager.LoadScene(battleSceneName);
        }
    }
}