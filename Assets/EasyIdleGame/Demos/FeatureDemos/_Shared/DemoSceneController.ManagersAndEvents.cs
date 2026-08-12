using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EasyIdleGame.Demos
{
    public abstract partial class DemoSceneController
    {
        protected void EnsureManagers()
        {
            EnsureManager<BusinessesManager>();
            EnsureManager<CurrencyManager>();
            EnsureManager<BoostsManager>();
            EnsureManager<ManagersManager>();
            EnsureManager<UpgradesManager>();
            EnsureManager<DailyRewardsManager>();
            EnsureManager<PrestigeManager>();
            EnsureManager<LocationsManager>();
            EnsureManager<OffersManager>();
            EnsureManager<ShopManager>();
            EnsureManager<AchievementsManager>();
            EnsureManager<PlayerStats>();
        }

        protected void EnsureManager<T>() where T : Component
        {
            if (FindObjectOfType<T>() == null)
            {
                gameObject.AddComponent<T>();
            }
        }

        protected void ConfigureManagers()
        {
            if (BusinessesManager.Instance)
            {
                BusinessesManager.Instance.buyAmounts = new[]
                {
                    new BuyAmount(1, BuyAmount.Type.digit),
                    new BuyAmount(5, BuyAmount.Type.digit),
                    new BuyAmount(50, BuyAmount.Type.percent)
                };
            }

            if (PlayerStats.Instance)
            {
                PlayerStats.Instance.firstLevelXp = 100;
                PlayerStats.Instance.levelIncrementMultiplier = 2;
                PlayerStats.Instance.ResetLevel();
            }
        }

        protected void RegisterEvents()
        {
            EconomyChangedEvents.ResetSaveWorthyDebounce();
            EconomyChangedEvents.OnSaveWorthyStateChanged.RemoveListener(OnSaveWorthyStateChanged);
            EconomyChangedEvents.OnSaveWorthyStateChanged.AddListener(OnSaveWorthyStateChanged);

            if (ShopManager.Instance)
            {
                ShopManager.Instance.OnShopItemRewardsGranted.RemoveListener(OnShopRewardsGranted);
                ShopManager.Instance.OnShopItemRewardsGranted.AddListener(OnShopRewardsGranted);
            }

            if (BoostsManager.Instance)
            {
                BoostsManager.Instance.OnBoostUsed.RemoveListener(OnBoostUsed);
                BoostsManager.Instance.OnBoostUsed.AddListener(OnBoostUsed);
                BoostsManager.Instance.OnTimeSkip.RemoveListener(OnTimeSkip);
                BoostsManager.Instance.OnTimeSkip.AddListener(OnTimeSkip);
            }

            if (UpgradesManager.Instance)
            {
                UpgradesManager.Instance.OnUpgradeAmountChanged.RemoveListener(OnUpgradeChanged);
                UpgradesManager.Instance.OnUpgradeAmountChanged.AddListener(OnUpgradeChanged);
                UpgradesManager.Instance.OnUpgradeApplied.RemoveListener(OnUpgradeChanged);
                UpgradesManager.Instance.OnUpgradeApplied.AddListener(OnUpgradeChanged);
            }

            if (LocationsManager.Instance)
            {
                LocationsManager.Instance.OnLocationChanged.RemoveListener(OnLocationChanged);
                LocationsManager.Instance.OnLocationChanged.AddListener(OnLocationChanged);
            }

            ProfitCalculator.OnProfitApplied.RemoveListener(OnProfitApplied);
            ProfitCalculator.OnProfitApplied.AddListener(OnProfitApplied);

            if (DailyRewardsManager.Instance)
            {
                DailyRewardsManager.Instance.OnDailyRewardClaimed.RemoveListener(OnDailyRewardClaimed);
                DailyRewardsManager.Instance.OnDailyRewardClaimed.AddListener(OnDailyRewardClaimed);
            }

            if (AchievementsManager.Instance)
            {
                AchievementsManager.Instance.OnAchievementClaimed.RemoveListener(OnAchievementClaimed);
                AchievementsManager.Instance.OnAchievementClaimed.AddListener(OnAchievementClaimed);
                AchievementsManager.Instance.OnAchievementChanged.RemoveListener(OnAchievementChanged);
                AchievementsManager.Instance.OnAchievementChanged.AddListener(OnAchievementChanged);
            }

            if (PlayerStats.Instance)
            {
                PlayerStats.Instance.OnXpAdded.RemoveListener(OnXpAdded);
                PlayerStats.Instance.OnXpAdded.AddListener(OnXpAdded);
                PlayerStats.Instance.OnLevelUpEvent.RemoveListener(OnPlayerLevelUp);
                PlayerStats.Instance.OnLevelUpEvent.AddListener(OnPlayerLevelUp);
            }

            if (PrestigeManager.Instance)
            {
                PrestigeManager.Instance.OnPrestige.RemoveListener(OnPrestige);
                PrestigeManager.Instance.OnPrestige.AddListener(OnPrestige);
            }

            if (ManagersManager.Instance)
            {
                ManagersManager.Instance.OnManagerAmountChanged.RemoveListener(OnManagerChanged);
                ManagersManager.Instance.OnManagerAmountChanged.AddListener(OnManagerChanged);
                ManagersManager.Instance.OnManagerBought.RemoveListener(OnManagerChanged);
                ManagersManager.Instance.OnManagerBought.AddListener(OnManagerChanged);
            }

            if (OffersManager.Instance)
            {
                OffersManager.Instance.OnOfferBecameAvailable.RemoveListener(OnOfferBecameAvailable);
                OffersManager.Instance.OnOfferBecameAvailable.AddListener(OnOfferBecameAvailable);
                OffersManager.Instance.OnOfferExpired.RemoveListener(OnOfferExpired);
                OffersManager.Instance.OnOfferExpired.AddListener(OnOfferExpired);
            }
        }

        protected void OnSaveWorthyStateChanged(string source)
        {
            LastSaveEventSource = source;
            saveEventCount++;
            saveEventSources.Insert(0, source);
            while (saveEventSources.Count > 12) saveEventSources.RemoveAt(saveEventSources.Count - 1);
        }

        protected void OnShopRewardsGranted(ShopItem item, SourceType source, List<Output> outputs)
        {
            LastShopRewardResult = $"{item.metadata.name}, source={source}, outputs={DescribeOutputs(outputs)}";
        }

        protected void OnBoostUsed(Boost boost)
        {
            LastBoostResult = $"Used {boost.boostName}.";
        }

        protected void OnUpgradeChanged(Upgrade upgrade)
        {
            LastUpgradeResult = $"Changed {upgrade.upgradeName}.";
        }

        protected void OnLocationChanged(Location location)
        {
            LastLocationResult = location ? $"Active location is {location.metadata.name}." : "Active location cleared.";
        }

        protected void OnProfitApplied(ProfitApplicationSummary summary)
        {
            LastProfitSummary = summary;
            LastProfitResult = $"Applied {summary.SourceProfit.ElapsedTime()}: {DescribeSummary(summary)}";
        }

        protected void OnTimeSkip(ProfitData profit)
        {
            LastCalculatedProfit = profit;
            LastTimeSkipResult = $"Skipped {profit.ElapsedTime()}: {DescribeProfit(profit)}";
        }

        protected void OnDailyRewardClaimed(int day, List<Output> outputs)
        {
            LastDailyRewardResult = $"Claimed day {day}: {DescribeOutputs(outputs)}";
        }

        protected void OnAchievementClaimed(Achievement achievement)
        {
            LastAchievementResult = $"Claimed {achievement.metadata.name}.";
        }

        protected void OnAchievementChanged(Achievement achievement)
        {
            LastAchievementResult = $"Changed {achievement.metadata.name}.";
        }

        protected void OnXpAdded(BigNumber amount)
        {
            LastPlayerStatsResult = $"XP added: {amount}.";
        }

        protected void OnPlayerLevelUp(int level)
        {
            LastPlayerStatsResult = $"Player reached level {level}.";
        }

        protected void OnPrestige()
        {
            LastPrestigeResult = $"Prestige complete; count={PrestigeManager.Instance.prestiges.prestigeCount}.";
        }

        protected void OnManagerChanged(Manager manager)
        {
            ManagerHolder holder = ManagersManager.Instance.GetOrAddHolder(manager);
            LastManagerResult = $"{manager.managerName}: owned={holder.totalCount}, level={holder.level:0}.";
            RefreshDashboard();
        }

        protected void OnOfferBecameAvailable(OfferPoolItem offer)
        {
            LastOfferResult = offer == null || offer.item == null ? "Offer became available." : $"Offer available: {offer.item.metadata.name}.";
            RefreshDashboard();
        }

        protected void OnOfferExpired(OfferPoolItem offer)
        {
            LastOfferResult = offer == null || offer.item == null ? "Offer expired." : $"Offer expired: {offer.item.metadata.name}.";
            RefreshDashboard();
        }

    }
}

