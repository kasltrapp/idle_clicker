using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that handles player level and total money made
    /// </summary>
    [AddComponentMenu("EasyIdleGame/Managers/PlayerStats")]
    public class PlayerStats : ManagerBase<PlayerStats>
    {
        [CommentArea("Player Stats", "Tracks player XP, level rewards, total earned resources, total obtained businesses/boosts, and custom stat counters.", "Use this manager for achievement progress such as total coins earned. Business and currency managers update the totals automatically, and custom systems can call IncrementStat for project-specific counters.")]
        [SerializeField] private string _playerStatsComment;

        [Tooltip("XP required to reach the first player level.")]
        public int firstLevelXp = 1000;

        [Tooltip("Multiplier applied to required XP after each player level-up.")]
        public int levelIncrementMultiplier = 2;

        [Tooltip("If enabled, pending player levels are claimed automatically as soon as enough XP is earned. Disable for manual level-up claim flows.")]
        public bool autoLevelUp = true;

        [Tooltip("Rewards and descriptions used when the player levels up. Leave empty if player levels should not grant rewards or labels.")]
        public LevelDataHolder levelupData = new LevelDataHolder();

        [Header("Auto")]
        [Tooltip("Runtime total currency earned by type. Usually managed automatically and saved; edit only for debugging or migration.")]
        public List<CurrencyHolder> totalMoneyMade = new List<CurrencyHolder>();

        [Tooltip("Runtime total businesses obtained by type. Usually managed automatically and saved; edit only for debugging or migration.")]
        public List<BusinessHolder> totalBusinessesObtained = new List<BusinessHolder>();

        [Tooltip("Runtime total boosts obtained by type. Usually managed automatically and saved; edit only for debugging or migration.")]
        public List<BoostHolder> totalBoostsObtained = new List<BoostHolder>();

        [Tooltip("Runtime custom stat values. Usually updated through IncrementStat and saved; edit only for debugging or migration.")]
        public List<CustomStatHolder> customStats = new List<CustomStatHolder>();

        [Tooltip("Current player level and XP state. Usually managed automatically and saved; edit only for debugging or migration.")]
        public Level level;

        [Header("Events")]
        [Tooltip("Invoked after the player levels up and level rewards are applied. Argument is the new level.")]
        public UnityEvent<int> OnLevelUpEvent = new();

        [Tooltip("Invoked whenever XP is added. Argument is the amount added after upgrade multipliers.")]
        public UnityEvent<BigNumber> OnXpAdded = new();

        public void AddXp(BigNumber xps)
        {
            if (xps == 0) return;

            if (UpgradesManager.Instance)
            {
                xps *= UpgradesManager.Instance.GetMultiplierOfType(UpgradeType.playerXp, "");
            }

            if (BoostsManager.Instance)
            {
                xps *= BoostsManager.Instance.GetMultiplierOfType(UpgradeType.playerXp, "");
            }

            OnXpAdded.Invoke(xps);

            level.AddXp(xps);

            Log($"Added {xps} XP. New total XP: {level.currentXp}", LogCategory.Verbose);

            if (!autoLevelUp) return;

            // BUGFIX (see CLAUDE.md Watch-list): level.LevelUp() decrements level.unclaimedLevelups
            // in place. Re-reading that same field as the loop bound on every iteration meant the
            // increasing counter and the shrinking bound crossed at the midpoint, silently dropping
            // the second half of any multi-level claim (both currentLevel advancement and the
            // OnLevelUp reward firing for those dropped levels). Caching the count up front fixes it.
            int levelsToClaim = level.unclaimedLevelups;
            for (int z = 0; z < levelsToClaim; z++)
            {
                level.LevelUp();
                OnLevelUp(level.currentLevel);
            }
        }

        private void OnLevelUp(int level)
        {
            Log($"Player Leveled Up to level {level}!", LogCategory.Verbose);
            OnLevelUpEvent.Invoke(level);
            levelupData.ApplyLevelReward(level);
        }

        public void Start()
        {
            ResetLevel();
        }

        public void ResetLevel()
        {
            level = new Level(firstLevelXp, levelIncrementMultiplier);
        }

        public int GetPlayerLevel() => level.currentLevel;

        /// <summary> Is used mainly internally - whenever is currency added into bank </summary>
        public void OnCurrencyAmountAdded(Currency currency, BigNumber amount)
        {
            if (currency == null) return;
            foreach (CurrencyHolder moneyMade in totalMoneyMade)
            {
                if (moneyMade.currency == currency)
                {
                    moneyMade.amount += amount;
                    return;
                }
            }

            totalMoneyMade.Add(new CurrencyHolder(currency, amount));
        }

        public BigNumber GetTotalMoneyMade(Currency currency)
        {
            if (currency == null) return 0;
            foreach (CurrencyHolder moneyMade in totalMoneyMade)
            {
                if (moneyMade.currency == currency)
                {
                    return moneyMade.amount;
                }
            }
            return 0;
        }

        public void OnBusinessAmountAdded(Business business, BigNumber amount)
        {
            if (business == null) return;
            foreach (BusinessHolder businessObtained in totalBusinessesObtained)
            {
                if (businessObtained.business == business)
                {
                    businessObtained.amount += amount;
                    return;
                }
            }
            totalBusinessesObtained.Add(new BusinessHolder(business, amount));
        }

        public BigNumber GetTotalBusinessesObtained(Business business)
        {
            if (business == null) return 0;
            foreach (BusinessHolder businessObtained in totalBusinessesObtained)
            {
                if (businessObtained.business == business)
                {
                    return businessObtained.amount;
                }
            }
            return 0;
        }

        public void OnBoostAmountAdded(Boost boost, BigNumber amount)
        {
            if (boost == null) return;
            foreach (BoostHolder boostObtained in totalBoostsObtained)
            {
                if (boostObtained.boost == boost)
                {
                    boostObtained.amount += amount;
                    return;
                }
            }
            totalBoostsObtained.Add(new BoostHolder(boost) { amount = amount });
        }

        public BigNumber GetTotalBoostsObtained(Boost boost)
        {
            if (boost == null) return 0;
            foreach (BoostHolder boostObtained in totalBoostsObtained)
            {
                if (boostObtained.boost == boost)
                {
                    return boostObtained.amount;
                }
            }
            return 0;
        }

        public void IncrementStat(CustomStat customStat, BigNumber amount)
        {
            if (customStat == null) return;

            Log($"Incrementing custom stat {customStat.GetDisplayName()} by {amount}.", LogCategory.Verbose);

            for (int i = 0; i < customStats.Count; i++)
            {
                if (customStats[i].stat == customStat)
                {
                    CustomStatHolder statHolder = customStats[i];
                    statHolder.amount += amount;
                    customStats[i] = statHolder;
                    return;
                }
            }

            customStats.Add(new CustomStatHolder(customStat, amount));
        }

        public BigNumber GetStat(CustomStat customStat)
        {
            if (customStat == null) return 0;

            foreach (var statHolder in customStats)
            {
                if (statHolder.stat == customStat)
                    return statHolder.amount;
            }

            return 0;
        }
    }

    [System.Serializable]
    public class CustomStatHolder : HolderBase<CustomStat>
    {
        public CustomStat stat => Item;
        public BigNumber amount;

        public CustomStatHolder(CustomStat stat, BigNumber amount) : base(stat)
        {
            this.amount = amount;
        }
    }
}
