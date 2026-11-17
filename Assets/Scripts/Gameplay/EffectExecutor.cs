using MedicalDeck.Data;

namespace MedicalDeck.Gameplay
{
    public static class EffectExecutor
    {
        public delegate void DrawRequestHandler(int count);

        public static void Execute(CardEffect effect, FamilyMember target, DrawRequestHandler drawHandler)
        {
            if (effect == null) return;
            Execute(effect.effectId, effect.amount, effect.duration, target, drawHandler);
        }

        public static void Execute(CardEffectId effectId, int amount, int duration, FamilyMember target, DrawRequestHandler drawHandler)
        {
            switch (effectId)
            {
                case CardEffectId.Damage: target?.ApplyDamage(amount); break;
                case CardEffectId.Infect: target?.ApplyInfection(amount); break;
                case CardEffectId.Heal: target?.ApplyHeal(amount); break;
                case CardEffectId.RestoreFullHealth: target?.RestoreFullHealth(); break;
                case CardEffectId.Cleanse: target?.CleanseInfection(amount); break;
                case CardEffectId.OneShotDiseaseAmplify: target?.AddOneShotDiseaseAmplify(amount); break;
                case CardEffectId.BuffMaxHealth: target?.AddBuff("max_health", amount, duration); break;
                case CardEffectId.BuffHealPower: target?.AddBuff("heal_power", amount, duration); break;
                case CardEffectId.BuffDamagePower: target?.AddBuff("damage_power", amount, duration); break;
                case CardEffectId.BuffInfectionPower: target?.AddBuff("infection_power", amount, duration); break;
                case CardEffectId.Draw: drawHandler?.Invoke(amount); break;
                default:
                    UnityEngine.Debug.LogWarning($"[EffectExecutor] Unhandled CardEffectId '{effectId}' — nothing applied.");
                    break;
            }
        }
    }
}