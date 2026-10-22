using UnityEngine;

namespace MedicalDeck.Data
{
    [CreateAssetMenu(fileName = "NewFamilyMember", menuName = "Medical Deck/Family Member Data")]
    public class FamilyMemberData : ScriptableObject
    {
        [Header("Identity")]
        public string memberName = "New Member";
        [TextArea(2, 4)] public string description = "Family member description.";

        [Header("Stats")]
        public int maxHealth = 20;

        [Header("Art (leave blank for artist)")]
        public Sprite memberArt;
    }
}