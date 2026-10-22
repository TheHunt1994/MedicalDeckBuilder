using System.Collections.Generic;
using MedicalDeck.Data;
using UnityEngine;

namespace MedicalDeck.Gameplay
{
    public class PlayerDeckLoader : MonoBehaviour
    {
        [SerializeField] private DeckManager playerDeck;
        [SerializeField] private CardData[] allCards;

        private void Awake()
        {
            var saved = MedicalDeck.UI.DeckEditorMenu.DeckPersistence.Load(allCards);
            if (saved.Count > 0)
            {
                playerDeck.SetMasterDeck(new List<CardData>(saved));
            }
        }
    }
}