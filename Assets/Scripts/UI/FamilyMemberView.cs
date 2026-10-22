using MedicalDeck.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace MedicalDeck.UI
{
    public class FamilyMemberView : MonoBehaviour
    {
        [SerializeField] private Text nameText;
        [SerializeField] private Text healthText;
        [SerializeField] private Text infectionText;
        [SerializeField] private Text buffText;
        [SerializeField] private Image artImage;          // leave blank — artist fills this
        [SerializeField] private Image infectionOverlayImage; // leave blank — artist supplies green tint
        [SerializeField] private GameObject highlightObject;
        [SerializeField] private GameObject deadOverlayObject;
        [SerializeField] private Button selectButton;

        private FamilyMember member;
        private MatchManager match;

        public FamilyMember Member => member;

        private void Awake()
        {
            if (selectButton == null) selectButton = GetComponent<Button>();
            selectButton.onClick.AddListener(HandleClicked);
        }

        public void Bind(FamilyMember familyMember)
        {
            member = familyMember;
            match = FindFirstObjectByType<MatchManager>();
            member.OnStatsChanged -= HandleStatsChanged;
            member.OnStatsChanged += HandleStatsChanged;
            member.OnDeath -= HandleDeath;
            member.OnDeath += HandleDeath;
            Refresh();
        }

        private void HandleStatsChanged(FamilyMember m) => Refresh();
        private void HandleDeath(FamilyMember m) => Refresh();

        private void Refresh()
        {
            if (member == null) return;
            if (nameText != null) nameText.text = member.Data.memberName;
            if (healthText != null) healthText.text = $"{member.CurrentHealth}/{member.Data.maxHealth}";
            if (infectionText != null) infectionText.text = member.Infection > 0 ? $"☣ {member.Infection}" : "";
            if (buffText != null)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var buff in member.Buffs)
                    sb.Append($"[{buff.buffId} +{buff.amount} ({buff.remainingRounds})] ");
                buffText.text = sb.ToString();
            }
            if (artImage != null) artImage.sprite = member.Data.memberArt;
            if (infectionOverlayImage != null)
            {
                infectionOverlayImage.gameObject.SetActive(member.Infection > 0);
            }
            if (deadOverlayObject != null) deadOverlayObject.SetActive(member.IsDead);
            SetHighlighted(false);
        }

        public void SetHighlighted(bool on)
        {
            if (highlightObject != null) highlightObject.SetActive(on);
        }

        private void HandleClicked()
        {
            if (member == null || match == null) return;
            match.SelectTarget(member);
            SetHighlighted(true);
        }
    }
}