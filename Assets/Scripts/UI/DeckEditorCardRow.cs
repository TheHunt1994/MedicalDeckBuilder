using System;
using MedicalDeck.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MedicalDeck.UI
{
    public class DeckEditorCardRow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Text cardNameText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text countText;
        [SerializeField] private Image artImage;
        [SerializeField] private Button actionButton;

        private CardData data;
        private Action actionCallback;
        private Action<CardData> hoverEnterCallback;
        private Action hoverExitCallback;

        public CardData Data => data;

        private void Awake()
        {
            if (actionButton == null) actionButton = GetComponent<Button>();
            actionButton.onClick.AddListener(HandleClicked);
        }

        public void Bind(CardData cardData, int ownedCount, Action onAction, Action<CardData> onHoverEnter, Action onHoverExit)
        {
            data = cardData;
            actionCallback = onAction;
            hoverEnterCallback = onHoverEnter;
            hoverExitCallback = onHoverExit;
            if (cardNameText != null) cardNameText.text = data.cardName;
            if (descriptionText != null) descriptionText.text = data.description;
            if (artImage != null) artImage.sprite = data.cardArt;
            ShowCount(ownedCount);
        }

        public void ShowCount(int count)
        {
            if (countText != null) countText.text = count > 1 ? $"x{count}" : "";
        }

        private void HandleClicked()
        {
            actionCallback?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (data != null && hoverEnterCallback != null) hoverEnterCallback(data);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (hoverExitCallback != null) hoverExitCallback();
        }
    }
}