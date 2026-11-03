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

        public static void Execute(string effectId, int amount, int duration, FamilyMember target, DrawRequestHandler drawHandler)
        {
            switch (effectId)
            {
                case "damage": target?.ApplyDamage(amount); break;
                case "infect": target?.ApplyInfection(amount); break;
                case "heal": target?.ApplyHeal(amount); break;
                case "cleanse": target?.CleanseInfection(amount); break;
                case "full_heal": target?.RestoreFullHealth(); break;
                case "buff_max_health": target?.AddBuff("max_health", amount, duration); break;
                case "buff_heal_power": target?.AddBuff("heal_power", amount, duration); break;
                case "buff_damage_power": target?.AddBuff("damage_power", amount, duration); break;
                case "buff_infection_power": target?.AddBuff("infection_power", amount, duration); break;
                case "disease_shield": target?.AddBuff("disease_shield", amount, duration); break;
                case "disease_amplify": target?.AddBuff("disease_amplify", amount, duration); break;
                case "disease_amplify_next": target?.AddOneShotDiseaseAmplify(amount); break;
                case "block_heal": target?.AddBuff("block_heal", amount, duration); break;
                case "quarantine": target?.AddBuff("quarantine", amount, duration); break;
                case "delay_buffs": target?.AddBuff("malnourished", amount, duration); break;
                case "draw": drawHandler?.Invoke(amount); break;
                default:
                    UnityEngine.Debug.LogWarning($"[EffectExecutor] Unknown effectId '{effectId}' — nothing applied. Add it to the switch in EffectExecutor.cs.");
                    break;
            }
        }
    }
}