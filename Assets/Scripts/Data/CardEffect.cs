using UnityEngine;

namespace MedicalDeck.Data
{
    [System.Serializable]
    public class CardEffect
    {
        [Tooltip("Key looked up in EffectExecutor. Examples: damage, heal, buff_attack, cleanse, draw")]
        public string effectId = "damage";
        public int amount = 0;
        public int duration = 1;
    }
}