using UnityEngine;

namespace MedicalDeck.Data
{
    public enum HealthStage
    {
        Full,
        Injured,
        Dead
    }

    [CreateAssetMenu(fileName = "NewFamilyMember", menuName = "Medical Deck/Family Member Data")]
    public class FamilyMemberData : ScriptableObject
    {
        [Header("Identity")]
        public string memberName = "New Member";
        [TextArea(2, 4)] public string description = "Family member description.";

        [Header("Stats")]
        public int maxHealth = 20;
        [Tooltip("Stage becomes Injured when health drops below this percent of max health.")]
        [Range(1, 99)] public int injuredBelowPercent = 50;

        [Header("Art stages (leave blank for artist)")]
        [Tooltip("Full health stage.")]
        public Sprite memberArt;
        [Tooltip("Injured stage. Falls back to full health art if blank.")]
        public Sprite injuredArt;
        [Tooltip("Dead stage. Falls back to full health art if blank.")]
        public Sprite deadArt;

        public Sprite GetStageArt(HealthStage stage)
        {
            switch (stage)
            {
                case HealthStage.Injured:
                    return injuredArt != null ? injuredArt : memberArt;
                case HealthStage.Dead:
                    return deadArt != null ? deadArt : memberArt;
                default:
                    return memberArt;
            }
        }
    }
}