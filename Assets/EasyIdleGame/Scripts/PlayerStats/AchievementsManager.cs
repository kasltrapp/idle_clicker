using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame
{
    [AddComponentMenu("EasyIdleGame/Managers/Achievements Manager")]
    public class AchievementsManager : ManagerHolderBase<AchievementsManager, AchievementHolder, Achievement>,
        IUnlockableHolderManager<AchievementHolder, Achievement>
    {
        public IUnlockableHolderManager<AchievementHolder, Achievement> iUnlockable => this;

        public UnityEvent<Achievement> OnItemUnlocked { get; } = new UnityEvent<Achievement>();
        public UnityEvent<AchievementHolder> OnHolderUnlockedEvent { get; } = new UnityEvent<AchievementHolder>();

        [Tooltip("Invoked after an achievement is claimed and its rewards are applied.")]
        public UnityEvent<Achievement> OnAchievementClaimed = new UnityEvent<Achievement>();

        [Tooltip("Invoked when an achievement's unlock or claimed state changes.")]
        public UnityEvent<Achievement> OnAchievementChanged = new UnityEvent<Achievement>();

        public override AchievementHolder GetOrAddHolder(Achievement item)
        {
            if (item == null) throw new Exception("Achievement is null");

            AchievementHolder h = GetHolder(item);
            if (h == null)
            {
                holders.Add(h = new AchievementHolder(item));
                OnHolderAdded?.Invoke(h);
            }
            return h;
        }

        protected override void NotifyHolderReset(AchievementHolder holder)
        {
            if (holder?.achievement != null)
                OnAchievementChanged.Invoke(holder.achievement);
        }
        public bool TryClaimAchievement(Achievement achievement, out List<Output> outputs)
        {
            outputs = null;
            AchievementHolder holder = GetOrAddHolder(achievement);

            if (!holder.CanClaimRewards) return false;

            bool success = holder.TryClaimRewards(out outputs);
            if (!success) return false;

            OnAchievementClaimed.Invoke(achievement);
            EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(AchievementsManager));

            Log($"Claimed achievement: {achievement.GetDisplayName()} ({holder.ClaimedAmount}/{holder.TotalClaimAmount})", LogCategory.Verbose);
            return true;
        }

        public void CheckAutoUnlocks()
        {
            foreach (AchievementHolder holder in holders)
            {
                if (holder == null || holder.IsFullyClaimed) continue;
                if (holder.AlreadyUnlocked || !holder.IsCurrentTierUnlocked()) continue;

                holder.AlreadyUnlocked = true;
                holder.OnUnlocked?.Invoke(holder);
                OnItemUnlocked?.Invoke(holder.achievement);
                OnHolderUnlockedEvent?.Invoke(holder);
                OnAchievementChanged?.Invoke(holder.achievement);
            }
        }

        private float _timeSinceLastCheck = 0f;
        private void Update()
        {
            _timeSinceLastCheck += Time.deltaTime;
            if (_timeSinceLastCheck >= 0.2f)
            {
                _timeSinceLastCheck = 0f;
                CheckAutoUnlocks();
                foreach (AchievementHolder holder in holders)
                {
                    int claimsThisUpdate = 0;
                    while (holder.achievement.autoClaim && holder.CanClaimRewards && claimsThisUpdate < holder.TotalClaimAmount)
                    {
                        TryClaimAchievement(holder.achievement, out _);
                        claimsThisUpdate++;
                    }
                }
            }
        }
    }
}
