using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EasyIdleGame
{
    public enum ProductionRoundMode
    {
        SingleRoundLiveAmount = 0,
        SingleRoundSnapshot = 1,
        ConcurrentRounds = 2
    }

    /// <summary>
    /// Scriptable object that stores business data
    /// </summary>
    [CreateAssetMenu(fileName = "newBusiness", menuName = "EasyIdleGame/Businesses/Business", order = 1)]
    public class Business : ScriptableObject, IBuyable, IUnlockable, ILevelupable, IProducable, IUpgradable, ISerializationCallbackReceiver, IMetadataProvider, ILocatable
    {
        [CommentArea("Business", "The core entity of the idle game that produces outputs over time. Businesses can be bought, upgraded, leveled up, and automated via managers. They scale in production and cost based on player progression.", "Create a Lemonade Stand business, set a Coin cost, add a DropTable that outputs Coins, then assign the business to a BusinessDisplayer prefab or BusinessListDisplayer so players can buy and run it.")]
        [SerializeField] private string _businessComment;

        public IBuyable iBuyable => this;
        public IUnlockable iUnlockable => this;
        public ILevelupable iLevelupable => this;
        public IProducable iProducable => this;
        public IUpgradable iUpgradable => this;

        [Header("Audio")]
        [Tooltip("Sound played when this business is bought.")]
        public AudioData buySound;
        [Tooltip("Sound played when this business is unlocked via locks.")]
        public AudioData unlockSound;
        [Tooltip("Sound played when this business levels up.")]
        public AudioData levelUpSound;
        [Tooltip("Sound played when this business starts a production round.")]
        public AudioData productionStartSound;
        [Tooltip("Sound played when this business completes a production round.")]
        public AudioData productionEndSound;

        // IBuyable
        public List<Input> Cost => cost;
        public BigNumber CostMultiplier => costMultiplier;
        public BigNumber MaxBuyableAmount => maxBuyableAmount;
        public bool UseGeometricBoughtAmount => useGeometricBoughtAmount;
        public AudioData BuySound => buySound;

        // IUnlockable
        public List<Lock> Locks => locks;
        public List<Lock> UpperLocks => upperLocks;
        public bool UnlockOnlyOnce => unlockOnlyOnce;
        public AudioData UnlockSound => unlockSound;

        // ILevelupable
        public int FirstLevelXp => firstLevelCopies;
        public float IncrementMultiplier => incrementMultiplier;
        public LevelDataHolder LevelupData => levelupData;
        public bool AutoLevelup => autoLevelUp;
        public AudioData LevelUpSound => levelUpSound;

        // IProducable
        [System.Obsolete("Use OutputTables instead. Note: Outputs is now a flattened list for backward compatibility.", false)]
        public List<Output> Outputs => outputTables.SelectMany(x => x.outputs).ToList();

        public List<DropTable> OutputTables => outputTables;

        public float InitialTimeToProduce => timeToProduce;
        public List<Input> ProductionCost => productionCost;
        public bool ScaleProductionCostWithAmount => scaleProductionCostWithAmount;
        public bool AllowPartialProductionCost => allowPartialProductionCost;
        public bool ConsumeOnProduce => consumeOnProduce;
        public ProductionRoundMode ProductionRoundMode => GetProductionRoundMode();
        public Metadata Metadata => GetMetadata();
        public List<Lock> ProductionLocks => productionLocks;
        public AudioData ProductionStartSound => productionStartSound;
        public AudioData ProductionEndSound => productionEndSound;

        // IUpgradable
        public List<Input> UpgradeCost => upgradeCost;
        public float UpgradeCostMultiplier => upgradeCostMultiplier;
        public bool AutoUpgradeForFree => autoUpgradeForFree;
        public bool AutoUpgradeToFirstLevelFree => autoUpgradeToFirstLevelFree;
        public BigNumber MaxLevel => BigNumber.MaxValue;
        public UpgradeLevel[] UpgradeLevels => upgradeLevels;

        [Header("General")] // ------------------------------- GENERAL ------------------------------- 
        [Tooltip("Display metadata shown by business UI. New content should use this instead of the legacy name/icon/description fields.")]
        public Metadata metadata = new Metadata();

        [Tooltip("The location this business belongs to. Used to filter sounds and interactions based on active location. Leave null if it's global.")]
        public Location location;

        public Location Location => location;

        [HideInInspector]
        [Tooltip("[LEGACY] Display name migrated into metadata.name.")]
        public string businessName;

        [HideInInspector]
        [Tooltip("[LEGACY] Sprite migrated into metadata.icon.")]
        public Sprite image;

        [HideInInspector]
        [Tooltip("[LEGACY] Compact text migrated into metadata.iconString.")]
        public string iconString;

        [HideInInspector]
        [Tooltip("[LEGACY] Model migrated into metadata.model.")]
        public GameObject model;

        [HideInInspector]
        [TextArea(3, 10)]
        [Tooltip("[LEGACY] Description migrated into metadata.description.")]
        public string description;

        [HideInInspector]
        [SerializeField]
        private MetadataLegacySyncState metadataLegacySyncState = new MetadataLegacySyncState();

        [Space(16)]
        [Header("Deprecated")]
        [Tooltip("[DEPRECATED] Legacy string group used by old assets and APIs. Use businessGroups for new content; this value is only a fallback when no typed groups are assigned.")]
        [System.Obsolete("Use businessGroups instead.")]
        public string businessGroup;

        [Tooltip("Typed groups this business belongs to. Used for shared capacities and targeting upgrades, boosts, and managers; leave empty if the business has no specific group.")]
        public List<BusinessGroup> businessGroups = new List<BusinessGroup>();

        [Tooltip("Maximum owned copies of this business. -1 means infinite; upgrade-level overrides, override upgrades, and business group capacity can further affect the effective cap.")]
        public BigNumber maxAmount_ = -1;

        [Header("Buying")] // ------------------------------- BUYING ------------------------------- 
        [ConditionalCommentArea("Business setup", "Max buyable amount is higher than max amount. This may be intentional, but the max amount will still cap ownership.", "maxBuyableAmount", ConditionalCommentAreaMode.BigNumberGreaterThan, "maxAmount_")]
        [SerializeField] private string _maxBuyableAmountInfo;
        [Tooltip("Maximum copies that can be bought directly. -1 means unlimited purchases; outputs and rewards can still grant copies beyond this purchase cap unless maxAmount_ or group capacity stops them.")]
        public BigNumber maxBuyableAmount = new BigNumber(-1);

        [ConditionalCommentArea("Business setup", "No cost is defined. This is fine for free businesses, but max buy amount checks need a cost when the business is available in the store.", "cost", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _costInfo;
        [Tooltip("Inputs paid to buy one base copy of this business. Leave empty for a free business; percent buy options need at least one cost to calculate affordability.")]
        public List<Input> cost;
        [Tooltip("Geometric cost multiplier applied after each obtained copy. If useGeometricBoughtAmount is enabled, only bought copies affect this scaling.")]
        public float costMultiplier = 1;

        [Tooltip("When enabled, the geometric cost formula uses only the amount bought via TryBuyItems, " +
            "ignoring free grants from outputs or other sources. Useful when businesses can be awarded freely " +
            "and you do not want those grants to raise the purchase price.")]
        public bool useGeometricBoughtAmount = false;

        [Space(16)]
        [Header("Production")] // ------------------------------- PRODUCTION ------------------------------- 
        [CommentArea("Production", "Businesses can produce Outputs over time. If 'scaleProductionCostWithAmount' is true, the productionCost is multiplied by the amount of businesses owned. 'allowPartialProductionCost' can start production for only the affordable amount. 'autoProduce' runs production independently of managers. 'productionRoundMode' controls whether new copies update the current round, wait for the next round, or start concurrent rounds.", "For a simple clicker business, set timeToProduce to 3, add one Coin output table, leave productionCost empty, and wire the BusinessDisplayer main button to Click(). For an automated business, assign a Manager or enable autoProduce.")]
        [SerializeField] private string _productionComment;

        [Min(.01f)]
        [Tooltip("Seconds required for one production round. Lower values produce more often; values below 0.01 are blocked by the Inspector minimum.")]
        public float timeToProduce = 1;

        [Tooltip("How output chances are evaluated when multiple business copies produce together. Evaluate Once creates all-or-nothing batches; Evaluate Per Instance normalizes results across copies.")]
        public DropEvaluationMode dropEvaluationMode = DropEvaluationMode.EvaluateOnce;

        [Tooltip("If enabled, this business starts production automatically without requiring a manager. Assigned manager boosts and thresholds can still apply.")]
        public bool autoProduce = false;

        [Tooltip("Manager that can automate and boost this business. Leave null if the business has no manager-specific automation or bonuses.")]
        public Manager manager;

        [Tooltip("Controls how owned copies are assigned to production rounds. Single Round Live Amount folds new copies into the current round; Single Round Snapshot waits until the next round; Concurrent Rounds lets idle copies start separate rounds.")]
        public ProductionRoundMode productionRoundMode = ProductionRoundMode.SingleRoundLiveAmount;

        [HideInInspector]
        [System.Obsolete("Use productionRoundMode instead.")]
        public bool increaseProductionAmountAfterRoundEnd = false;

        [HideInInspector]
        [SerializeField]
        private bool syncedIncreaseProductionAmountAfterRoundEnd = false;

        [HideInInspector]
        [SerializeField]
        private bool productionRoundModeMigrated = false;

        [HideInInspector]
        [SerializeField]
        private bool allowConcurrentProductionRounds = false;

        [Tooltip("If enabled, produced outputs are applied automatically when a round finishes. Disable to store outputs until the player manually claims them.")]
        public bool autoCollect = true;

        [ConditionalCommentArea("Business setup", "No outputs are defined. This business can still exist, but it will not produce anything.", "outputTables", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _outputTablesInfo;
        [Tooltip("Drop tables produced by this business each round. Empty businesses produce nothing; use multiple tables for separate guaranteed and weighted output pools.")]
        public List<DropTable> outputTables = new List<DropTable>();

        [HideInInspector]
        [Tooltip("Legacy list of outputs that will be produced by this business. Use outputTables instead.")]
        public List<Output> outputs;

        [Tooltip("Inputs consumed when a production round starts. Leave empty for free production; combine with allowPartialProductionCost for input-limited factories.")]
        public List<Input> productionCost;

        [Tooltip("If enabled, productionCost is multiplied by the number of copies producing. If disabled, the base productionCost is paid once while outputs still scale by producing amount.")]
        public bool scaleProductionCostWithAmount;

        [Tooltip("If enabled, production starts for the largest affordable copy count when the full scaled productionCost cannot be paid. Requires scaleProductionCostWithAmount to matter.")]
        public bool allowPartialProductionCost;

        [Tooltip("If enabled, producing copies are removed after their outputs are applied. Use for consumable generators or merge-style recipes.")]
        public bool consumeOnProduce;

        [Tooltip("Multiplier sources ignored when calculating this business production. Use to opt out of boosts, prestige, upgrades, or manager effects for special businesses.")]
        public MultiplierSource ignoredMultipliers;

        [Tooltip("Requirements that must be met for this business to produce outputs. If these conditions are not met, production pauses and new rounds cannot start. This is checked continuously.")]
        public List<Lock> productionLocks;

        [Tooltip("Upper-bound requirements that must be met for this business to produce outputs. If these conditions are not met, production pauses. This is checked continuously.")]
        public List<Lock> upperProductionLocks;

        public List<Lock> UpperProductionLocks => upperProductionLocks;

        [Header("Locks")]
        [CommentArea("Unlock Conditions", "A list of 'Lock' requirements that must be met before this business is available. If 'unlockOnlyOnce' is true, the business stays unlocked permanently even if the requirements later become false, such as after spending resources.", "Use locks to reveal a business after the player owns 25 Coins or reaches player level 3. Use upperLocks to hide early-game businesses once the player passes a threshold.")]
        [SerializeField] private string _locksComment;
        [Tooltip("If enabled, this business stays unlocked after its locks are met once, even if the player later spends the required resources.")]
        public bool unlockOnlyOnce = true;

        [ConditionalCommentArea("Business setup", "One or more locks have amount 0 or less. Those locks will be satisfied immediately.", "locks", ConditionalCommentAreaMode.AnyListBigNumberLessThanOrEqual, "inputAmount", 0)]
        [SerializeField] private string _locksInfo;
        [Tooltip("Requirements that must be met before this business can be shown or bought. Empty list means it is available immediately.")]
        public List<Lock> locks;

        [Tooltip("Upper-bound requirements that keep this business visible only while they remain under or equal to their threshold. Use to hide early-game content after progression.")]
        public List<Lock> upperLocks;

        [Header("Leveling - player")]  // ------------------------------- LEVELING - PLAYER ------------------------------- 
        [Tooltip("Player XP added whenever copies of this business are obtained. Applies to purchases and free grants from outputs or rewards.")]
        public int xpsAddPerPurchaseToPlayer = 1;

        [Header("Leveling - business")] // ------------------------------- LEVELING - BUSINESS ------------------------------- 
        [Tooltip("Rewards and descriptions applied when this business levels up. Leave empty if business levels should not grant rewards or labels.")]
        public LevelDataHolder levelupData;

        [Tooltip("If enabled, business level rewards are multiplied by the reached level before being granted.")]
        public bool scaleWithLevel;

        [Tooltip("Owned copies required for the first business level-up.")]
        public int firstLevelCopies = 100;

        [Tooltip("Multiplier applied to the required copy threshold after each business level-up.")]
        public float incrementMultiplier = 2;

        [Tooltip("If enabled, this business levels up automatically when enough copies are owned. Disable if your UI should let the player claim level-ups manually.")]
        public bool autoLevelUp = true;

        public BigNumber PriceMultiplier => GetBusinessMultiplierOfType(UpgradeType.purchaseCost) * UpgradeEffectResolver.GetPurchaseCostMultiplier(this);

        public BigNumber GetBusinessMultiplierOfType(UpgradeType type)
        {
            BigNumber multiplier = 1;

            if (type != UpgradeType.production || !ignoredMultipliers.HasFlag(MultiplierSource.Boosts))
                multiplier *= BoostsManager.GetMultiplierOfTypeStatic(type, GetEffectiveBusinessGroups(), GetEffectiveBusinessGroupName());

            if (type != UpgradeType.production || !ignoredMultipliers.HasFlag(MultiplierSource.Prestige))
                multiplier *= PrestigeManager.GetPrestigeMultiplierOfType(type);

            if (type != UpgradeType.production || !ignoredMultipliers.HasFlag(MultiplierSource.Upgrades))
                multiplier *= UpgradesManager.GetMultiplierOfTypeStatic(type, GetEffectiveBusinessGroups(), GetEffectiveBusinessGroupName());

            if (type != UpgradeType.production || !ignoredMultipliers.HasFlag(MultiplierSource.Managers))
            {
                multiplier *= ManagersManager.GetPassiveMultiplierOfType(type, this);
                multiplier *= ManagersManager.GetActiveMultiplierOfType(type, this);
            }

            return multiplier;
        }

        bool IBuyable.CanPay(BigNumber amountToBuy, BigNumber currentAmount)
        {
            return cost.IsSufficent(amountToBuy, currentAmount, CostMultiplier, PriceMultiplier);
        }

        void IBuyable.Pay(BigNumber amountToBuy, BigNumber currentAmount) => cost.RemoveInputs(amountToBuy, currentAmount, CostMultiplier, PriceMultiplier);

        [Header("Upgrading")]  // ------------------------------- LEVELING - PLAYER ------------------------------- 
        [CommentArea("Upgrade Levels", "Optional override data for specific business levels. Reaching a configured level can change the display name, icon, model, description, max amount, or max buyable amount.", "Use this for visual progression: level 1 shows a Lemonade Stand, level 5 overrides the image/name to Lemonade Shop, and level 10 raises the max amount.")]
        [SerializeField] private string _upgradeLevelsComment;
        [Tooltip("Per-level visual or limit overrides for this business. Empty array means all levels use the base business settings.")]
        public BusinessUpgradeLevel[] upgradeLevels = new BusinessUpgradeLevel[0];

        [Tooltip("If enabled, upgrade levels are applied automatically for free when their requirements are met.")]
        public bool autoUpgradeForFree = true;

        [Tooltip("If enabled, the first upgrade level is granted without charging upgradeCost. Later manual upgrades still use upgradeCost unless autoUpgradeForFree handles them.")]
        public bool autoUpgradeToFirstLevelFree = true;

        [Tooltip("Inputs paid when manually upgrading this business. Leave empty for free manual upgrades.")]
        public List<Input> upgradeCost;

        [Min(0.01f)]
        [Tooltip("Geometric multiplier applied to upgradeCost after each business upgrade level.")]
        public int upgradeCostMultiplier = 2;

        //[Header("Experimental")]

        [Obsolete("Use iProducable.IsProductionCostSufficent(BigNumber amount) instead.")]
        public bool IsProductionCostSufficent(BigNumber amount)
        {
            if (!scaleProductionCostWithAmount) amount = 1;

            // the production cost is not supposed to scale in any way
            return productionCost.IsSufficent(amount, 0, 1, 1);
        }

        public BigNumber GetMaxBuyableAmount(BigNumber currentAmount)
        {
            if (Cost.Count == 0)
                throw new Exception("Business cost is empty! Cannot calculate max buyable amount.");

            return Cost.Min(x =>
            {
                return x.GetMaxBuyAmount(PriceMultiplier, currentAmount, CostMultiplier);
            }).Floor();
        }

        public string GetIconString()
        {
            return GetEffectiveMetadata(null).GetIconString();
        }

        public Metadata GetEffectiveMetadata() => GetEffectiveMetadata(TryGetRuntimeHolder());

        public Metadata GetEffectiveMetadata(BusinessHolder holder)
        {
            Metadata baseMetadata = GetEffectiveBaseMetadata();
            BusinessUpgradeLevel upgradeLevel = holder != null && holder.business == this
                ? holder.iUpgradable.GetCurrentUpgradeLevel() as BusinessUpgradeLevel
                : null;

            return upgradeLevel == null ? baseMetadata : MergeMetadata(baseMetadata, upgradeLevel.Metadata);
        }

        public GameObject GetEffectiveModel() => GetEffectiveModel(TryGetRuntimeHolder());

        public GameObject GetEffectiveModel(BusinessHolder holder) => GetEffectiveMetadata(holder)?.model;

        public List<BusinessGroup> GetEffectiveBusinessGroups()
        {
            if (businessGroups != null && businessGroups.Count > 0) return businessGroups;

            if (this is BusinessWrapper wrapper && wrapper.underlyingBusiness != null && wrapper.underlyingBusiness != this)
                return wrapper.underlyingBusiness.GetEffectiveBusinessGroups();

            return businessGroups;
        }

        public string GetEffectiveBusinessGroupName()
        {
            if (!string.IsNullOrEmpty(businessGroup)) return businessGroup;

            if (this is BusinessWrapper wrapper && wrapper.underlyingBusiness != null && wrapper.underlyingBusiness != this)
                return wrapper.underlyingBusiness.GetEffectiveBusinessGroupName();

            return businessGroup;
        }

        private Metadata GetEffectiveBaseMetadata()
        {
            Metadata ownMetadata = Metadata;

            if (this is BusinessWrapper wrapper && wrapper.underlyingBusiness != null && wrapper.underlyingBusiness != this)
                return MergeMetadata(wrapper.underlyingBusiness.GetEffectiveBaseMetadata(), ownMetadata);

            return ownMetadata;
        }

        private BusinessHolder TryGetRuntimeHolder()
        {
            if (!Application.isPlaying) return null;

            BusinessesManager manager = UnityEngine.Object.FindObjectOfType<BusinessesManager>();
            return manager != null ? manager.GetHolder(this) : null;
        }

        private static Metadata MergeMetadata(Metadata fallback, Metadata preferred)
        {
            if (fallback == null && preferred == null) return new Metadata();
            if (fallback == null) fallback = new Metadata();
            if (preferred == null) preferred = new Metadata();

            return new Metadata
            {
                name = string.IsNullOrWhiteSpace(preferred.name) ? fallback.name : preferred.name,
                icon = preferred.icon ? preferred.icon : fallback.icon,
                iconString = string.IsNullOrWhiteSpace(preferred.iconString) ? fallback.iconString : preferred.iconString,
                model = preferred.model ? preferred.model : fallback.model,
                description = string.IsNullOrWhiteSpace(preferred.description) ? fallback.description : preferred.description
            };
        }

        public Metadata GetMetadata()
        {
            EasyIdleGame.Metadata.SyncLegacyFields(ref metadata, ref metadataLegacySyncState, businessName, image, iconString, model, description);
            businessName = metadata.name;
            image = metadata.icon;
            iconString = metadata.iconString;
            model = metadata.model;
            description = metadata.description;
            metadataLegacySyncState.Capture(metadata);
            return metadata;
        }

        public static bool HasGroupMatch(List<BusinessGroup> sourceGroups, string sourceGroupString, List<BusinessGroup> targetGroups, string targetGroupString)
        {
            if (string.IsNullOrEmpty(targetGroupString) && (targetGroups == null || targetGroups.Count == 0))
                return true;

            // Obsolete string check happens first. If used, lists are ignored.
            if (!string.IsNullOrEmpty(targetGroupString))
            {
                if (!string.IsNullOrEmpty(sourceGroupString)) return targetGroupString == sourceGroupString;

                if (sourceGroups != null && sourceGroups.Any(bg => bg != null && bg.name == targetGroupString)) return true;

                return false;
            }

            // Normal list check happens only when string is null/empty
            if (targetGroups != null && targetGroups.Count > 0)
            {
                if (!string.IsNullOrEmpty(sourceGroupString)) return targetGroups.Any(bg => bg != null && bg.name == sourceGroupString);

                if (sourceGroups != null && targetGroups.Intersect(sourceGroups).Any()) return true;
            }

            return false;
        }

        public bool HasGroupMatch(List<BusinessGroup> targetGroups, string targetGroupString)
        {
            return HasGroupMatch(GetEffectiveBusinessGroups(), GetEffectiveBusinessGroupName(), targetGroups, targetGroupString);
        }

        /// <summary> applies outputs of the business (such as adding currency that is in output) </summary>
        [Obsolete("Use iProducable.ApplyOuputs(BigNumber amount) instead.")]
        public List<Output> ApplyOutputs(BigNumber amount) => outputTables.ApplyOutputs(amount);

        public void OnBeforeSerialize()
        {
            GetMetadata();
            SyncUpgradeLevelMetadata();

            if (!productionRoundModeMigrated)
                productionRoundMode = GetProductionRoundMode();

            productionRoundModeMigrated = true;
#pragma warning disable 0618
            increaseProductionAmountAfterRoundEnd = productionRoundMode == ProductionRoundMode.SingleRoundSnapshot;
            syncedIncreaseProductionAmountAfterRoundEnd = increaseProductionAmountAfterRoundEnd;
#pragma warning restore 0618
            allowConcurrentProductionRounds = productionRoundMode == ProductionRoundMode.ConcurrentRounds;
        }

        public void OnAfterDeserialize()
        {
            GetMetadata();
            SyncUpgradeLevelMetadata();

            if (!productionRoundModeMigrated)
            {
                productionRoundMode = GetProductionRoundMode();
                productionRoundModeMigrated = true;
#pragma warning disable 0618
                syncedIncreaseProductionAmountAfterRoundEnd = increaseProductionAmountAfterRoundEnd;
#pragma warning restore 0618
            }

            if (outputs != null && outputs.Count > 0)
            {
                if (outputTables == null) outputTables = new List<DropTable>();

                DropTable table = new DropTable();
                table.strategy = DropStrategy.All;
                table.outputs = new List<Output>(outputs);

                outputTables.Add(table);
                outputs.Clear();
            }
        }

        private ProductionRoundMode GetProductionRoundMode()
        {
            if (productionRoundModeMigrated)
            {
#pragma warning disable 0618
                if (increaseProductionAmountAfterRoundEnd != syncedIncreaseProductionAmountAfterRoundEnd)
                {
                    productionRoundMode = increaseProductionAmountAfterRoundEnd
                        ? ProductionRoundMode.SingleRoundSnapshot
                        : ProductionRoundMode.SingleRoundLiveAmount;
                    syncedIncreaseProductionAmountAfterRoundEnd = increaseProductionAmountAfterRoundEnd;
                }
#pragma warning restore 0618

                return productionRoundMode;
            }

            if (allowConcurrentProductionRounds) return ProductionRoundMode.ConcurrentRounds;
#pragma warning disable 0618
            if (increaseProductionAmountAfterRoundEnd) return ProductionRoundMode.SingleRoundSnapshot;
#pragma warning restore 0618

            return productionRoundMode;
        }

        private void SyncUpgradeLevelMetadata()
        {
            if (upgradeLevels == null) return;

            foreach (BusinessUpgradeLevel upgradeLevel in upgradeLevels)
            {
                upgradeLevel?.GetMetadata();
            }
        }
    }
}
