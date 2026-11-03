using System.Collections.Generic;
using MedicalDeck.Data;
using UnityEngine;

namespace MedicalDeck.Gameplay
{
    public enum FamilySide
    {
        Player,
        Enemy
    }

    public class ActiveBuff
    {
        public string buffId;
        public int amount;
        public int remainingRounds;
    }

    public class PendingBuff
    {
        public string buffId;
        public int amount;
        public int duration;
    }

    public class FamilyMember
    {
        public FamilyMemberData Data { get; }
        public FamilySide Side { get; }
        public int CurrentHealth { get; private set; }
        public int Infection { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        private readonly List<ActiveBuff> buffs = new List<ActiveBuff>();
        private readonly List<PendingBuff> pendingBuffs = new List<PendingBuff>();
        private int oneShotDiseaseAmplify;

        public IReadOnlyList<ActiveBuff> Buffs => buffs;
        public IReadOnlyList<PendingBuff> PendingBuffs => pendingBuffs;

        public event System.Action<FamilyMember> OnStatsChanged;
        public event System.Action<FamilyMember> OnDeath;

        public FamilyMember(FamilyMemberData data, FamilySide side)
        {
            Data = data;
            Side = side;
            CurrentHealth = data.maxHealth;
            Infection = 0;
        }

        public int GetDiseaseReduction()
        {
            int total = 0;
            foreach (var b in buffs) if (b.buffId == "disease_shield") total += b.amount;
            return total;
        }

        public int GetDiseaseAmplification()
        {
            int total = oneShotDiseaseAmplify;
            foreach (var b in buffs) if (b.buffId == "disease_amplify") total += b.amount;
            return total;
        }

        public bool HasBuff(string buffId) => buffs.Exists(b => b.buffId == buffId);

        public void ApplyDamage(int amount)
        {
            if (amount <= 0 || IsDead) return;
            CurrentHealth -= amount;
            OnStatsChanged?.Invoke(this);
            CheckDeath();
        }

        public void ApplyInfection(int amount)
        {
            if (amount <= 0 || IsDead) return;
            if (HasBuff("quarantine")) return;
            Infection += amount;
            OnStatsChanged?.Invoke(this);
        }

        public void ApplyHeal(int amount)
        {
            if (amount <= 0 || IsDead) return;
            if (HasBuff("block_heal")) return;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, Data.maxHealth);
            OnStatsChanged?.Invoke(this);
        }

        public void RestoreFullHealth()
        {
            if (IsDead) return;
            CurrentHealth = Data.maxHealth;
            OnStatsChanged?.Invoke(this);
        }

        public void CleanseInfection(int amount)
        {
            if (amount <= 0) return;
            Infection = Mathf.Max(0, Infection - amount);
            OnStatsChanged?.Invoke(this);
        }

        public void AddBuff(string buffId, int amount, int duration)
        {
            if (IsDead) return;
            if (HasBuff("quarantine") && buffId != "quarantine") return;

            if (HasBuff("malnourished") && buffId != "malnourished")
            {
                pendingBuffs.Add(new PendingBuff { buffId = buffId, amount = amount, duration = duration });
                OnStatsChanged?.Invoke(this);
                return;
            }

            var existing = buffs.Find(b => b.buffId == buffId);
            if (existing != null)
            {
                existing.amount = Mathf.Max(existing.amount, amount);
                existing.remainingRounds = Mathf.Max(existing.remainingRounds, duration);
            }
            else
            {
                buffs.Add(new ActiveBuff { buffId = buffId, amount = amount, remainingRounds = duration });
            }
            OnStatsChanged?.Invoke(this);
        }

        public void AddOneShotDiseaseAmplify(int amount)
        {
            if (IsDead) return;
            if (HasBuff("quarantine")) return;
            oneShotDiseaseAmplify += amount;
            OnStatsChanged?.Invoke(this);
        }

        public int GetBuffAmount(string buffId)
        {
            var buff = buffs.Find(b => b.buffId == buffId);
            return buff != null ? buff.amount : 0;
        }

        public void TickBuffs()
        {
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                buffs[i].remainingRounds--;
                if (buffs[i].remainingRounds <= 0) buffs.RemoveAt(i);
            }

            for (int i = pendingBuffs.Count - 1; i >= 0; i--)
            {
                var pending = pendingBuffs[i];
                buffs.Add(new ActiveBuff { buffId = pending.buffId, amount = pending.amount, remainingRounds = pending.duration });
                pendingBuffs.RemoveAt(i);
            }

            OnStatsChanged?.Invoke(this);
        }

        public void TickInfection(int damagePerStack)
        {
            if (Infection <= 0 && GetDiseaseAmplification() == 0) return;
            int raw = Infection * damagePerStack;
            int amplified = raw + GetDiseaseAmplification();
            int final = Mathf.Max(0, amplified - GetDiseaseReduction());
            oneShotDiseaseAmplify = 0;
            if (final > 0) ApplyDamage(final);
        }

        private void CheckDeath()
        {
            if (IsDead) OnDeath?.Invoke(this);
        }
    }
}