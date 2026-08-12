using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that handles businesses
    /// </summary>
    [AddComponentMenu("EasyIdleGame/Managers/Businesses Manager")]
    public class BusinessesManager : ManagerHolderBase<BusinessesManager, BusinessHolder, Business>,
        IBuyableHolderManager<BusinessesManager, BusinessHolder, Business>,
        IUnlockableHolderManager<BusinessHolder, Business>,
        ILevelupableHolderManager<BusinessHolder, Business>,
        IProducableHolderManager<BusinessHolder, Business>,
        IUpgradableHolderManager<BusinessHolder, Business>
    {
        public IBuyableHolderManager<BusinessesManager, BusinessHolder, Business> iBuyable => this;
        public IUnlockableHolderManager<BusinessHolder, Business> iUnlockable => this;
        public ILevelupableHolderManager<BusinessHolder, Business> iLevelupable => this;
        public IProducableHolderManager<BusinessHolder, Business> iProducable => this;
        public IUpgradableHolderManager<BusinessHolder, Business> iUpgradable => this;

        public UnityEvent<Business> OnItemUnlocked { get; } = new UnityEvent<Business>();
        public UnityEvent<BusinessHolder> OnHolderBoughtEvent { get; } = new UnityEvent<BusinessHolder>();
        public UnityEvent<BusinessHolder> OnHolderUnlockedEvent { get; } = new UnityEvent<BusinessHolder>();
        public UnityEvent<BusinessHolder> OnHolderLeveledUpEvent { get; } = new UnityEvent<BusinessHolder>();
        public UnityEvent<BusinessHolder> OnHolderUpgradedEvent { get; } = new UnityEvent<BusinessHolder>();
        public UnityEvent<BusinessHolder> OnHolderProductionStartedEvent { get; } = new UnityEvent<BusinessHolder>();
        public UnityEvent<BusinessHolder> OnHolderProductionFinishedEvent { get; } = new UnityEvent<BusinessHolder>();
        public UnityEvent<BusinessHolder, BigNumber, object> OnBusinessAmountAddedEvent { get; } = new UnityEvent<BusinessHolder, BigNumber, object>();
        public UnityEvent<BusinessHolder, BigNumber, object, SourceType> OnBusinessAmountAddedWithSourceEvent { get; } = new UnityEvent<BusinessHolder, BigNumber, object, SourceType>();
        public UnityEvent<BusinessHolder, BigNumber, object> OnBusinessAmountRemovedEvent { get; } = new UnityEvent<BusinessHolder, BigNumber, object>();
        public UnityEvent<BusinessMergeRecipe> OnBusinessesMerged { get; } = new UnityEvent<BusinessMergeRecipe>();

        public BusinessesManager() { _necessary = true; }

        public void NotifyBusinessUpgradeChanged(BusinessHolder holder)
        {
            if (holder == null || holder.business == null) return;

            OnHolderUpgradedEvent?.Invoke(holder);
            OnBusinessUpgradeChanged?.Invoke(holder.business);
            EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(BusinessesManager));
        }

        [Header("Settings")]
        [CommentArea("Buying Settings", "This defines the cycle of buying amounts a player can toggle between, such as Buy x1, Buy x10, or Buy Max. When 'ChangeBuyAmount' is called, it cycles to the next entry in 'buyAmounts'.", "Place one BusinessesManager in the gameplay scene. Add BuyAmount entries like x1, x10, and 100%, then wire BuyAmountButtonDisplayer.OnClick to ChangeBuyAmount through the displayer.")]
        [SerializeField] private string _buyingSettingsComment;
        [ConditionalCommentArea("Manager setup", "No buy amounts are configured. Buying will fall back to x1 and the buy amount button will have nothing to cycle through.", "buyAmounts", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _buyAmountsInfo;
        [Tooltip("Purchase amount options cycled by ChangeBuyAmount. Empty array falls back to Buy x1 and gives the UI nothing to cycle.")]
        public BuyAmount[] buyAmounts;

        [Header("Auto")]
        private int buyAmountId = 0;

        [Tooltip("Invoked when a business amount changes from buying, removal, production consumption, or free grants.")]
        public UnityEvent<Business> OnBusinessAmountChanged = new UnityEvent<Business>();

        [Tooltip("Invoked after a business is bought through the normal purchase flow.")]
        public UnityEvent<Business> OnBusinessBought = new UnityEvent<Business>();

        [Tooltip("Invoked after a business upgrade level changes.")]
        public UnityEvent<Business> OnBusinessUpgradeChanged = new UnityEvent<Business>();

        public UnityEvent<Business> OnItemBought => OnBusinessBought;

        [Tooltip("Invoked when a business holder finishes a production round and outputs are ready or applied.")]
        public UnityEvent<BusinessHolder> OnProductionFinishedEvent = new UnityEvent<BusinessHolder>();

        [Tooltip("Invoked when a business holder starts a production round after paying any productionCost.")]
        public UnityEvent<BusinessHolder> OnProductionStartedEvent = new UnityEvent<BusinessHolder>();

        public UnityEvent<BusinessHolder, ProductionRound> OnProductionRoundStartedEvent = new UnityEvent<BusinessHolder, ProductionRound>();
        public UnityEvent<BusinessHolder, ProductionRound> OnProductionRoundFinishedEvent = new UnityEvent<BusinessHolder, ProductionRound>();

        private static BusinessMergeRecipe[] _mergeRecipeCache;

        public override BusinessHolder GetOrAddHolder(Business business)
        {
            if (business == null) throw new Exception("Business is null");

            BusinessHolder h = GetHolder(business);
            if (h == null)
            {
                holders.Add(h = new BusinessHolder(business, 0));
                OnHolderAdded?.Invoke(h);
            }
            return h;
        }

        protected override void NotifyHolderReset(BusinessHolder holder)
        {
            if (holder?.business == null) return;
            if (holder.amount <= 0 && holder.totalAmount <= 0 && holder.boughtAmount <= 0) return;

            OnBusinessAmountChanged?.Invoke(holder.business);
        }

        /// <returns> removes 'amount' of 'business' </returns>
        public void RemoveBusiness(Business business, BigNumber amount, object context = null)
        {
            BusinessHolder h = GetHolder(business) ?? throw new Exception("Business was not added yet, therefore can't be removed");
            h.RemoveAmount(amount, context);
        }

        /// <summary> Changes to next purchase multiplier </summary>
        public void ChangeBuyAmount()
        {
            buyAmountId++;
            if (buyAmountId >= buyAmounts.Length) buyAmountId = 0;
        }

        public BigNumber GetTotalProductionPerSecond(Currency currency, bool automatic = true, bool ignoreProductionAmount = false)
            => GetTotalProductionPerSecond(currency, null, automatic, ignoreProductionAmount);

        public BigNumber GetTotalProductionPerSecond(Business business, bool automatic = true, bool ignoreProductionAmount = false)
            => GetTotalProductionPerSecond(null, business, automatic, ignoreProductionAmount);

        public BigNumber GetTotalProductionPerSecond(Currency currency, Business business, bool automatic = true, bool ignoreProductionAmount = false)
        {
            BigNumber totalPerSecond = new BigNumber(0);

            foreach (BusinessHolder businessHolder in holders)
            {
                if (automatic && !businessHolder.IsAutoProductionPossible()) continue;

                foreach (Output output in businessHolder.iProducable.GetActualProductionPerSecond(ignoreProductionAmount))
                {
                    if ((currency && output.currencyOutput != currency) ||
                        (business && output.businessOutput != business)) continue;

                    totalPerSecond += output.GetAverageAmount();
                }
            }

            return totalPerSecond;
        }

        public BigNumber GetCurrentProductionPerSecond(Currency currency)
            => GetCurrentProductionPerSecond(currency, null);

        public BigNumber GetCurrentProductionPerSecond(Business business)
            => GetCurrentProductionPerSecond(null, business);

        public BigNumber GetCurrentProductionPerSecond(Currency currency = null, Business business = null)
        {
            BigNumber totalPerSecond = new BigNumber(0);

            foreach (BusinessHolder businessHolder in holders)
            {
                if (businessHolder.ActiveProductions.Count == 0) continue;

                foreach (Output output in businessHolder.iProducable.GetCurrentProductionPerSecond())
                {
                    if ((currency && output.currencyOutput != currency) ||
                        (business && output.businessOutput != business)) continue;

                    totalPerSecond += output.GetAverageAmount();
                }
            }

            return totalPerSecond;
        }

        /// <returns> current purchase multiplier </returns>
        public BuyAmount GetBuyAmount()
        {
            if (buyAmounts.Length == 0) return new BuyAmount(1, BuyAmount.Type.digit);

            return buyAmounts[buyAmountId];
        }

        /// <returns> puchase multiplier tailored for business </returns>
        public BigNumber GetTargetBusinessBuyAmount(Business business)
        {
            BusinessHolder holder = GetOrAddHolder(business);
            if (!holder.iUnlockable.IsUnlocked()) return 0;

            BigNumber remainingBuyableAmount = holder.GetRemainingBuyableAmountConstrain();
            if (remainingBuyableAmount <= 0) return 0;

            BigNumber bAmount = GetTargetBusinessBuyAmount_Prev(business);

            if (bAmount == 0) bAmount = 1;

            return BigNumber.Min(bAmount, remainingBuyableAmount);
        }

        private BigNumber GetTargetBusinessBuyAmount_Prev(Business business)
        {
            if (business.cost.Count == 0 || buyAmounts.Length == 0) return 0;
            if (buyAmounts[buyAmountId].type == BuyAmount.Type.digit) return buyAmounts[buyAmountId].amount;

            BigNumber fAmount = business.GetMaxBuyableAmount(iBuyable.GetItemAmount(business)) * buyAmounts[buyAmountId].amount / 100f;

            return fAmount.Floor() < 0 ? 0 : fAmount.Floor();
        }

        private float _timeSinceLastCheck = 0f;
        private void Update()
        {
            for (int i = 0; i < holders.Count; i++)
            {
                holders[i].HandleUpdate();
            }

            _timeSinceLastCheck += Time.deltaTime;
            if (_timeSinceLastCheck >= 0.2f)
            {
                _timeSinceLastCheck = 0f;
                iUnlockable.CheckAutoUnlocks();
            }
        }

        public void ForceBuyItemsForFree(Business item, BigNumber amount, SourceType sourceType = SourceType.Generic)
        {
            GetOrAddHolder(item).AddAmount(amount, sourceType: sourceType);
            EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(BusinessesManager));
        }

        public static bool TryGetMergeRecipe(Business a, Business b, out BusinessMergeRecipe recipe)
        {
            if (TryGetMergeRecipe(new[]
            {
                new Input { businessInput = a, inputAmount = 1 },
                new Input { businessInput = b, inputAmount = 1 }
            }, out recipe))
            {
                return true;
            }

            if (a != b)
                return false;

            return TryGetMergeRecipe(new[]
            {
                new Input { businessInput = a, inputAmount = 2 }
            }, out recipe);
        }

        public static void RefreshMergeRecipeCache()
        {
            _mergeRecipeCache = Resources.LoadAll<BusinessMergeRecipe>("");
        }

        public static bool TryGetMergeRecipe(IEnumerable<Input> inputs, out BusinessMergeRecipe recipe)
        {
            recipe = null;

            if (inputs == null)
                return false;

            List<Input> inputList = inputs.Where(input => input != null).ToList();

            if (_mergeRecipeCache == null)
                RefreshMergeRecipeCache();

            foreach (BusinessMergeRecipe candidate in _mergeRecipeCache)
            {
                if (InputsMatch(candidate.inputs, inputList))
                {
                    recipe = candidate;
                    return true;
                }
            }

            return false;
        }

        private static bool InputsMatch(IReadOnlyList<Input> recipeInputs, IReadOnlyList<Input> inputs)
        {
            if (recipeInputs == null || inputs == null || recipeInputs.Count != inputs.Count)
                return false;

            bool[] matchedInputs = new bool[inputs.Count];

            for (int recipeIndex = 0; recipeIndex < recipeInputs.Count; recipeIndex++)
            {
                bool foundMatch = false;

                for (int inputIndex = 0; inputIndex < inputs.Count; inputIndex++)
                {
                    if (matchedInputs[inputIndex])
                        continue;

                    if (!InputMatches(recipeInputs[recipeIndex], inputs[inputIndex]))
                        continue;

                    matchedInputs[inputIndex] = true;
                    foundMatch = true;
                    break;
                }

                if (!foundMatch)
                    return false;
            }

            return true;
        }

        private static bool InputMatches(Input a, Input b)
        {
            if (a == null || b == null)
                return false;

            return a.businessInput == b.businessInput &&
                a.currencyInput == b.currencyInput &&
                a.inputAmount == b.inputAmount;
        }

        public bool CanMergeBusinesses(BusinessMergeRecipe recipe)
        {
            foreach (Input input in GetModifiedMergeInputs(recipe))
            {
                if (!input.IsThisInputSufficent())
                    return false;
            }
            return true;
        }

        public List<Input> GetModifiedMergeInputs(BusinessMergeRecipe recipe)
        {
            List<Input> modifiedInputs = new List<Input>();
            if (recipe == null || recipe.inputs == null) return modifiedInputs;

            foreach (Input input in recipe.inputs)
            {
                if (input == null) continue;
                BigNumber multiplier = UpgradeEffectResolver.GetMergeInputCostMultiplier(recipe, input);
                modifiedInputs.Add(new Input
                {
                    businessInput = input.businessInput,
                    currencyInput = input.currencyInput,
                    inputAmount = input.inputAmount * multiplier
                });
            }

            return modifiedInputs;
        }

        public float GetEffectiveMergeDuration(BusinessMergeRecipe recipe)
        {
            if (recipe == null || recipe.mergeDurationSeconds <= 0) return 0;
            BigNumber speed = UpgradeEffectResolver.GetMergeSpeedMultiplier(recipe);
            if (speed <= 0) return recipe.mergeDurationSeconds;
            return (float)(recipe.mergeDurationSeconds / speed).ToDouble();
        }

        public float GetEffectiveMergeCooldown(BusinessMergeRecipe recipe)
        {
            if (recipe == null || recipe.mergeCooldownSeconds <= 0) return 0;
            BigNumber multiplier = UpgradeEffectResolver.GetMergeCooldownMultiplier(recipe);
            return Mathf.Max(0f, (float)(recipe.mergeCooldownSeconds * multiplier).ToDouble());
        }

        public List<Output> ForceMergeBusinesses(BusinessMergeRecipe recipe)
        {
            foreach (Input input in GetModifiedMergeInputs(recipe))
            {
                if (input.businessInput != null) GetOrAddHolder(input.businessInput).RemoveAmount(input.inputAmount);
                if (input.currencyInput != null) CurrencyManager.Instance.RemoveCurrencyAmount(input.currencyInput, input.inputAmount);
            }

            UpgradeEffectContext context = new UpgradeEffectContext
            {
                Source = SourceType.Conversion,
                TargetMergeRecipe = recipe
            };
            List<Output> outputs = RewardCalculator.ApplyOutputs(recipe.outputTables, 1, context);
            EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(BusinessesManager));
            return outputs;
        }

        public bool TryMergeBusinesses(BusinessMergeRecipe recipe, out List<Output> outputs)
        {
            outputs = null;
            if (!CanMergeBusinesses(recipe)) return false;

            outputs = ForceMergeBusinesses(recipe);
            OnBusinessesMerged?.Invoke(recipe);
            return true;
        }

        public BigNumber GetBusinessGroupUsedCapacity(BusinessGroup group)
        {
            if (group == null) return 0;
            BigNumber sum = 0;
            foreach (var holder in holders)
            {
                List<BusinessGroup> groups = holder.business.GetEffectiveBusinessGroups();
                if (groups != null && groups.Contains(group))
                {
                    sum += holder.amount;
                }
            }
            return sum;
        }

        public BigNumber GetBusinessGroupAmount(BusinessGroup group, LockAmountType amountType)
        {
            if (group == null) return 0;
            BigNumber sum = 0;
            foreach (var holder in holders)
            {
                List<BusinessGroup> groups = holder.business.GetEffectiveBusinessGroups();
                if (groups != null && groups.Contains(group))
                {
                    sum += amountType switch
                    {
                        LockAmountType.TotalAmount => holder.TotalAmount,
                        LockAmountType.BoughtAmount => holder.BoughtAmount,
                        LockAmountType.MaxAmount => holder.GetMaxAmountConstrain(),
                        _ => holder.Amount,
                    };
                }
            }
            return sum;
        }

        public void PauseAllProductions()
        {
            foreach (var holder in holders)
            {
                holder.PauseAllProductionRounds();
            }
        }

        public void UnpauseAllProductions()
        {
            foreach (var holder in holders)
            {
                holder.UnpauseAllProductionRounds();
            }
        }
    }
}
