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
        private static readonly string[] ResourceCategories =
        {
            "business_group",
            "currency_group",
            "shop_item",
            "custom_stat",
            "currency",
            "business",
            "boost",
            "upgrade",
            "recipe",
            "manager",
            "location",
            "reward",
            "achievement"
        };

        protected string GetBusinessCardTitle(Business business)
        {
            if (business is BusinessWrapper)
            {
                return $"{business.businessName} (wrapper)";
            }

            return business.businessName;
        }

        protected string GetBusinessRuntimeDisplayName(BusinessHolder holder)
        {
            Metadata metadata = GetBusinessRuntimeMetadata(holder);
            if (!string.IsNullOrWhiteSpace(metadata?.name)) return metadata.name;
            return holder?.business ? holder.business.businessName : "Business";
        }

        protected Metadata GetBusinessRuntimeMetadata(BusinessHolder holder)
        {
            if (holder == null || holder.business == null) return null;

            Metadata baseMetadata = holder.business.Metadata;
            BusinessUpgradeLevel level = holder.iUpgradable.GetCurrentUpgradeLevel() as BusinessUpgradeLevel;
            if (level == null) return baseMetadata;

            Metadata levelMetadata = level.Metadata;
            if (levelMetadata == null) return baseMetadata;

            return new Metadata
            {
                name = string.IsNullOrWhiteSpace(levelMetadata.name) ? baseMetadata?.name : levelMetadata.name,
                icon = levelMetadata.icon ? levelMetadata.icon : baseMetadata?.icon,
                iconString = string.IsNullOrWhiteSpace(levelMetadata.iconString) ? baseMetadata?.iconString : levelMetadata.iconString,
                model = levelMetadata.model ? levelMetadata.model : baseMetadata?.model,
                description = string.IsNullOrWhiteSpace(levelMetadata.description) ? baseMetadata?.description : levelMetadata.description
            };
        }

        protected string GetAssetRoleName(string assetName)
        {
            string roleName = StripResourcePrefixAndCategory(assetName);
            return roleName.Replace("_", " ");
        }

        protected T Load<T>(string name) where T : UnityEngine.Object
        {
            T asset = LoadOptional<T>(name);
            if (asset == null)
            {
                throw new InvalidOperationException($"Missing demo resource '{BuildResourceName<T>(name)}' of type {typeof(T).Name}.");
            }

            return asset;
        }

        protected T LoadOptional<T>(string name) where T : UnityEngine.Object
        {
            T asset = Resources.Load<T>(BuildResourceName<T>(name));
            return asset != null ? asset : Resources.Load<T>($"{resourcePrefix}_{name}");
        }

        protected List<T> LoadAllPrefixed<T>() where T : UnityEngine.Object
        {
            return Resources.LoadAll<T>("")
                .Where(asset => asset != null && IsResourceInCurrentDemo(asset))
                .OrderBy(GetResourceNameWithPrefix)
                .ToList();
        }

        protected bool ResourceNameEquals(UnityEngine.Object asset, string name)
        {
            return string.Equals(GetResourceNameWithoutPrefix(asset), name, StringComparison.Ordinal);
        }

        protected bool IsResourceInCurrentDemo(UnityEngine.Object asset)
        {
            string prefix = string.IsNullOrWhiteSpace(resourcePrefix) ? string.Empty : resourcePrefix + "_";
            return !string.IsNullOrEmpty(prefix)
                && GetResourceNameWithPrefix(asset).StartsWith(prefix, StringComparison.Ordinal);
        }

        private string BuildResourceName<T>(string name) where T : UnityEngine.Object
        {
            string category = GetResourceCategory(typeof(T));
            return string.IsNullOrWhiteSpace(category)
                ? $"{resourcePrefix}_{name}"
                : $"{resourcePrefix}_{category}_{name}";
        }

        private static string GetResourceCategory(Type type)
        {
            if (typeof(Currency).IsAssignableFrom(type)) return "currency";
            if (typeof(Business).IsAssignableFrom(type)) return "business";
            if (typeof(Boost).IsAssignableFrom(type)) return "boost";
            if (typeof(Upgrade).IsAssignableFrom(type)) return "upgrade";
            if (typeof(BusinessGroup).IsAssignableFrom(type)) return "business_group";
            if (typeof(CurrencyGroup).IsAssignableFrom(type)) return "currency_group";
            if (typeof(BusinessMergeRecipe).IsAssignableFrom(type)) return "recipe";
            if (typeof(Manager).IsAssignableFrom(type)) return "manager";
            if (typeof(Location).IsAssignableFrom(type)) return "location";
            if (typeof(ShopItem).IsAssignableFrom(type)) return "shop_item";
            if (typeof(Reward).IsAssignableFrom(type)) return "reward";
            if (typeof(Achievement).IsAssignableFrom(type)) return "achievement";
            if (typeof(CustomStat).IsAssignableFrom(type)) return "custom_stat";
            return string.Empty;
        }

        protected string StripResourcePrefixAndCategory(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName)) return resourceName;

            string prefix = string.IsNullOrWhiteSpace(resourcePrefix) ? string.Empty : resourcePrefix + "_";
            if (!string.IsNullOrEmpty(prefix) && resourceName.StartsWith(prefix, StringComparison.Ordinal))
            {
                string withoutPrefix = resourceName.Substring(prefix.Length);
                foreach (string category in ResourceCategories)
                {
                    string categoryPrefix = category + "_";
                    if (withoutPrefix.StartsWith(categoryPrefix, StringComparison.Ordinal))
                    {
                        return withoutPrefix.Substring(categoryPrefix.Length);
                    }
                }

                return withoutPrefix;
            }

            return resourceName;
        }

        protected string DescribeCurrencies(IEnumerable<Currency> currencies)
        {
            List<string> parts = new List<string>();

            foreach (Currency currency in currencies)
            {
                BigNumber amount = CurrencyManager.Instance ? CurrencyManager.Instance.GetCurrencyAmount(currency) : 0;
                BigNumber max = CurrencyManager.Instance ? CurrencyManager.Instance.GetMaxCurrencyConstrain(currency) : currency.maxAmount;
                parts.Add(IsUnlimited(max) ? $"{currency.currencyName} {amount}" : $"{currency.currencyName} {amount}/{DescribeLimit(max)}");
            }

            return parts.Count == 0 ? "none" : string.Join(", ", parts);
        }

        protected string DescribeInputs(List<Input> inputs)
        {
            if (inputs == null || inputs.Count == 0) return "none";

            return string.Join(", ", inputs.Select(input =>
            {
                if (input == null) return "null";
                if (input.currencyInput != null) return $"{input.currencyInput.currencyName} x{input.inputAmount}";
                if (input.businessInput != null) return $"{input.businessInput.businessName} x{input.inputAmount}";
                return $"Empty input x{input.inputAmount}";
            }));
        }

        protected string DescribeInputProgress(List<Input> inputs)
        {
            if (inputs == null || inputs.Count == 0) return "none";

            return string.Join(", ", inputs.Select(input =>
            {
                if (input == null) return "null";

                BigNumber required = input.inputAmount;
                if (input.currencyInput != null)
                {
                    BigNumber current = CurrencyManager.Instance ? CurrencyManager.Instance.GetCurrencyAmount(input.currencyInput) : 0;
                    return $"{input.currencyInput.currencyName} {current}/{required}";
                }

                if (input.businessInput != null)
                {
                    BusinessHolder holder = BusinessesManager.Instance ? BusinessesManager.Instance.GetOrAddHolder(input.businessInput) : null;
                    BigNumber current = holder == null ? 0 : holder.amount;
                    return $"{input.businessInput.businessName} {current}/{required}";
                }

                return $"Empty input 0/{required}";
            }));
        }

        protected bool IsUnlimited(BigNumber limit)
        {
            return limit < 0 || limit >= BigNumber.MaxValue || limit.Exponent > 1000000;
        }

        protected string DescribeLimit(BigNumber limit)
        {
            return IsUnlimited(limit) ? "unlimited" : limit.ToString();
        }

        protected string DescribeBuyCost(IBuyableHolder holder, BigNumber amount)
        {
            if (holder == null || amount <= 0) return "none";
            return DescribeInputs(holder.GetRealCostToInputs(amount));
        }

        protected Color GetBuyCostColor(IBuyableHolder holder, BigNumber amount)
        {
            if (holder == null || amount <= 0) return TextColor;
            return GetInputCostColor(holder.GetRealCostToInputs(amount));
        }

        protected Color GetInputCostColor(List<Input> inputs)
        {
            return DescribeInputShortfalls(inputs) == "none" ? TextColor : ErrorColor;
        }

        protected bool CanBuyAmount(IBuyableHolder holder, BigNumber amount)
        {
            return holder != null && amount > 0 && holder.CanBuy(amount);
        }

        protected string DescribeCanBuy(IBuyableHolder holder, BigNumber amount)
        {
            if (holder == null) return "no holder";
            if (amount <= 0)
            {
                if (holder is IUnlockableHolder unlockableHolder && !unlockableHolder.IsUnlocked())
                {
                    return "no, locked";
                }

                if (holder.Maxed)
                {
                    return "no, maxed";
                }

                return "no target";
            }
            return holder.CanBuy(amount) ? "yes" : "no";
        }

        protected string DescribeBuyAvailability(IBuyableHolder holder, BigNumber amount)
        {
            List<string> blockers = GetBuyBlockers(holder, amount);
            if (blockers.Count == 0)
            {
                return $"ready, will spend {DescribeBuyCost(holder, amount)}";
            }

            return string.Join("; ", blockers);
        }

        protected List<string> GetBuyBlockers(IBuyableHolder holder, BigNumber amount)
        {
            List<string> blockers = new List<string>();
            if (holder == null)
            {
                blockers.Add("no runtime holder exists for this object");
                return blockers;
            }

            if (holder is IUnlockableHolder unlockableHolder && !unlockableHolder.IsUnlocked())
            {
                blockers.Add($"locked: {DescribeBlockingLocks(unlockableHolder.UnlockableItem)}");
            }

            if (holder.Maxed)
            {
                blockers.Add($"maxed: total {holder.TotalAmount} has reached max buyable {DescribeLimit(holder.holdedItem.MaxBuyableAmount)}");
            }
            else if (holder.holdedItem.ExceedsBuyableAmount(holder.TotalAmount + amount))
            {
                blockers.Add($"would exceed max buyable {DescribeLimit(holder.holdedItem.MaxBuyableAmount)}; current total is {holder.TotalAmount}");
            }

            if (holder is BusinessHolder businessHolder)
            {
                BigNumber remainingCapacity = businessHolder.GetRemainingAmountConstrain();
                if (remainingCapacity <= 0)
                {
                    blockers.Add($"capacity reached: {DescribeBusinessCapacityLimits(businessHolder)}");
                }
                else if (amount > remainingCapacity)
                {
                    blockers.Add($"only {remainingCapacity} more fit before capacity is full: {DescribeBusinessCapacityLimits(businessHolder)}");
                }

                BigNumber maxBuyable = businessHolder.GetMaxBuyableAmountConstrain();
                if (maxBuyable < BigNumber.MaxValue)
                {
                    BigNumber remainingBuyable = maxBuyable - businessHolder.amount;
                    if (remainingBuyable <= 0)
                    {
                        blockers.Add($"direct buy cap reached: owned {businessHolder.amount}/{DescribeLimit(maxBuyable)}");
                    }
                    else if (amount > remainingBuyable)
                    {
                        blockers.Add($"only {remainingBuyable} more fit before the direct buy cap {DescribeLimit(maxBuyable)}");
                    }
                }
            }

            if (amount <= 0)
            {
                if (blockers.Count == 0)
                {
                    blockers.Add("buy amount is 0 because the current buy setting cannot resolve a valid target for this object");
                }

                return blockers;
            }

            string inputShortfalls = DescribeInputShortfalls(holder.GetRealCostToInputs(amount));
            if (inputShortfalls != "none")
            {
                blockers.Add($"not enough inputs: {inputShortfalls}");
            }

            if (blockers.Count == 0 && !holder.CanBuy(amount))
            {
                blockers.Add("the package CanBuy check returned false; inspect locks, caps, and cost configuration");
            }

            return blockers;
        }

        protected string DescribeInputShortfalls(List<Input> requiredInputs)
        {
            if (requiredInputs == null || requiredInputs.Count == 0) return "none";

            List<string> parts = new List<string>();
            foreach (Input input in requiredInputs)
            {
                if (input == null) continue;

                if (input.currencyInput != null)
                {
                    BigNumber current = CurrencyManager.Instance ? CurrencyManager.Instance.GetCurrencyAmount(input.currencyInput) : 0;
                    if (current < input.inputAmount)
                    {
                        parts.Add($"{input.currencyInput.currencyName} needs {input.inputAmount}, have {current}, missing {input.inputAmount - current}");
                    }
                }

                if (input.businessInput != null)
                {
                    BusinessHolder holder = BusinessesManager.Instance ? BusinessesManager.Instance.GetOrAddHolder(input.businessInput) : null;
                    BigNumber current = holder == null ? 0 : holder.amount;
                    if (current < input.inputAmount)
                    {
                        parts.Add($"{input.businessInput.businessName} needs {input.inputAmount}, have {current}, missing {input.inputAmount - current}");
                    }
                }
            }

            return parts.Count == 0 ? "none" : string.Join("; ", parts);
        }

        protected string DescribeBlockingLocks(IUnlockable unlockable)
        {
            if (unlockable == null) return "no lock details available";

            List<string> blockers = new List<string>();
            if (unlockable.Locks != null)
            {
                blockers.AddRange(unlockable.Locks.Where(lockItem => lockItem != null && !lockItem.IsUnlocked()).Select(lockItem => DescribeLock(lockItem, false)));
            }

            if (unlockable.UpperLocks != null)
            {
                blockers.AddRange(unlockable.UpperLocks.Where(lockItem => lockItem != null && !lockItem.IsUpperUnlocked()).Select(lockItem => DescribeLock(lockItem, true)));
            }

            return blockers.Count == 0 ? "lock exists but no specific blocker was found" : string.Join("; ", blockers);
        }

        protected string DescribeStartProductionAvailability(string businessName, BigNumber amount)
        {
            Business business = LoadOptional<Business>(businessName);
            if (business == null) return "business asset is missing";
            if (amount <= 0) return "requested production amount is 0";

            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);
            if (business.ProductionRoundMode != ProductionRoundMode.ConcurrentRounds && holder.ActiveProductions.Count > 0)
            {
                return $"single production round is active; finish it before starting another round";
            }

            BigNumber idleOwned = holder.amount - holder.CurrentlyProducing;
            if (idleOwned <= 0)
            {
                return $"no idle copies are available; owned {holder.amount}, currently producing {holder.CurrentlyProducing}";
            }

            if (idleOwned < amount)
            {
                return $"needs {amount} idle copies, but only {idleOwned} are idle; owned {holder.amount}, currently producing {holder.CurrentlyProducing}";
            }

            BigNumber affordable = business.iProducable.GetAffordableProductionAmount(amount);
            if (affordable <= 0)
            {
                string shortfalls = DescribeInputShortfalls(business.iProducable.GetProductionCost(amount));
                return shortfalls == "none"
                    ? "production cost check returned 0 affordable copies"
                    : $"production inputs are missing: {shortfalls}";
            }

            if (affordable < amount)
            {
                return $"can only afford {affordable}/{amount} because production inputs are limited: {DescribeInputShortfalls(business.iProducable.GetProductionCost(amount))}";
            }

            return $"ready, will start {amount} idle copy/copies";
        }

        protected string DescribeStartAllProductionAvailability(string businessName)
        {
            Business business = LoadOptional<Business>(businessName);
            if (business == null) return "business asset is missing";

            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);
            BigNumber idleOwned = holder.amount - holder.CurrentlyProducing;
            if (idleOwned <= 0)
            {
                return $"no idle copies are available; owned {holder.amount}, currently producing {holder.CurrentlyProducing}";
            }

            return DescribeStartProductionAvailability(businessName, idleOwned);
        }

        protected string DescribeFinishProductionAvailability(string businessName)
        {
            return HasActiveBusinessProduction(businessName) ? "ready, will finish all active rounds on this business" : "no active production round exists yet";
        }

        protected string DescribeClaimAvailability(string businessName)
        {
            Business business = LoadOptional<Business>(businessName);
            if (business == null) return "business asset is missing";

            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);
            if (CanAutoCollectOutput(holder)) return "outputs are auto-collected, so there is no manual claim step";
            return HasStoredBusinessOutputs(businessName) ? "ready, will move stored outputs into inventory" : "no stored output yet; start production and wait until it finishes";
        }

        protected string DescribeBoostUseAvailability(Boost boost, BigNumber amount)
        {
            if (boost == null) return "boost asset is missing";
            BigNumber inventory = BoostsManager.Instance ? BoostsManager.Instance.GetBoostAmountInInventory(boost) : 0;
            return inventory >= amount ? $"ready, will consume {amount} from inventory" : $"inventory is {inventory}, needs {amount}; press Add +1 or earn this boost first";
        }

        protected string DescribeActiveBoostTimers()
        {
            if (!BoostsManager.Instance || BoostsManager.Instance.activeBoosts.Count == 0) return "none";

            return string.Join(", ", BoostsManager.Instance.activeBoosts
                .Where(active => active != null && active.boost != null)
                .Select(active => $"{active.boost.boostName} {Math.Max(0, active.timeLeft):0.0}s"));
        }

        protected bool CanBuyBusinessAmount(string businessName, BigNumber amount)
        {
            Business business = LoadOptional<Business>(businessName);
            if (business == null || amount <= 0 || !BusinessesManager.Instance) return false;

            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);
            return CanBuyAmount(holder.iBuyable, amount);
        }

        protected string DescribeBusinessBuyAvailability(string businessName, BigNumber amount)
        {
            Business business = LoadOptional<Business>(businessName);
            if (business == null || !BusinessesManager.Instance) return "business asset is missing";

            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);
            return DescribeBuyAvailability(holder.iBuyable, amount);
        }

        protected bool CanBuyTargetBusinessAmount(string businessName)
        {
            Business business = LoadOptional<Business>(businessName);
            if (business == null || !BusinessesManager.Instance) return false;

            BigNumber amount = BusinessesManager.Instance.GetTargetBusinessBuyAmount(business);
            return CanBuyAmount(BusinessesManager.Instance.GetOrAddHolder(business).iBuyable, amount);
        }

        protected string DescribeTargetBusinessBuyAvailability(string businessName)
        {
            Business business = LoadOptional<Business>(businessName);
            if (business == null || !BusinessesManager.Instance) return "business asset is missing";

            BigNumber amount = BusinessesManager.Instance.GetTargetBusinessBuyAmount(business);
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);
            return DescribeBuyAvailability(holder.iBuyable, amount);
        }

        protected string DescribeBusinessBuyTarget(Business business, BigNumber target)
        {
            if (business == null) return "missing business";
            BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(business);
            if (!holder.iUnlockable.IsUnlocked()) return "locked by requirements";
            if (holder.GetRemainingAmountConstrain() <= 0) return "capacity full";
            if (holder.GetRemainingBuyableAmountConstrain() <= 0) return "direct buy cap reached";
            return target > 0 ? target.ToString() : "none";
        }

        protected bool HasLocks(Business business)
        {
            return business != null
                && ((business.locks != null && business.locks.Count > 0)
                    || (business.upperLocks != null && business.upperLocks.Count > 0));
        }

        protected string DescribeBusinessLocks(Business business)
        {
            if (business == null) return "none";

            List<string> parts = new List<string>();
            string lower = DescribeLocks(business.locks);
            if (lower != "none") parts.Add(lower);
            string upper = DescribeLocks(business.upperLocks, true);
            if (upper != "none") parts.Add("upper: " + upper);
            return parts.Count == 0 ? "none" : string.Join("; ", parts);
        }

        protected bool UsesPlayerLevelLock(Business business)
        {
            return ContainsPlayerLevelLock(business?.locks) || ContainsPlayerLevelLock(business?.upperLocks);
        }

        protected bool UsesAnyPlayerLevelLock()
        {
            return LoadAllPrefixed<Business>().Any(UsesPlayerLevelLock);
        }

        private bool ContainsPlayerLevelLock(List<Lock> locks)
        {
            return locks != null && locks.Any(lockItem => lockItem != null && lockItem.playerLevelLock);
        }

        protected string DescribeCurrentPlayerLevel()
        {
            if (!PlayerStats.Instance || PlayerStats.Instance.level == null) return "missing PlayerStats";
            return $"{PlayerStats.Instance.GetPlayerLevel()}, XP {DescribePlayerXp()}";
        }

        protected string DescribeBusinessGroupCapacity(Business business)
        {
            if (business == null || business.businessGroups == null || business.businessGroups.Count == 0) return "none";

            return string.Join(", ", business.businessGroups.Where(group => group != null).Select(group =>
            {
                BigNumber used = BusinessesManager.Instance.GetBusinessGroupUsedCapacity(group);
                return IsUnlimited(group.capacity)
                    ? $"{group.metadata.name} {used}"
                    : $"{group.metadata.name} {used}/{DescribeLimit(group.capacity)}";
            }));
        }

        protected string DescribeBusinessCapacityLimits(BusinessHolder holder)
        {
            if (holder == null) return "none";

            List<string> parts = new List<string>();
            BigNumber maxAmount = holder.GetMaxAmountConstrain();
            if (!IsUnlimited(maxAmount))
            {
                parts.Add($"owned cap {holder.amount}/{DescribeLimit(maxAmount)}");
            }

            if (holder.business.businessGroups != null)
            {
                parts.AddRange(holder.business.businessGroups.Where(group => group != null && group.capacity >= 0).Select(group =>
                    $"{group.metadata.name} {BusinessesManager.Instance.GetBusinessGroupUsedCapacity(group)}/{DescribeLimit(group.capacity)}"));
            }

            return parts.Count == 0 ? "none" : string.Join(", ", parts);
        }

        protected bool CanStartBusinessProduction(string businessName, BigNumber amount)
        {
            return DescribeStartProductionAvailability(businessName, amount).StartsWith("ready", StringComparison.Ordinal);
        }

        protected bool CanStartAnyBusinessProduction(string businessName)
        {
            return DescribeStartAllProductionAvailability(businessName).StartsWith("ready", StringComparison.Ordinal);
        }

        protected bool HasActiveBusinessProduction(string businessName)
        {
            Business business = LoadOptional<Business>(businessName);
            if (business == null || !BusinessesManager.Instance) return false;

            BusinessHolder holder = BusinessesManager.Instance.GetHolder(business);
            return holder != null && holder.ActiveProductions.Count > 0;
        }

        protected bool HasStoredBusinessOutputs(string businessName)
        {
            Business business = LoadOptional<Business>(businessName);
            if (business == null || !BusinessesManager.Instance) return false;

            BusinessHolder holder = BusinessesManager.Instance.GetHolder(business);
            return holder != null && holder.unclaimedOutputs != null && holder.unclaimedOutputs.Count > 0;
        }

        protected bool AnyLoadedBusinessHasActiveProduction()
        {
            return LoadAllPrefixed<Business>().Any(business =>
            {
                BusinessHolder holder = BusinessesManager.Instance.GetHolder(business);
                return holder != null && holder.ActiveProductions.Count > 0;
            });
        }

        protected bool AnyLoadedBusinessHasStoredOutputs()
        {
            return LoadAllPrefixed<Business>().Any(business =>
            {
                BusinessHolder holder = BusinessesManager.Instance.GetHolder(business);
                return holder != null && holder.unclaimedOutputs != null && holder.unclaimedOutputs.Count > 0;
            });
        }

        protected bool CanUseBoost(Boost boost)
        {
            return CanUseBoost(boost, 1);
        }

        protected bool CanUseBoost(Boost boost, BigNumber amount)
        {
            if (boost == null || !BoostsManager.Instance) return false;
            return amount > 0 && BoostsManager.Instance.GetBoostAmountInInventory(boost) >= amount;
        }

        protected bool CanBuyUpgrade(Upgrade upgrade)
        {
            return upgrade != null && UpgradesManager.Instance && CanBuyAmount(UpgradesManager.Instance.GetOrAddHolder(upgrade).iBuyable, 1);
        }

        protected bool CanCompleteShopPurchase(ShopItem item)
        {
            if (item == null || !ShopManager.Instance) return false;

            ShopItemHolder holder = ShopManager.Instance.GetOrAddHolder(item);
            return !holder.iBuyable.Maxed;
        }

        protected bool CanAttemptRewardAd(ShopItem item)
        {
            return item != null && item.buyWithAd && CanCompleteShopPurchase(item);
        }

        protected bool CanConsumeShopItem(ShopItem item)
        {
            return item != null && ShopManager.Instance && ShopManager.Instance.GetOrAddHolder(item).amount > 0;
        }

        protected bool CanTravelTo(Location location)
        {
            if (location == null || !LocationsManager.Instance) return false;

            LocationHolder holder = LocationsManager.Instance.GetOrAddHolder(location);
            return holder.iBuyable.Maxed || CanBuyAmount(holder.iBuyable, 1);
        }

        protected bool CanMergeRecipe(BusinessMergeRecipe recipe)
        {
            return recipe != null && BusinessesManager.Instance && BusinessesManager.Instance.CanMergeBusinesses(recipe);
        }

        protected bool CanBuyManager(Manager manager)
        {
            return manager != null && ManagersManager.Instance && CanBuyAmount(ManagersManager.Instance.GetOrAddHolder(manager).iBuyable, 1);
        }

        protected bool CanActivateManager(Manager manager)
        {
            return manager != null && ManagersManager.Instance && ManagersManager.Instance.GetOrAddHolder(manager).CanActivate();
        }

        protected bool CanClaimAchievement(Achievement achievement)
        {
            if (achievement == null || !AchievementsManager.Instance) return false;

            AchievementHolder holder = AchievementsManager.Instance.GetOrAddHolder(achievement);
            return holder.CanClaimRewards;
        }

        protected bool CanClaimDailyReward(DailyRewardHolder holder)
        {
            if (holder == null || !DailyRewardsManager.Instance) return false;
            return !holder.isClaimed && !holder.isUpcoming && holder.expectedOutputs != null && holder.expectedOutputs.Count > 0;
        }

        protected string DescribeDailyRewardClaimAvailability(DailyRewardHolder holder)
        {
            if (holder == null) return "daily reward holder is missing";
            if (!DailyRewardsManager.Instance) return "DailyRewardsManager is missing";
            if (holder.isClaimed) return "already claimed";
            if (holder.isUpcoming)
            {
                int currentDay = DailyRewardsManager.Instance.GetCurrentDay();
                return $"day {holder.day} is upcoming; current day is {currentDay}, so use Next day or wait for the countdown";
            }

            if (holder.expectedOutputs == null || holder.expectedOutputs.Count == 0)
            {
                return "this calendar day has no configured explicit or repeating reward";
            }

            return holder.isMissed
                ? "ready, this missed day still has unclaimed rewards and TryClaimDailyRewards(day) can claim it"
                : "ready, this is the current unclaimed reward day";
        }

        protected bool CanOfferEnterPool(OfferPoolItem offer)
        {
            if (offer == null || offer.item == null || !ShopManager.Instance) return false;

            ShopItemHolder holder = ShopManager.Instance.GetOrAddHolder(offer.item);
            return !holder.iBuyable.Maxed && holder.iUnlockable.IsUnlocked();
        }

        protected string DescribeOfferEligibility(OfferPoolItem offer)
        {
            if (offer == null) return "offer entry is missing";
            if (offer.item == null) return "offer has no ShopItem assigned";
            if (!ShopManager.Instance) return "ShopManager is missing, so the offer cannot validate locks or limits";

            ShopItemHolder holder = ShopManager.Instance.GetOrAddHolder(offer.item);
            if (holder.iBuyable.Maxed) return $"maxed: bought {holder.BoughtAmount}/{DescribeLimit(holder.holdedItem.MaxBuyableAmount)}";
            if (!holder.iUnlockable.IsUnlocked()) return $"locked: {DescribeBlockingLocks(holder.UnlockableItem)}";
            return $"eligible, Trigger can choose this offer with weight {offer.weight}";
        }

        protected string DescribeActiveOfferAvailability()
        {
            if (!OffersManager.Instance) return "OffersManager is missing";
            OfferPoolItem active = OffersManager.Instance.ActiveOffer;
            if (active != null)
            {
                string itemName = active.item != null ? active.item.metadata.name : "missing ShopItem";
                return $"ready, {itemName} is active and CompleteExternalPurchase will grant its rewards";
            }

            List<OfferPoolItem> offers = OffersManager.Instance.possibleOffers;
            if (offers == null || offers.Count == 0)
            {
                return "no active offer exists, and the offer pool is empty";
            }

            int eligibleCount = offers.Count(CanOfferEnterPool);
            if (eligibleCount == 0)
            {
                string blockers = string.Join("; ", offers.Where(offer => offer != null).Select(DescribeOfferEligibility).Take(3));
                return string.IsNullOrWhiteSpace(blockers)
                    ? "no active offer exists, and no offer pool item is valid"
                    : $"no active offer exists; Trigger will not show one because all pool items are blocked: {blockers}";
            }

            return $"no active offer exists; press Trigger to choose from {eligibleCount}/{offers.Count} eligible offer(s)";
        }

        protected string DescribeOfferTimer()
        {
            if (!OffersManager.Instance) return "OffersManager is missing";

            float remaining = OffersManager.Instance.OfferTimerRemainingSeconds;
            OfferPoolItem active = OffersManager.Instance.ActiveOffer;
            if (active != null)
            {
                return $"{remaining:0.0}s / {active.durationSeconds:0.0}s";
            }

            return remaining > 0f ? $"next attempt in {remaining:0.0}s" : "no active offer";
        }

        protected float GetOfferTimerProgress()
        {
            if (!OffersManager.Instance) return 0f;

            OfferPoolItem active = OffersManager.Instance.ActiveOffer;
            if (active == null || active.durationSeconds <= 0f) return 0f;

            return OffersManager.Instance.OfferTimerRemainingSeconds / active.durationSeconds;
        }

        protected string DescribeMergeAvailability(BusinessMergeRecipe recipe)
        {
            if (recipe == null) return "merge recipe asset is missing";
            string shortfalls = DescribeInputShortfalls(recipe.inputs);
            return shortfalls == "none" ? "ready, all recipe inputs are available" : $"missing recipe inputs: {shortfalls}";
        }

        protected string DescribePendingUpgrades(UpgradeHolder holder)
        {
            if (holder == null || holder.pendingUpgrades == null || holder.pendingUpgrades.Count == 0)
            {
                return "none";
            }

            DateTime now = DateTime.Now;
            return string.Join("; ", holder.pendingUpgrades.Select((pending, index) =>
            {
                double seconds = Math.Max(0d, (pending.upgradeEndTime - now).TotalSeconds);
                return $"#{index + 1} x{pending.amount} in {seconds:0.#}s";
            }));
        }

        protected float GetPendingUpgradeProgress(Upgrade upgrade, UpgradeHolder holder)
        {
            if (upgrade == null || holder == null || holder.pendingUpgrades == null || holder.pendingUpgrades.Count == 0)
            {
                return 0f;
            }

            PendingUpgrade pending = holder.pendingUpgrades
                .OrderBy(item => item.upgradeEndTime)
                .FirstOrDefault();
            if (pending == null)
            {
                return 0f;
            }

            double duration = Math.Max(0.01d, upgrade.upgradeDurationInSeconds * Math.Max(1d, pending.amount.ToDouble()));
            double remaining = Math.Max(0d, (pending.upgradeEndTime - DateTime.Now).TotalSeconds);
            return Mathf.Clamp01((float)(1d - remaining / duration));
        }

        protected string DescribeUpgradeBuyAvailability(Upgrade upgrade)
        {
            if (upgrade == null) return "upgrade asset is missing";
            if (!UpgradesManager.Instance) return "UpgradesManager is missing";
            return DescribeBuyAvailability(UpgradesManager.Instance.GetOrAddHolder(upgrade).iBuyable, 1);
        }

        protected string DescribeShopCompleteAvailability(ShopItem item)
        {
            if (item == null) return "shop item asset is missing";
            if (!ShopManager.Instance) return "ShopManager is missing";

            ShopItemHolder holder = ShopManager.Instance.GetOrAddHolder(item);
            List<string> blockers = new List<string>();
            if (holder.iBuyable.Maxed) blockers.Add("purchase limit reached");

            return blockers.Count == 0
                ? "ready, CompleteExternalPurchase simulates a confirmed external purchase and grants reward outputs"
                : string.Join("; ", blockers);
        }

        protected string DescribeShopAdAvailability(ShopItem item)
        {
            if (item == null) return "shop item asset is missing";
            if (!item.buyWithAd) return "this shop item is not configured as an ad reward";
            return DescribeShopCompleteAvailability(item);
        }

        protected string DescribeShopConsumeAvailability(ShopItem item)
        {
            if (item == null) return "shop item asset is missing";
            if (!ShopManager.Instance) return "ShopManager is missing";

            BigNumber amount = ShopManager.Instance.GetOrAddHolder(item).amount;
            return amount > 0 ? $"ready, inventory has {amount}" : "inventory is 0; complete the purchase first so there is an item to consume";
        }

        protected string DescribeTravelAvailability(Location location)
        {
            if (location == null) return "location asset is missing";
            if (!LocationsManager.Instance) return "LocationsManager is missing";

            LocationHolder holder = LocationsManager.Instance.GetOrAddHolder(location);
            if (holder.iBuyable.Maxed) return "ready, this location was already unlocked";
            return DescribeBuyAvailability(holder.iBuyable, 1);
        }

        protected string DescribeManagerBuyAvailability(Manager manager)
        {
            if (manager == null) return "manager asset is missing";
            if (!ManagersManager.Instance) return "ManagersManager is missing";
            return DescribeBuyAvailability(ManagersManager.Instance.GetOrAddHolder(manager).iBuyable, 1);
        }

        protected string DescribeManagerActivationAvailability(Manager manager)
        {
            if (manager == null) return "manager asset is missing";
            if (!ManagersManager.Instance) return "ManagersManager is missing";

            ManagerHolder holder = ManagersManager.Instance.GetOrAddHolder(manager);
            if (holder.level <= 0) return "manager level is 0; buy or grant levels before activating";
            if (holder.IsOnCooldown) return $"manager is on cooldown for {holder.cooldownTimeLeft:0.0}s";
            return "ready, manager has a level and is not on cooldown";
        }

        protected string DescribeAchievementClaimAvailability(Achievement achievement)
        {
            if (achievement == null) return "achievement asset is missing";
            if (!AchievementsManager.Instance) return "AchievementsManager is missing";

            AchievementHolder holder = AchievementsManager.Instance.GetOrAddHolder(achievement);
            if (holder.IsFullyClaimed) return "all achievement tiers claimed";
            if (!holder.iUnlockable.IsUnlocked()) return $"locked tier {holder.NextClaimNumber}/{holder.TotalClaimAmount}: {DescribeLocks(holder.GetCurrentTierLocks())}";
            return $"ready, tier {holder.NextClaimNumber}/{holder.TotalClaimAmount} is unlocked";
        }

        protected string DescribePrestigeAvailability()
        {
            if (!PrestigeManager.Instance) return "PrestigeManager is missing";
            PrestigeManager manager = PrestigeManager.Instance;
            if (manager.CanPrestige()) return "ready, all prestige requirements are met";

            List<Lock> requirements = manager.currentPrestigeRequirements.Count == 0 ? manager.prestigeRequirements : manager.currentPrestigeRequirements;
            string blockers = DescribeLocks(requirements);
            return blockers == "none" ? "CanPrestige returned false, but no requirements are configured" : $"requirements are not met: {blockers}";
        }

        protected string DescribePlayerXp()
        {
            if (!PlayerStats.Instance || PlayerStats.Instance.level == null) return "none";
            return $"{PlayerStats.Instance.level.currentXp}/{PlayerStats.Instance.level.TargetXp}";
        }

        protected string DescribeLocks(List<Lock> locks, bool upper = false)
        {
            if (locks == null || locks.Count == 0) return "none";

            return string.Join("; ", locks.Select(lockItem => DescribeLock(lockItem, upper)));
        }

        private string DescribeLock(Lock lockItem, bool upper)
        {
            if (lockItem == null) return "null";

            string comparator = upper ? "<=" : ">=";
            string state = upper
                ? lockItem.IsUpperUnlocked() ? "ready" : "blocked"
                : lockItem.IsUnlocked() ? "ready" : "missing";

            string target = "empty";
            if (lockItem.businessLock != null)
            {
                BusinessHolder holder = BusinessesManager.Instance ? BusinessesManager.Instance.GetOrAddHolder(lockItem.businessLock) : null;
                BigNumber current = holder == null ? 0 : lockItem.amountType switch
                {
                    LockAmountType.TotalAmount => holder.TotalAmount,
                    LockAmountType.BoughtAmount => holder.BoughtAmount,
                    _ => holder.Amount
                };
                target = $"{lockItem.businessLock.businessName} {lockItem.amountType} {current} {comparator} {lockItem.inputAmount}";
            }
            else if (lockItem.businessUpgradeLevelLock != null)
            {
                BusinessHolder holder = BusinessesManager.Instance ? BusinessesManager.Instance.GetOrAddHolder(lockItem.businessUpgradeLevelLock) : null;
                BigNumber current = holder == null ? 0 : holder.iUpgradable.Level;
                string activeLevel = holder?.iUpgradable.GetCurrentUpgradeLevel() is BusinessUpgradeLevel upgradeLevel
                    ? $", active tier {upgradeLevel.level}"
                    : string.Empty;
                target = $"{lockItem.businessUpgradeLevelLock.businessName} upgrade level {current} {comparator} {lockItem.inputAmount}{activeLevel}";
            }
            else if (lockItem.currencyLock != null)
            {
                BigNumber current = 0;
                if (lockItem.amountType == LockAmountType.TotalAmount)
                {
                    current = CurrencyManager.Instance ? CurrencyManager.Instance.GetTotalCurrencyAmount(lockItem.currencyLock) : 0;
                }
                else
                {
                    current = CurrencyManager.Instance ? CurrencyManager.Instance.GetCurrencyAmount(lockItem.currencyLock) : 0;
                }
                target = $"{lockItem.currencyLock.currencyName} {lockItem.amountType} {current} {comparator} {lockItem.inputAmount}";
            }
            else if (lockItem.playerLevelLock)
            {
                int currentLevel = PlayerStats.Instance ? PlayerStats.Instance.GetPlayerLevel() : 0;
                string xp = PlayerStats.Instance && PlayerStats.Instance.level != null
                    ? $", xp {DescribePlayerXp()}"
                    : string.Empty;
                target = $"Player level {currentLevel} {comparator} {lockItem.inputAmount}{xp}";
            }
            else if (lockItem.customStatLock != null)
            {
                BigNumber current = PlayerStats.Instance ? PlayerStats.Instance.GetStat(lockItem.customStatLock) : 0;
                target = $"{lockItem.customStatLock.metadata.name} {current} {comparator} {lockItem.inputAmount}";
            }
            else if (lockItem.prestigeLock)
            {
                BigNumber current = PrestigeManager.Instance ? PrestigeManager.Instance.prestiges.prestigeCount : 0;
                target = $"Prestige count {current} {comparator} {lockItem.inputAmount}";
            }
            else if (lockItem.locationLock != null)
            {
                Location active = LocationsManager.Instance ? LocationsManager.Instance.ActiveLocation : null;
                string activeName = active ? active.metadata.name : "None";
                string expected = lockItem.locationLock.metadata.name;
                target = upper ? $"Active location {activeName} must not be {expected}" : $"Active location {activeName} must be {expected}";
            }

            return $"{target} ({state})";
        }

        protected bool CanAutoCollectOutput(BusinessHolder holder)
        {
            if (holder == null) return false;
            return holder.business.autoCollect || holder.IsManagerUnlockedAndAboveAutoCollectThreshold();
        }

        protected string FormatRewardCountdown(TimeSpan time)
        {
            if (time.TotalSeconds <= 0) return "ready";
            return time.TotalHours >= 1 ? time.ToString(@"hh\:mm\:ss") : time.ToString(@"mm\:ss");
        }

        protected string DescribeOutput(Output output)
        {
            if (output == null) return "null";

            string name = "Unknown";
            if (output.currencyOutput != null) name = output.currencyOutput.currencyName;
            if (output.businessOutput != null) name = output.businessOutput.businessName;
            if (output.boostOutput != null) name = output.boostOutput.boostName;
            if (output.managerOutput != null) name = output.managerOutput.name;

            string amount = output.outputMaxAmount > output.outputAmount
                ? $"{output.outputAmount}-{output.outputMaxAmount}"
                : output.outputAmount.ToString();

            return $"{name} x{amount}";
        }

        protected string DescribeStoredOutputs(BusinessHolder holder)
        {
            return holder == null ? "none" : DescribeOutputs(holder.unclaimedOutputs.Squash());
        }

        protected string DescribeOutputs(List<Output> outputs)
        {
            if (outputs == null || outputs.Count == 0) return "none";

            List<Output> displayOutputs = outputs.Squash();
            return displayOutputs.Count == 0 ? "none" : string.Join(", ", displayOutputs.Select(DescribeOutput));
        }

        protected string DescribeDropTables(List<DropTable> tables)
        {
            if (tables == null || tables.Count == 0) return "none";

            return string.Join(" | ", tables.Select(table =>
                $"{table.strategy}, picks={table.totalDropAmount}, avg={DescribeOutputs(table.GetAverageOutputs(1))}"));
        }

        protected string DescribeDropTableStrategies(List<DropTable> tables)
        {
            if (tables == null || tables.Count == 0) return "none";

            return string.Join(", ", tables.Select(table => $"{table.strategy} x{table.totalDropAmount}"));
        }

        protected string DescribeUpgradeBlocks(IEnumerable<UpgradeBlock> blocks)
        {
            if (blocks == null) return "none";
            List<string> parts = blocks.Select(block => block == null ? "null" : $"{block.upgradeType} x{block.value:0.##}").ToList();
            return parts.Count == 0 ? "none" : string.Join(", ", parts);
        }

        protected string DescribeOverrideBlocks(IEnumerable<OverrideUpgradeBlock> blocks)
        {
            if (blocks == null) return "none";
            List<string> parts = blocks.Select(block => block == null ? "null" : $"{block.upgradeType}={block.value}").ToList();
            return parts.Count == 0 ? "none" : string.Join(", ", parts);
        }

        protected string DescribeOverrideModifiers(IEnumerable<OverrideUpgradeModifierBlock> blocks)
        {
            if (blocks == null) return "none";
            List<string> parts = blocks.Select(block => block == null ? "null" : $"{block.operation} {block.targetOverrideType} {block.value}").ToList();
            return parts.Count == 0 ? "none" : string.Join(", ", parts);
        }

        protected string DescribeBusinessUpgradeLevel(BusinessUpgradeLevel level)
        {
            if (level == null) return "none";

            List<string> parts = new List<string> { $"level {level.level}" };
            if (!string.IsNullOrEmpty(level.businessName)) parts.Add($"name={level.businessName}");
            if (!string.IsNullOrEmpty(level.iconString)) parts.Add($"icon={level.iconString}");
            if (!string.IsNullOrEmpty(level.description)) parts.Add("description override");
            if (level.maxAmount_ >= 0) parts.Add($"max={level.maxAmount_}");
            if (level.maxBuyableAmount >= 0) parts.Add($"buy cap={level.maxBuyableAmount}");
            return string.Join(", ", parts);
        }

        protected string DescribeGroups(List<BusinessGroup> groups)
        {
            if (groups == null || groups.Count == 0) return "all";
            return string.Join(", ", groups.Where(group => group != null).Select(group => group.metadata.name));
        }

        protected string DescribeProfit(ProfitData profit)
        {
            if (profit == null) return "none";

            List<string> parts = new List<string>();
            HashSet<Currency> stockpiledCurrencies = new HashSet<Currency>();
            HashSet<Business> stockpiledBusinesses = new HashSet<Business>();
            foreach (List<Output> stockpile in profit.businessesStockpile.Values)
            {
                if (stockpile == null) continue;

                foreach (Output output in stockpile)
                {
                    if (output.currencyOutput != null) stockpiledCurrencies.Add(output.currencyOutput);
                    if (output.businessOutput != null) stockpiledBusinesses.Add(output.businessOutput);
                }
            }

            parts.AddRange(profit.businessProfits.Select(pair => $"{pair.Key.currencyName} applied +{pair.Value}"));
            parts.AddRange(profit.businessOutputs.Select(pair => $"{pair.Key.businessName} applied +{pair.Value}"));
            parts.AddRange(profit.absoluteBusinessProfits
                .Where(pair => !stockpiledCurrencies.Contains(pair.Key))
                .Select(pair => $"{pair.Key.currencyName} generated {pair.Value}"));
            parts.AddRange(profit.absoluteBusinessOutputs
                .Where(pair => !stockpiledBusinesses.Contains(pair.Key))
                .Select(pair => $"{pair.Key.businessName} generated {pair.Value}"));
            parts.AddRange(profit.businessesStockpile.Select(pair => $"{pair.Key.businessName} stockpile {DescribeOutputs(pair.Value)}"));
            return parts.Count == 0 ? "no applied outputs" : string.Join(", ", parts);
        }

        protected string DescribeSummary(ProfitApplicationSummary summary)
        {
            if (summary == null) return "none";

            List<string> parts = new List<string>();
            parts.AddRange(summary.AppliedCurrencies.Select(pair => $"{pair.Key.currencyName} +{pair.Value}"));
            parts.AddRange(summary.AppliedBusinesses.Select(pair => $"{pair.Key.businessName} +{pair.Value}"));
            parts.AddRange(summary.AppliedProductionTimes.Select(pair => $"{pair.Key.businessName} progress={pair.Value}"));
            return parts.Count == 0 ? "nothing applied" : string.Join(", ", parts);
        }

        protected string DescribeStockpiles(Dictionary<Business, List<Output>> stockpiles)
        {
            if (stockpiles == null || stockpiles.Count == 0) return "none";

            return string.Join(", ", stockpiles.Select(pair => $"{pair.Key.businessName}: {DescribeOutputs(pair.Value)}"));
        }

        protected void UpdateStatusText()
        {
            if (statusText == null) return;

            statusText.text = string.Empty;
            if (titleText != null) titleText.text = demoTitle;
        }

        protected void Log(string message)
        {
            LastActionResult = message;
            Debug.Log($"[{demoTitle}] {message}");
        }
    }
}

