using System;
using MedicalDeck.Data;
using UnityEngine;
using UnityEngine.UI;

namespace MedicalDeck.UI
{
    public class DeckEditorCardRow : MonoBehaviour
    {
        [SerializeField] private Text cardNameText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text countText;
        [SerializeField] private Image artImage;
        [SerializeField] private Button actionButton;
        [SerializeField] private Text actionButtonText;

        private CardData data;
        private Action actionCallback;

        private void Awake()
        {
            if (actionButton == null) actionButton = GetComponent<Button>();
            actionButton.onClick.AddListener(HandleClicked);
        }

        public void Bind(CardData cardData, int ownedCount, Action onAction, Action secondaryAction)
        {
            data = cardData;
            actionCallback = onAction;
            if (cardNameText != null) cardNameText.text = data.cardName;
            if (descriptionText != null) descriptionText.text = data.description;
            if (countText != null) countText.text = ownedCount.ToString();
            if (artImage != null) artImage.sprite = data.cardArt;
        }

        private void HandleClicked()
        {
            actionCallback?.Invoke();
        }
    }
}