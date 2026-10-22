using System.Collections.Generic;
using MedicalDeck.Data;
using UnityEngine;

namespace MedicalDeck.Gameplay
{
    public class DeckManager : MonoBehaviour
    {
        [SerializeField] private List<CardData> masterDeck = new List<CardData>();

        private List<CardData> drawPile = new List<CardData>();
        private List<CardData> discardPile = new List<CardData>();
        private List<CardData> hand = new List<CardData>();

        public IReadOnlyList<CardData> Hand => hand;
        public IReadOnlyList<CardData> DrawPile => drawPile;
        public IReadOnlyList<CardData> DiscardPile => discardPile;
        public int DrawPileCount => drawPile.Count;
        public int DiscardPileCount => discardPile.Count;

        public event System.Action OnPilesChanged;

        private void Start()
        {
            BuildDrawPile();
        }

        public void BuildDrawPile()
        {
            drawPile.Clear();
            discardPile.Clear();
            hand.Clear();
            drawPile.AddRange(masterDeck);
            Shuffle(drawPile);
            OnPilesChanged?.Invoke();
        }

        public void SetMasterDeck(List<CardData> cards)
        {
            masterDeck = new List<CardData>(cards);
            BuildDrawPile();
        }

        public void Shuffle(List<CardData> pile)
        {
            for (int i = pile.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (pile[i], pile[j]) = (pile[j], pile[i]);
            }
        }

        public List<CardData> DrawCards(int count)
        {
            var drawn = new List<CardData>();
            for (int i = 0; i < count; i++)
            {
                if (drawPile.Count == 0)
                {
                    if (discardPile.Count == 0) break;
                    drawPile.AddRange(discardPile);
                    discardPile.Clear();
                    Shuffle(drawPile);
                }
                CardData card = drawPile[0];
                drawPile.RemoveAt(0);
                hand.Add(card);
                drawn.Add(card);
            }
            OnPilesChanged?.Invoke();
            return drawn;
        }

        public void RemoveFromHand(CardData card)
        {
            hand.Remove(card);
        }

        public void DiscardCard(CardData card)
        {
            RemoveFromHand(card);
            discardPile.Add(card);
            OnPilesChanged?.Invoke();
        }

        public void DiscardHand()
        {
            foreach (var card in hand) discardPile.Add(card);
            hand.Clear();
            OnPilesChanged?.Invoke();
        }

        public void ReshuffleDeck()
        {
            discardPile.AddRange(hand);
            hand.Clear();
            BuildDrawPile();
        }

        public void AddToMasterDeck(CardData card) => masterDeck.Add(card);
        public void RemoveFromMasterDeck(CardData card) => masterDeck.Remove(card);
        public IReadOnlyList<CardData> GetMasterDeck() => masterDeck;
    }
}