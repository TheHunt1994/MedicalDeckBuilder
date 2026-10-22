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

    public class FamilyMember
    {
        public FamilyMemberData Data { get; }
        public FamilySide Side { get; }
        public int CurrentHealth { get; private set; }
        public int Infection { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        private readonly List<ActiveBuff> buffs = new List<ActiveBuff>();
        public IReadOnlyList<ActiveBuff> Buffs => buffs;

        public event System.Action<FamilyMember> OnStatsChanged;
        public event System.Action<FamilyMember> OnDeath;

        public FamilyMember(FamilyMemberData data, FamilySide side)
        {
            Data = data;
            Side = side;
            CurrentHealth = data.maxHealth;
            Infection = 0;
        }

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
            Infection += amount;
            OnStatsChanged?.Invoke(this);
        }

        public void ApplyHeal(int amount)
        {
            if (amount <= 0 || IsDead) return;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, Data.maxHealth);
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
            OnStatsChanged?.Invoke(this);
        }

        public void TickInfection(int damagePerStack)
        {
            if (Infection > 0) ApplyDamage(Infection * damagePerStack);
        }

        private void CheckDeath()
        {
            if (IsDead) OnDeath?.Invoke(this);
        }
    }
}