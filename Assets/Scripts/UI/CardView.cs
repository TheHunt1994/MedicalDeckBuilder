using MedicalDeck.Data;
using MedicalDeck.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace MedicalDeck.UI
{
    public class CardView : MonoBehaviour
    {
        [Header("Card Front")]
        [SerializeField] private Text cardNameText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text costText;
        [SerializeField] private Image artImage;     // leave blank — artist fills this
        [SerializeField] private Image frameImage;   // leave blank — artist fills this

        [Header("Selection")]
        [SerializeField] private GameObject selectionHighlight;
        [SerializeField] private Button cardButton;

        private CardData data;
        public CardData Data => data;

        private MatchManager match;
        private CardHandView handView;

        public event System.Action<CardView> OnCardClicked;

        private void Awake()
        {
            if (cardButton == null) cardButton = GetComponent<Button>();
            cardButton.onClick.AddListener(HandleClick);
        }

        public void Bind(CardData cardData, MatchManager matchRef, CardHandView hand)
        {
            data = cardData;
            match = matchRef;
            handView = hand;
            Refresh();
        }

        public void Refresh()
        {
            if (data == null) return;
            if (cardNameText != null) cardNameText.text = data.cardName;
            if (descriptionText != null) descriptionText.text = data.description;
            if (artImage != null) artImage.sprite = data.cardArt;
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (selectionHighlight != null) selectionHighlight.SetActive(selected);
        }

        private void HandleClick()
        {
            if (handView != null && !handView.Interactable) return;
            OnCardClicked?.Invoke(this);
        }
    }
}