using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that holds an achievement at runtime, tracking claimed tiers.
    /// </summary>
    [Serializable]
    public class AchievementHolder : HolderBase<Achievement>, IUnlockableHolder
    {
        public bool isClaimed = false;
        public bool isUnlocked = false;
        public int claimedAmount = 0;

        public IUnlockableHolder iUnlockable => this;
        public Action<IUnlockableHolder> OnUnlocked { get; set; }

        // IUnlockableHolder
        public IUnlockable UnlockableItem => achievement;
        public bool AlreadyUnlocked { get => IsFullyClaimed || isUnlocked; set => isUnlocked = value; }

        public Achievement achievement => base.Item;
        public int ClaimedAmount => claimedAmount <= 0 && isClaimed ? 1 : Mathf.Max(0, claimedAmount);
        public int TotalClaimAmount => achievement != null ? achievement.TotalClaimAmount : 1;
        public bool IsFullyClaimed => ClaimedAmount >= TotalClaimAmount;
        public int CurrentTierIndex => Mathf.Clamp(ClaimedAmount, 0, Mathf.Max(0, TotalClaimAmount - 1));
        public int NextClaimNumber => Mathf.Min(ClaimedAmount + 1, TotalClaimAmount);
        public BigNumber CurrentClaimMultiplier => achievement != null ? achievement.GetClaimTierMultiplier(CurrentTierIndex) : 1;
        public bool CanClaimRewards => !IsFullyClaimed && IsUnlocked();

        public AchievementHolder(Achievement achievement_) : base(achievement_)
        {
        }

        public new void Load()
        {
            base.Load();
            SyncClaimedState();
        }

        public List<Lock> GetCurrentTierLocks()
        {
            return achievement != null ? achievement.GetLocksForClaimTier(CurrentTierIndex) : new List<Lock>();
        }

        public bool IsCurrentTierUnlocked()
        {
            if (achievement == null) return false;
            if (IsFullyClaimed) return true;
            return achievement.IsClaimTierUnlocked(ClaimedAmount);
        }

        public bool IsUnlocked()
        {
            SyncClaimedState();

            if (achievement == null) return false;
            if (IsFullyClaimed) return true;
            if (isUnlocked) return true;

            if (!IsCurrentTierUnlocked()) return false;

            isUnlocked = true;
            OnUnlocked?.Invoke(this);
            return true;
        }

        public bool TryClaimRewards(out List<Output> outputs)
        {
            outputs = null;
            if (!CanClaimRewards) return false;

            outputs = RewardCalculator.ApplyOutputs(achievement.rewardTables, 1, RewardCalculator.CreateContext(SourceType.Milestone));
            claimedAmount = ClaimedAmount + 1;
            isUnlocked = false;
            SyncClaimedState();

            if (AchievementsManager.Instance)
                AchievementsManager.Instance.OnAchievementChanged?.Invoke(achievement);

            return true;
        }

        private void SyncClaimedState()
        {
            bool migratedLegacyClaim = isClaimed && claimedAmount <= 0;
            if (migratedLegacyClaim)
                claimedAmount = 1;

            claimedAmount = Mathf.Max(0, claimedAmount);
            isClaimed = IsFullyClaimed;

            if (isClaimed)
                isUnlocked = true;
            else if (migratedLegacyClaim)
                isUnlocked = false;
        }

        public override string ToString()
        {
            return $"{achievement.GetDisplayName()}: claimed:{ClaimedAmount}/{TotalClaimAmount}";
        }
    }
}
