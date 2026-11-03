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
        [SerializeField] private int maxCopiesPerCard = 3;

        [Header("Add Confirmation Dialog")]
        [SerializeField] private GameObject addDialog;
        [SerializeField] private Text addDialogNameText;
        [SerializeField] private Text addDialogDescText;
        [SerializeField] private Image addDialogArtImage;

        [Header("Card Preview")]
        [SerializeField] private GameObject previewPanel;
        [SerializeField] private Image previewArtImage;
        [SerializeField] private Text previewNameText;
        [SerializeField] private Text previewDescText;
        [SerializeField] private float previewDelaySeconds = 1f;
        [SerializeField] private float cursorGapPixels = 40f;

        [Header("Battle")]
        [SerializeField] private string battleSceneName = "BattleScene";

        private readonly List<CardData> workingDeck = new List<CardData>();
        private CardData pendingAddCard;
        private RectTransform previewRect;
        private Canvas parentCanvas;
        private CardData hoveredCard;
        private Coroutine previewRoutine;

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
            var saved = DeckPersistence.Load(allCards);
            workingDeck.Clear();
            workingDeck.AddRange(saved);
            if (previewPanel != null) previewRect = previewPanel.GetComponent<RectTransform>();
            parentCanvas = previewPanel != null ? previewPanel.GetComponentInParent<Canvas>() : null;
            RebuildCollectionList();
            RebuildDeckList();
        }

        private void Update()
        {
            if (previewPanel != null && previewPanel.activeSelf && previewRect != null)
            {
                PositionPreviewBesideCursor();
            }
        }

        private int CopiesInDeck(CardData card)
        {
            return workingDeck.Count(c => c.cardName == card.cardName);
        }

        private void RebuildCollectionList()
        {
            ClearContainer(collectionGrid);
            if (collectionGrid == null || tilePrefab == null) return;
            foreach (var card in allCards.OrderBy(c => c.cardName))
            {
                var tile = Instantiate(tilePrefab, collectionGrid);
                tile.Bind(card, CopiesInDeck(card), () => RequestAdd(card), ShowPreview, HidePreview);
            }
        }

        private void RebuildDeckList()
        {
            ClearContainer(deckGrid);
            if (deckGrid == null || tilePrefab == null) return;

            var uniqueSorted = workingDeck
                .GroupBy(c => c.cardName)
                .Select(g => g.First())
                .OrderBy(c => c.cardName);

            foreach (var card in uniqueSorted)
            {
                var tile = Instantiate(tilePrefab, deckGrid);
                tile.Bind(card, CopiesInDeck(card), () => RemoveCopy(card), ShowPreview, HidePreview);
            }

            if (deckCountText != null) deckCountText.text = $"Deck: {workingDeck.Count}/{maxDeckSize}";
        }

        private void RequestAdd(CardData card)
        {
            if (card == null) return;
            if (workingDeck.Count >= maxDeckSize)
            {
                Debug.Log($"[DeckEditor] Deck is full ({maxDeckSize} cards).");
                return;
            }
            if (CopiesInDeck(card) >= maxCopiesPerCard)
            {
                Debug.Log($"[DeckEditor] Max {maxCopiesPerCard} copies of '{card.cardName}' allowed.");
                return;
            }

            pendingAddCard = card;
            if (addDialogNameText != null) addDialogNameText.text = card.cardName;
            if (addDialogDescText != null) addDialogDescText.text = card.description;
            if (addDialogArtImage != null) addDialogArtImage.sprite = card.cardArt;
            if (addDialog != null) addDialog.SetActive(true);
        }

        public void ConfirmAdd()
        {
            if (pendingAddCard == null) return;
            workingDeck.Add(pendingAddCard);
            DeckPersistence.Save(workingDeck);
            pendingAddCard = null;
            if (addDialog != null) addDialog.SetActive(false);
            RebuildCollectionList();
            RebuildDeckList();
        }

        public void CancelAdd()
        {
            pendingAddCard = null;
            if (addDialog != null) addDialog.SetActive(false);
        }

        private void RemoveCopy(CardData card)
        {
            var match = workingDeck.FirstOrDefault(c => c.cardName == card.cardName);
            if (match == null) return;
            workingDeck.Remove(match);
            DeckPersistence.Save(workingDeck);
            RebuildCollectionList();
            RebuildDeckList();
        }

        private void ShowPreview(CardData card)
        {
            hoveredCard = card;
            if (card == null || previewPanel == null) return;
            if (previewRoutine != null) StopCoroutine(previewRoutine);
            previewRoutine = StartCoroutine(PreviewAfterDelay(card));
        }

        private System.Collections.IEnumerator PreviewAfterDelay(CardData card)
        {
            yield return new WaitForSeconds(previewDelaySeconds);
            if (hoveredCard != card) yield break;

            if (previewArtImage != null) previewArtImage.sprite = card.cardArt;
            if (previewNameText != null) previewNameText.text = card.cardName;
            if (previewDescText != null) previewDescText.text = card.description;
            previewPanel.SetActive(true);
            PositionPreviewBesideCursor();
        }

        private void HidePreview()
        {
            hoveredCard = null;
            if (previewRoutine != null)
            {
                StopCoroutine(previewRoutine);
                previewRoutine = null;
            }
            if (previewPanel != null) previewPanel.SetActive(false);
        }

        private void PositionPreviewBesideCursor()
        {
            if (previewRect == null || parentCanvas == null) return;
            float scale = parentCanvas.scaleFactor;
            float canvasWidth = Screen.width / scale;
            float canvasHeight = Screen.height / scale;
            Vector2 mouse = new Vector2(Input.mousePosition.x / scale, Input.mousePosition.y / scale);
            Vector2 size = previewRect.sizeDelta;
            float margin = 10f;
            float gap = cursorGapPixels;

            bool fitsRight = mouse.x + gap + size.x + margin <= canvasWidth;
            bool fitsLeft = mouse.x - gap - size.x - margin >= 0f;

            float centerX;
            if (fitsRight) centerX = mouse.x + gap + size.x * 0.5f;
            else if (fitsLeft) centerX = mouse.x - gap - size.x * 0.5f;
            else centerX = Mathf.Clamp(mouse.x, size.x * 0.5f + margin, canvasWidth - size.x * 0.5f - margin);

            float centerY = Mathf.Clamp(mouse.y, size.y * 0.5f + margin, canvasHeight - size.y * 0.5f - margin);

            previewRect.position = new Vector3(centerX, centerY, 0f);
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
                Debug.Log($"[DeckEditor] Deck needs at least {minDeckSize} cards. Current: {workingDeck.Count}");
                return;
            }
            DeckPersistence.Save(workingDeck);
            SceneManager.LoadScene(battleSceneName);
        }
    }
}