using System.Collections.Generic;
using System.Linq;
using MedicalDeck.Data;
using MedicalDeck.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MedicalDeck.UI
{
    public class DeckEditorMenu : MonoBehaviour
    {
        [Header("Collection")]
        [SerializeField] private CardData[] allCards;
        [SerializeField] private RectTransform collectionListContainer;
        [SerializeField] private DeckEditorCardRow cardRowPrefab;

        [Header("Deck Display")]
        [SerializeField] private RectTransform deckListContainer;
        [SerializeField] private DeckEditorCardRow deckRowPrefab;
        [SerializeField] private Text deckCountText;
        [SerializeField] private int minDeckSize = 10;
        [SerializeField] private int maxDeckSize = 30;

        [Header("Battle")]
        [SerializeField] private string battleSceneName = "BattleScene";

        private List<CardData> workingDeck = new List<CardData>();

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
            RebuildCollectionList();
            RebuildDeckList();
        }

        private void RebuildCollectionList()
        {
            ClearContainer(collectionListContainer);
            if (collectionListContainer == null || cardRowPrefab == null) return;
            foreach (var card in allCards)
            {
                var row = Instantiate(cardRowPrefab, collectionListContainer);
                int count = workingDeck.Count(c => c == card);
                row.Bind(card, count, () => AddToDeck(card), null);
            }
        }

        private void RebuildDeckList()
        {
            ClearContainer(deckListContainer);
            if (deckListContainer == null || deckRowPrefab == null) return;
            int index = 0;
            foreach (var card in workingDeck)
            {
                int capturedIndex = index;
                var row = Instantiate(deckRowPrefab, deckListContainer);
                row.Bind(card, 1, () => RemoveFromDeck(capturedIndex), null);
                index++;
            }
            if (deckCountText != null) deckCountText.text = $"Deck: {workingDeck.Count}/{maxDeckSize}";
        }

        private void AddToDeck(CardData card)
        {
            if (workingDeck.Count >= maxDeckSize)
            {
                Debug.Log($"[DeckEditor] Deck is full ({maxDeckSize}).");
                return;
            }
            workingDeck.Add(card);
            DeckPersistence.Save(workingDeck);
            RebuildCollectionList();
            RebuildDeckList();
        }

        private void RemoveFromDeck(int index)
        {
            if (index < 0 || index >= workingDeck.Count) return;
            workingDeck.RemoveAt(index);
            DeckPersistence.Save(workingDeck);
            RebuildCollectionList();
            RebuildDeckList();
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