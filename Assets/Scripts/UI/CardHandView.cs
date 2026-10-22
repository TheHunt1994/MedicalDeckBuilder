using System.Collections.Generic;
using MedicalDeck.Data;
using MedicalDeck.Gameplay;
using UnityEngine;

namespace MedicalDeck.UI
{
    public class CardHandView : MonoBehaviour
    {
        [SerializeField] private MatchManager match;
        [SerializeField] private RectTransform cardContainer;
        [SerializeField] private CardView cardViewPrefab;
        [SerializeField] private float cardSpacing = 150f;
        [SerializeField] private float selectedLift = 30f;

        private readonly List<CardView> cardViews = new List<CardView>();
        private CardView selectedCard;
        private bool interactable = true;

        public bool Interactable => interactable;

        public void SetInteractable(bool value)
        {
            interactable = value;
            foreach (var view in cardViews) view.SetSelected(false);
            if (!interactable) selectedCard = null;
        }

        public void RebuildHand()
        {
            selectedCard = null;
            for (int i = cardViews.Count - 1; i >= 0; i--) Destroy(cardViews[i].gameObject);
            cardViews.Clear();

            if (match == null || cardContainer == null || cardViewPrefab == null) return;
            var hand = match.PlayerDeck.Hand;
            int count = hand.Count;
            for (int i = 0; i < count; i++)
            {
                var view = Instantiate(cardViewPrefab, cardContainer);
                view.Bind(hand[i], match, this);
                view.OnCardClicked += HandleCardClicked;
                cardViews.Add(view);
                PositionCard(view, i, count, false);
            }
        }

        private void HandleCardClicked(CardView view)
        {
            if (!interactable) return;
            if (selectedCard == view)
            {
                selectedCard = null;
                view.SetSelected(false);
                match.CancelPendingCard();
                return;
            }

            if (selectedCard != null) selectedCard.SetSelected(false);
            selectedCard = view;
            view.SetSelected(true);
            match.TrySelectCard(view.Data);
        }

        private void PositionCard(CardView view, int index, int total, bool isLifted)
        {
            float step = cardSpacing;
            float startX = -step * (total - 1) / 2f;
            Vector2 anchored = new Vector2(startX + index * step, isLifted ? selectedLift : 0f);
            ((RectTransform)view.transform).anchoredPosition = anchored;
        }
    }
}