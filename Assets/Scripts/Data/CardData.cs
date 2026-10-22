using System.Collections.Generic;
using UnityEngine;

namespace MedicalDeck.Data
{
    public enum TargetSide
    {
        Ally,
        Enemy,
        Any
    }

    public enum CardType
    {
        Infection,
        Heal,
        Buff
    }

    [CreateAssetMenu(fileName = "NewCard", menuName = "Medical Deck/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("Identity")]
        public string cardName = "New Card";
        [TextArea(2, 4)] public string description = "Card description.";
        public CardType cardType = CardType.Heal;

        [Header("Targeting")]
        public TargetSide validTarget = TargetSide.Any;
        public bool requiresTarget = true;

        [Header("Effects (applied to the chosen target)")]
        public List<CardEffect> effects = new List<CardEffect>();

        [Header("Art (leave blank for artist)")]
        public Sprite cardArt;
    }
}