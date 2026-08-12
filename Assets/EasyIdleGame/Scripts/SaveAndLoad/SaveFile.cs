using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that hold all the data that needs to be saved
    /// </summary>
    [System.Serializable]
    public class SaveFile
    {
        public CurrencyHolder[] currencies;
        public BusinessHolder[] businessHolders;
        public ManagerHolder[] managerHolders = null;
        public LocationHolder[] locationHolders = null;
        public ShopItemHolder[] shopItemHolders = null;
        public UpgradeHolder[] upgradeHolders = null;
        public AchievementHolder[] achievementHolders = null;

        // player stats
        public Level levelData = null;
        public CurrencyHolder[] totalMoneyMade = null;
        public BusinessHolder[] totalBusinessesObtained = null;
        public BoostHolder[] totalBoostsObtained = null;
        public CustomStatHolder[] customStats = null;

        // boosts
        public BoostHolder[] boostHolders = null;
        public ActiveBoostHolder[] activeBoosts = null;

        // prestiges
        public PrestigeData prestigeData = null;

        // reward data
        public long _initTimecode;
        public DateTime initDate
        {
            get => DateTime.FromFileTime(_initTimecode);
            set => _initTimecode = value.ToFileTime();
        }
        public List<int> claimedDays = null;

        public long _timecode;
        public DateTime saveTime
        {
            get => DateTime.FromFileTime(_timecode);
            set => _timecode = value.ToFileTime();
        }

        public SaveFile()
        {
            currencies = CurrencyManager.Instance.holders.ToArray();
            businessHolders = BusinessesManager.Instance.holders.ToArray();

            if (ManagersManager.Instance) managerHolders = ManagersManager.Instance.holders.ToArray();

            if (LocationsManager.Instance) locationHolders = LocationsManager.Instance.holders.ToArray();

            if (ShopManager.Instance) shopItemHolders = ShopManager.Instance.holders.ToArray();

            if (PlayerStats.Instance)
            {
                levelData = PlayerStats.Instance.level;
                totalMoneyMade = PlayerStats.Instance.totalMoneyMade.ToArray();
                totalBusinessesObtained = PlayerStats.Instance.totalBusinessesObtained.ToArray();
                totalBoostsObtained = PlayerStats.Instance.totalBoostsObtained.ToArray();
                customStats = PlayerStats.Instance.customStats.ToArray();
            }

            if (BoostsManager.Instance)
            {
                boostHolders = BoostsManager.Instance.boostsInInventory.ToArray();
                activeBoosts = BoostsManager.Instance.activeBoosts.ToArray();
            }

            if (PrestigeManager.Instance) prestigeData = PrestigeManager.Instance.prestiges;

            if (UpgradesManager.Instance) upgradeHolders = UpgradesManager.Instance.upgrades.ToArray();

            if (AchievementsManager.Instance) achievementHolders = AchievementsManager.Instance.holders.ToArray();

            if (DailyRewardsManager.Instance)
            {
                initDate = DailyRewardsManager.Instance.initDate;
                claimedDays = DailyRewardsManager.Instance.claimedDays;
            }

            saveTime = DateTime.Now;
        }

        public virtual void Load()
        {
            // match currencies and set amounts
            CurrencyManager.Instance.holders = currencies
                .Select(x =>
                {
                    x.Load();

                    // patch: totalAmount was not tracked in older saves; seed it from amount
                    // so currency lifetime totals have a conservative starting point.
                    if (x.totalAmount == 0 && x.amount > 0)
                    {
                        x.totalAmount = x.amount;
                    }

                    return x;
                })
                .ToList();

            // set managerHolders
            if (ManagersManager.Instance)
            {
                ManagersManager.Instance.holders = managerHolders
                    .Select(x =>
                    {
                        x.Load();

                        // patch: boughtAmount was not tracked in older saves; seed from totalCount
                        if (x.boughtAmount == 0 && x.totalCount > 0)
                        {
                            x.boughtAmount = x.totalCount;
                        }

                        return x;
                    })
                    .ToList();
            }

            if (LocationsManager.Instance && locationHolders != null)
            {
                LocationsManager.Instance.holders = locationHolders
                    .Select(x =>
                    {
                        x.Load();

                        // patch: boughtAmount was not tracked in older saves; seed from amount
                        if (x.boughtAmount == 0 && x.amount > 0)
                        {
                            x.boughtAmount = x.amount;
                        }

                        return x;
                    })
                    .ToList();
            }

            if (ShopManager.Instance && shopItemHolders != null)
            {
                ShopManager.Instance.holders = shopItemHolders
                    .Select(x =>
                    {
                        x.Load();

                        // patch: boughtAmount was not tracked in older saves; seed from amount
                        if (x.boughtAmount == 0 && x.amount > 0)
                        {
                            x.boughtAmount = x.amount;
                        }

                        return x;
                    })
                    .ToList();
            }

            // set level
            if (PlayerStats.Instance)
            {
                PlayerStats.Instance.level = levelData;

                // patch to make save compatible with old saves
                if (PlayerStats.Instance.level.startXp == 0)
                {
                    PlayerStats.Instance.level.startXp = PlayerStats.Instance.level.startXp;
                }

                PlayerStats.Instance.totalMoneyMade = totalMoneyMade
                    .Select(x =>
                    {
                        x.Load();
                        return x;
                    })
                    .ToList();

                PlayerStats.Instance.totalBusinessesObtained = totalBusinessesObtained
                    .Select(x =>
                    {
                        x.Load();
                        return x;
                    })
                    .ToList();

                PlayerStats.Instance.totalBoostsObtained = totalBoostsObtained
                    .Select(x =>
                    {
                        x.Load();
                        return x;
                    })
                    .ToList();

                if (customStats != null)
                    PlayerStats.Instance.customStats = customStats.Select(x =>
                    {
                        x.Load();
                        return x;
                    }).ToList();
            }

            // set businessHolders
            BusinessesManager.Instance.holders = businessHolders
                .Select(x =>
                {
                    x.Load();

                    // patch to make save compatible with old saves
                    if (x.level.startXp == 0)
                    {
                        x.level.startXp = x.business.firstLevelCopies;
                    }

                    // patch: totalAmount was not tracked in older saves; seed it from amount so
                    // geometric cost scaling starts from the correct position instead of zero
                    if (x.totalAmount == 0 && x.amount > 0)
                    {
                        x.totalAmount = x.amount;
                    }

                    // patch: boughtAmount was not tracked in older saves; seed from totalAmount
                    // (conservative assumption: all previously acquired businesses were purchased)
                    if (x.boughtAmount == 0 && x.totalAmount > 0)
                    {
                        x.boughtAmount = x.totalAmount;
                    }

                    return x;
                })
                .ToList();

            if (BoostsManager.Instance)
            {
                // set boosts
                BoostsManager.Instance.holders = boostHolders
                    .Select(x =>
                    {
                        x.Load();

                        // patch: boughtAmount was not tracked in older saves; seed from totalAmount
                        if (x.boughtAmount == 0 && x.totalAmount > 0)
                        {
                            x.boughtAmount = x.totalAmount;
                        }

                        return x;
                    })
                    .ToList();

                // set active boosts
                BoostsManager.Instance.activeBoosts = activeBoosts
                    .Select(x =>
                    {
                        x.boost = x.boostSaveable.LoadScriptableObject();
                        return x;
                    })
                    // drop active boosts whose asset could not be resolved (renamed/removed since the save was written)
                    .Where(x => x.boost != null)
                    .ToList();
            }

            // set prestige data
            if (PrestigeManager.Instance)
                PrestigeManager.Instance.prestiges = prestigeData;

            // set upgrades
            if (UpgradesManager.Instance)
                UpgradesManager.Instance.holders = upgradeHolders
                    .Select(x =>
                    {
                        x.Load();

                        // patch: boughtAmount was not tracked in older saves; seed from amount
                        // (for upgrades TotalAmount == amount since they cannot be removed)
                        if (x.boughtAmount == 0 && x.amount > 0)
                        {
                            x.boughtAmount = x.amount;
                        }

                        return x;
                    })
                    .ToList();

            if (AchievementsManager.Instance && achievementHolders != null)
                AchievementsManager.Instance.holders = achievementHolders
                    .Select(x =>
                    {
                        x.Load();
                        return x;
                    })
                    .ToList();

            // set reward data
            if (DailyRewardsManager.Instance)
            {
                DailyRewardsManager.Instance.initDate = initDate;
                DailyRewardsManager.Instance.claimedDays = claimedDays ?? new List<int>();
            }
        }

        public override string ToString()
        {
            string currenciesString = string.Join("\n", currencies.Select(c => c.ToString()));
            string businessString = string.Join("\n", businessHolders.Select(b => b.ToString()));
            string managerString = string.Join("\n", managerHolders.Select(m => m.ToString()));
            string locationString = locationHolders != null ? string.Join("\n", locationHolders.Select(l => l.ToString())) : "";
            string shopItemString = shopItemHolders != null ? string.Join("\n", shopItemHolders.Select(s => s.ToString())) : "";
            string levelString = levelData.ToString();
            string totalMoneyMadeString = string.Join("\n", totalMoneyMade.Select(t => t.ToString()));
            string boostString = string.Join("\n", boostHolders.Select(b => b.ToString()));
            string activeBoostString = string.Join("\n", activeBoosts.Select(a => a.ToString()));
            string prestigeString = prestigeData.ToString();
            string upgradeString = string.Join("\n", upgradeHolders.Select(u => u.ToString()));
            string achievementsString = achievementHolders != null ? string.Join("\n", achievementHolders.Select(a => a.ToString())) : "";
            string customStatsString = customStats != null ? string.Join("\n", customStats.Select(s => $"{s.stat?.GetDisplayName()}: {s.amount}")) : "";

            return $"SaveFile:\n" +
                   $"Currencies:\n{currenciesString}\n" +
                   $"Businesses:\n{businessString}\n" +
                   $"Managers:\n{managerString}\n" +
                   $"Locations:\n{locationString}\n" +
                   $"ShopItems:\n{shopItemString}\n" +
                   $"Level:\n{levelString}\n" +
                   $"TotalMoneyMade:\n{totalMoneyMadeString}\n" +
                   $"Boosts:\n{boostString}\n" +
                   $"ActiveBoosts:\n{activeBoostString}\n" +
                   $"PrestigeData:\n{prestigeString}\n" +
                   $"Upgrades:\n{upgradeString}\n" +
                   $"Achievements:\n{achievementsString}\n" +
                   $"CustomStats:\n{customStatsString}";
        }
    }
}
