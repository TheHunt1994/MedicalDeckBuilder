namespace MedicalDeck.Data
{
    public enum CardEffectId
    {
        Damage,
        Infect,
        Heal,
        RestoreFullHealth,
        Cleanse,
        OneShotDiseaseAmplify,
        BuffMaxHealth,
        BuffHealPower,
        BuffDamagePower,
        BuffInfectionPower,
        Draw
    }

    [System.Serializable]
    public class CardEffect
    {
        public CardEffectId effectId = CardEffectId.Damage;
        public int amount = 0;
        public int duration = 1;
    }
}