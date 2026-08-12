using UnityEngine;

namespace EasyIdleGame
{
    /// <summary>
    /// Multiplier upgrade types.
    /// For these upgrade types, values are multiplied together unless a contextual UpgradeBlock uses an additive operation.
    /// </summary>
    public enum UpgradeType
    {
        [Tooltip("Multiplies generated output amounts, reward amounts, or other production-style values.")]
        production = 0,

        [Tooltip("Multiplies speed-style effects, such as business production speed or merge speed.")]
        speed = 1,

        [Tooltip("Multiplies purchase costs for buyable holders, such as businesses, managers, or boosts.")]
        purchaseCost = 2,

        [Tooltip("Multiplies player XP rewards.")]
        playerXp = 3,

        [Tooltip("Multiplies business XP rewards.")]
        businessXp = 4,

        [Tooltip("Multiplies active effect duration, such as boost duration or manager active duration.")]
        duration = 5,

        [Tooltip("Changes drop chance for matching reward or production outputs. Use Add for flat chance changes.")]
        dropChance = 6,

        [Tooltip("Multiplies weighted-random drop weights for matching reward or production outputs.")]
        dropWeight = 7,

        [Tooltip("Multiplies runtime production-output scaling such as bonus output per housed business.")]
        productionOutputScaling = 8,

        [Tooltip("Multiplies cooldown duration. Use values below 1 to shorten cooldowns and values above 1 to lengthen them.")]
        cooldown = 9,

        [Tooltip("Multiplies costs consumed to start production, such as feed or input materials.")]
        productionInputCost = 10,

        [Tooltip("Multiplies the cost of buying Upgrade assets.")]
        upgradePurchaseCost = 11,

        [Tooltip("Multiplies costs paid when leveling an IUpgradableHolder, such as a business or manager level-up.")]
        levelUpCost = 12,

        [Tooltip("Multiplies inputs consumed by merge recipes.")]
        mergeInputCost = 13,
        // !! DO NOT CHANGE THE ORDER OF THESE VALUES AS THEY ARE BEING SAVED AS INTEGER IN THE UNITY EDITOR
        // for the custom upgrade types, start from 1000
        // eg, customUpgradeType1 = 1000,
    }
}
