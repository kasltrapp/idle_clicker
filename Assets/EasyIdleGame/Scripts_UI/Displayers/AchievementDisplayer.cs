using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EasyIdleGame.UI
{
    [AddComponentMenu("EasyIdleGame/Displayers/Achievement Displayer")]
    public class AchievementDisplayer : BaseDisplayer
    {
        public Achievement achievement;
        public TMP_Text titleText;
        public TMP_Text descriptionText;
        public Image iconImage;
        public Slider progressSlider;
        public TMP_Text progressText;
        public TMP_Text rewardsText;
        public ButtonHolder claimButton;
        public GameObject claimedCheckmark;

        public override void Initialize()
        {
            base.Initialize();
            if (claimButton != null && claimButton.button)
            {
                claimButton.button.onClick.AddListener(() => 
                {
                    AchievementsManager.Instance.TryClaimAchievement(achievement, out _);
                });
            }
        }

        public override void Redraw()
        {
            base.Redraw();

            if (!achievement) return;

            AchievementHolder holder = AchievementsManager.Instance ? AchievementsManager.Instance.GetOrAddHolder(achievement) : null;
            List<Lock> currentLocks = holder != null ? holder.GetCurrentTierLocks() : achievement.GetLocksForClaimTier(0);
            Lock firstLock = currentLocks.Count > 0 ? currentLocks[0] : null;
            if (firstLock != null)
            {
                BigNumber amount = GetProgressAmount(firstLock);

                SetSliderValues(progressSlider, 0, (float)firstLock.inputAmount.ToDouble(), amount.ToDouble());
                UpdateText(progressText, $"{amount}/{firstLock.inputAmount}");
            }

            bool isUnlocked = holder != null ? holder.iUnlockable.IsUnlocked() : achievement.IsCurrentlyUnlocked();
            bool isClaimed = holder != null && holder.IsFullyClaimed;
            bool canClaim = holder != null ? holder.CanClaimRewards : isUnlocked;

            EnableComponent(claimButton?.button?.gameObject, canClaim);
            EnableComponent(claimedCheckmark, isClaimed);
        }

        public override void Redraw_Default()
        {
            base.Redraw_Default();

            if (!achievement) return;

            UpdateText(titleText, achievement.GetDisplayName());
            UpdateText(descriptionText, achievement.Metadata.description);
            UpdateImage(iconImage, achievement.Metadata.icon);

            if (rewardsText != null)
                UpdateText(rewardsText, achievement.rewardTables.GetAverageOutputs(1).CustomToString());
        }

        private BigNumber GetProgressAmount(Lock lockData)
        {
            if (lockData.businessLock != null && BusinessesManager.Instance)
            {
                BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(lockData.businessLock);
                return lockData.amountType switch
                {
                    LockAmountType.TotalAmount => holder.TotalAmount,
                    LockAmountType.BoughtAmount => holder.BoughtAmount,
                    LockAmountType.MaxAmount => holder.GetMaxAmountConstrain(),
                    _ => holder.Amount,
                };
            }

            if (lockData.currencyLock != null && CurrencyManager.Instance)
            {
                CurrencyHolder holder = CurrencyManager.Instance.GetOrAddHolder(lockData.currencyLock);
                return lockData.amountType switch
                {
                    LockAmountType.TotalAmount => holder.TotalAmount,
                    LockAmountType.MaxAmount => holder.GetMaxCurrencyConstrain(lockData.currencyLock),
                    _ => holder.amount,
                };
            }

            if (lockData.playerLevelLock && PlayerStats.Instance)
                return PlayerStats.Instance.GetPlayerLevel();

            if (lockData.customStatLock != null && PlayerStats.Instance)
                return PlayerStats.Instance.GetStat(lockData.customStatLock);

            if (lockData.prestigeLock && PrestigeManager.Instance)
                return PrestigeManager.Instance.prestiges.prestigeCount;

            if (lockData.businessGroupLock != null && BusinessesManager.Instance)
                return BusinessesManager.Instance.GetBusinessGroupAmount(lockData.businessGroupLock, lockData.amountType);

            if (lockData.currencyGroupLock != null && CurrencyManager.Instance)
                return CurrencyManager.Instance.GetCurrencyGroupAmount(lockData.currencyGroupLock, lockData.amountType);

            if (lockData.upgradeGroupLock != null && UpgradesManager.Instance)
                return UpgradesManager.Instance.GetUpgradeGroupAmount(lockData.upgradeGroupLock, lockData.amountType);

            return lockData.IsUnlocked() ? lockData.inputAmount : 0;
        }
    }
}
