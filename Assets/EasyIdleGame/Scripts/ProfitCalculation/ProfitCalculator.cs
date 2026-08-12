using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that handles profit calculation
    /// </summary>
    public static class ProfitCalculator
    {
        private const int MAX_SEGMENTS = 30;
        public static readonly UnityEvent<ProfitApplicationSummary> OnProfitApplied = new UnityEvent<ProfitApplicationSummary>();

        public static ProfitApplicationSummary CalculateAndApplyProfit(TimeSpan elapsedTime)
        {
            return ApplyProfit(CalculateProfit(elapsedTime));
        }

        /// <summary>
        /// calculation only, no changes are applied
        /// time boosts are simulated as well (time will be subtracted from them)
        /// simulateBoosts: if enabled, elapsed time will be substracted from all active boosts
        /// </summary>
        public static ProfitData CalculateProfit(TimeSpan elapsedTime, bool simulateBoosts = true, bool simulateActiveManagers = true)
        {
            if (elapsedTime.TotalSeconds <= 0)
            {
                ProfitData emptyData = new(elapsedTime);
                emptyData.updatedBoosts = BoostsManager.Instance ? BoostsManager.Instance.activeBoosts : new List<ActiveBoostHolder>();
                return emptyData;
            }

            int segments = Mathf.Clamp((int)elapsedTime.TotalSeconds, 1, MAX_SEGMENTS);
            float segmentLength = (float)elapsedTime.TotalSeconds / segments;

            List<BusinessHolder> businessHolders = BusinessesManager.Instance.holders.Select(h =>
            {
                var clone = new BusinessHolder(h.business, h.amount)
                {
                    count = h.count,
                    upgradableLevel = h.upgradableLevel,
                    currentUpgradeCostMultiplier = h.currentUpgradeCostMultiplier,
                    totalCount = h.totalCount,
                    totalAmount = h.totalAmount,
                    boughtAmount = h.boughtAmount,
                    unlockedOnce = h.unlockedOnce
                };

                clone.activeProductions = h.activeProductions.Select(p => new ProductionRound() { productionAmount = p.productionAmount, productionTime = p.productionTime }).ToList();
                return clone;
            }).ToList();

            foreach (var item in businessHolders)
            {
                item.ProductionAmount = item.amount;
            }

            List<Business> businessesToAdd = new List<Business>();
            foreach (var item in businessHolders)
            {
                foreach (var table in item.business.OutputTables)
                {
                    foreach (var output in table.outputs)
                    {
                        if (output.businessOutput != null && !businessesToAdd.Contains(output.businessOutput) && !businessHolders.Any(x => x.business == output.businessOutput))
                        {
                            businessesToAdd.Add(output.businessOutput);
                        }
                    }
                }
            }

            foreach (var business in businessesToAdd)
                businessHolders.Add(new BusinessHolder(business, 0));

            BigNumber[] progressions = new BigNumber[businessHolders.Count];

            // copy boosts holders, reduce time left on the original boosts and after the calculation, set them back to the original values
            List<ActiveBoostHolder> boostsSave = BoostsManager.Instance ? BoostsManager.Instance.activeBoosts.Select(boost => boost.Clone()).ToList() : new List<ActiveBoostHolder>();

            // save managers' active/cooldown timers, since OnTimePassed() below mutates the real, live holders directly (there is no simulated clone to run it against, as the multiplier lookups it feeds read from ManagersManager.Instance itself)
            List<(ManagerHolder holder, double activeTimeLeft, double cooldownTimeLeft)> managersSave = ManagersManager.Instance ? ManagersManager.Instance.holders.Select(h => (h, h.activeTimeLeft, h.cooldownTimeLeft)).ToList() : new List<(ManagerHolder, double, double)>();

            ProfitData data1 = new(elapsedTime);

            Dictionary<Business, BusinessHolder> businesses_ = businessHolders.ToDictionary();
            Dictionary<Currency, CurrencyHolder> currencies_ = CurrencyManager.Instance.holders.CloneToDictionary();

            // to increase performance
            Dictionary<Business, bool> managerIsUnlocked = new();

            for (int z = 0; z < segments; z++)
            {
                //Debug.Log($"--- SEGMENT {z}, duration: {segmentLength}");

                for (int j = 0; j < businessHolders.Count; j++)
                {
                    //Debug.Log($"    {z} Business: {businessHolders[j].business.name}, Amount: {businessHolders[j].ProductionAmount}, Progression: {progressions[j]}");

                    if (!CanAutoProduce(businessHolders[j], businesses_, currencies_, managerIsUnlocked))
                    {
                        progressions[j] = 0;
                        if (!data1.businessProductionTime.TryAdd(businessHolders[j].business, progressions[j])) data1.businessProductionTime[businessHolders[j].business] = progressions[j];

                        //Debug.Log($"    Skipping business");
                        continue;
                    }

                    //Debug.Log($"    progression += {segmentLength} * {businessHolders[j].GetSpeedMultiplier()}");

                    progressions[j] += segmentLength * businessHolders[j].GetSpeedMultiplier();

                    while (progressions[j] >= businessHolders[j].business.timeToProduce)
                    {
                        int rounds = GetMaxProductionRounds(businessHolders[j], businesses_, currencies_, (progressions[j] / businessHolders[j].business.timeToProduce).ToInt(), data1);

                        //Debug.Log($"    Rounds: {rounds}, Progression: {progressions[j]}");

                        if (rounds == 0)
                        {
                            progressions[j] = 0;
                            break;
                        }

                        progressions[j] -= businessHolders[j].business.timeToProduce * rounds;

                        List<Input> productionCost = businessHolders[j].business.iProducable.GetProductionCost(businessHolders[j].business.scaleProductionCostWithAmount ? businessHolders[j].amount : 1).Multiply(rounds);
                        data1.RemoveInput(productionCost);
                        RemoveInput(productionCost, businesses_, currencies_);

                        List<Output> output = businessHolders[j].iProducable.GetOptimisticActualProduction(
                            0,
                            false,
                            ProductionOutputCalculationMode.OfflineCalculation);

                        if (businessHolders[j].business.ConsumeOnProduce)
                        {
                            BigNumber consumed = businessHolders[j].amount;
                            if (consumed > 0)
                            {
                                if (!data1.businessOutputs.TryAdd(businessHolders[j].business, -consumed))
                                    data1.businessOutputs[businessHolders[j].business] -= consumed;

                                businessHolders[j].amount -= consumed;
                            }
                        }

                        //Debug.Log(" Actual Production: " + output.CustomToString());

                        foreach (var o in output)
                        {
                            o.outputAmount *= rounds;
                            o.outputMaxAmount *= rounds;

                            if (o.currencyOutput != null)
                            {
                                BigNumber avg = o.GetAverageAmount();
                                BigNumber allowed = GetAllowedCurrencyAddition(o.currencyOutput, avg, currencies_);
                                o.outputAmount = allowed;
                                o.outputMaxAmount = allowed;
                            }

                            if (o.businessOutput != null)
                            {
                                BigNumber avg = o.GetAverageAmount();
                                BigNumber allowed = GetAllowedBusinessAddition(o.businessOutput, avg, businesses_);
                                o.outputAmount = allowed;
                                o.outputMaxAmount = allowed;
                            }
                        }

                        //Debug.Log($"    Output: {output.CustomToString()}");

                        data1.AddOutput(output, businessHolders[j].business, IsManagerUnlocked(businessHolders[j], managerIsUnlocked));
                        AddOutput(output, businesses_, currencies_);

                        foreach (var o in output)
                        {
                            if (o.businessOutput != null)
                            {
                                BusinessHolder holder = businesses_[o.businessOutput];

                                if (holder != null)
                                {
                                    //Debug.Log($"    Adding output to business: {holder.business.name}, Amount: {holder.amount}, Output: {o.outputAmount}");
                                    holder.ProductionAmount = holder.amount;
                                }
                            }
                        }

                        businessHolders[j].ProductionAmount = businessHolders[j].amount;
                    }

                    if (!data1.businessProductionTime.TryAdd(businessHolders[j].business, progressions[j])) data1.businessProductionTime[businessHolders[j].business] = progressions[j];
                }

                if (BoostsManager.Instance && simulateBoosts) BoostsManager.Instance.OnTimePassed(segmentLength);

                if (ManagersManager.Instance && simulateActiveManagers) ManagersManager.Instance.OnTimePassed(segmentLength);

                //Debug.Log($"    Data after round {z}: {data1} (round lenght: {segmentLength})");
            }

            data1.updatedBoosts = BoostsManager.Instance ? BoostsManager.Instance.activeBoosts : new();
            if (BoostsManager.Instance) BoostsManager.Instance.activeBoosts = boostsSave;

            foreach (var (holder, activeTimeLeft, cooldownTimeLeft) in managersSave)
            {
                holder.activeTimeLeft = activeTimeLeft;
                holder.cooldownTimeLeft = cooldownTimeLeft;
            }

            return data1;
        }

        private static int GetMaxProductionRounds(BusinessHolder business, Dictionary<Business, BusinessHolder> businesses, Dictionary<Currency, CurrencyHolder> currencies, int maxTimeRounds, ProfitData data1)
        {
            BigNumber maxRounds = maxTimeRounds;

            if (business.business.ConsumeOnProduce)
            {
                maxRounds = BigNumber.Min(maxRounds, 1);
            }

            List<Input> costPerRound = business.business.iProducable.GetProductionCost(business.business.scaleProductionCostWithAmount ? business.amount : 1);
            foreach (var item in costPerRound)
            {
                if (item.currencyInput)
                {
                    CurrencyHolder currency = currencies.TryGetValue(item.currencyInput, out var currencyHolder) ? currencyHolder : null;

                    BigNumber amount = currency == null ? 0 : currency.amount;

                    BigNumber mR = item.inputAmount <= 0 ? maxRounds : amount / item.inputAmount;

                    maxRounds = BigNumber.Min(maxRounds, mR.Floor());
                }

                if (item.businessInput)
                {
                    BusinessHolder businessHolder = businesses.TryGetValue(item.businessInput, out var holder) ? holder : null;

                    BigNumber amount = businessHolder == null ? 0 : businessHolder.amount;

                    BigNumber mR = item.inputAmount <= 0 ? maxRounds : amount / item.inputAmount;

                    maxRounds = BigNumber.Min(maxRounds, mR.Floor());
                }
            }

            return maxRounds.ToInt();
        }

        private static void RemoveInput(List<Input> input, Dictionary<Business, BusinessHolder> businesses, Dictionary<Currency, CurrencyHolder> currencies)
        {
            foreach (var i in input)
            {
                if (i.businessInput != null)
                {
                    BusinessHolder businessHolder = businesses.TryGetValue(i.businessInput, out var holder) ? holder : null;

                    if (businessHolder != null) businessHolder.amount -= i.inputAmount;
                }

                if (i.currencyInput != null)
                {
                    CurrencyHolder currencyHolder = currencies.TryGetValue(i.currencyInput, out var holder) ? holder : null;

                    if (currencyHolder != null) currencyHolder.amount -= i.inputAmount;
                }
            }
        }

        private static void AddOutput(List<Output> output, Dictionary<Business, BusinessHolder> businesses, Dictionary<Currency, CurrencyHolder> currencies)
        {
            foreach (var o in output)
            {
                if (o.businessOutput != null)
                {
                    if (!businesses.TryGetValue(o.businessOutput, out var holder))
                    {
                        holder = new BusinessHolder(o.businessOutput, 0);
                        businesses.Add(o.businessOutput, holder);
                    }
                    holder.amount += o.GetAverageAmount();
                }

                if (o.currencyOutput != null)
                {
                    if (!currencies.TryGetValue(o.currencyOutput, out var holder))
                    {
                        holder = new CurrencyHolder(o.currencyOutput, 0);
                        currencies.Add(o.currencyOutput, holder);
                    }
                    holder.amount += o.GetAverageAmount();
                }
            }
        }

        private static BigNumber GetAllowedCurrencyAddition(Currency currency, BigNumber amountToAdd, Dictionary<Currency, CurrencyHolder> simulatedCurrencies)
        {
            BigNumber allowedAddition = amountToAdd;

            if (currency.currencyGroups != null)
            {
                foreach (var group in currency.currencyGroups)
                {
                    if (group != null && group.capacity >= 0)
                    {
                        BigNumber usedCapacity = 0;
                        foreach (var kvp in simulatedCurrencies)
                        {
                            if (kvp.Key.currencyGroups != null && kvp.Key.currencyGroups.Contains(group))
                            {
                                usedCapacity += kvp.Value.amount;
                            }
                        }

                        BigNumber remaining = group.capacity - usedCapacity;
                        if (remaining < allowedAddition)
                        {
                            allowedAddition = BigNumber.Max(0, remaining);
                        }
                    }
                }
            }

            allowedAddition = allowedAddition.Floor();

            if (!simulatedCurrencies.TryGetValue(currency, out var holder))
            {
                holder = new CurrencyHolder(currency, 0);
                simulatedCurrencies.Add(currency, holder);
            }

            BigNumber max = holder.GetMaxCurrencyConstrain(currency);
            if (holder.amount + allowedAddition > max)
            {
                allowedAddition = BigNumber.Max(0, max - holder.amount);
            }

            return allowedAddition;
        }

        private static BigNumber GetAllowedBusinessAddition(Business business, BigNumber amountToAdd, Dictionary<Business, BusinessHolder> simulatedBusinesses)
        {
            BigNumber allowedAddition = amountToAdd;

            List<BusinessGroup> effectiveGroups = business.GetEffectiveBusinessGroups();
            if (effectiveGroups != null)
            {
                foreach (var group in effectiveGroups)
                {
                    if (group != null && group.capacity >= 0)
                    {
                        BigNumber usedCapacity = 0;
                        foreach (var kvp in simulatedBusinesses)
                        {
                            List<BusinessGroup> simulatedGroups = kvp.Key.GetEffectiveBusinessGroups();
                            if (simulatedGroups != null && simulatedGroups.Contains(group))
                            {
                                usedCapacity += kvp.Value.amount;
                            }
                        }

                        BigNumber remaining = group.capacity - usedCapacity;
                        if (remaining < allowedAddition)
                        {
                            allowedAddition = BigNumber.Max(0, remaining);
                        }
                    }
                }
            }
            allowedAddition = allowedAddition.RoundToWholeNumber();

            if (!simulatedBusinesses.TryGetValue(business, out var holder))
            {
                holder = new BusinessHolder(business, 0);
                simulatedBusinesses.Add(business, holder);
            }

            BigNumber max = holder.GetMaxAmountConstrain();
            if (holder.amount + allowedAddition > max)
            {
                allowedAddition = BigNumber.Max(0, max - holder.amount);
            }

            return allowedAddition;
        }

        public static bool CanAutoProduce(BusinessHolder business, Dictionary<Business, BusinessHolder> businesses, Dictionary<Currency, CurrencyHolder> currencies, Dictionary<Business, bool> managerIsUnlocked)
        {
            bool isSufficent = true;

            List<Input> costPerRound = business.business.iProducable.GetProductionCost(business.business.scaleProductionCostWithAmount ? business.amount : 1);
            foreach (var item in costPerRound)
            {
                if (item.currencyInput)
                {
                    CurrencyHolder currency = currencies.ContainsKey(item.currencyInput) ? currencies[item.currencyInput] : null;

                    BigNumber amount = currency == null ? 0 : currency.amount;

                    isSufficent = isSufficent && amount >= item.inputAmount;
                }

                if (item.businessInput)
                {
                    BusinessHolder businessHolder = businesses.ContainsKey(item.businessInput) ? businesses[item.businessInput] : null;

                    BigNumber amount = businessHolder == null ? 0 : businessHolder.amount;

                    isSufficent = isSufficent && amount >= item.inputAmount;
                }
            }

            //Debug.Log($"    CanAutoProduce: {business.business.name}, {IsManagerUnlockedOrAutoProduce(business, managerIsUnlocked)} && {business.ProductionAmount != 0} && {isSufficent}");

            return (IsManagerUnlockedOrAutoProduce(business, managerIsUnlocked) || BoostsManager.IsAutoProduceOverrideActive(business.business)) && business.ProductionAmount != 0 && isSufficent;
        }

        private static bool IsManagerUnlockedOrAutoProduce(BusinessHolder business, Dictionary<Business, bool> managerIsUnlocked)
        {
            if (business.business.autoProduce) return true;

            return IsManagerUnlocked(business, managerIsUnlocked);
        }

        private static bool IsManagerUnlocked(BusinessHolder business, Dictionary<Business, bool> managerIsUnlocked)
        {
            if (business.business.manager == null) return false;

            bool mUnlocked = false;

            if (!managerIsUnlocked.TryGetValue(business.business, out mUnlocked))
            {
                mUnlocked = business.ManagerUnlocked();
                managerIsUnlocked.Add(business.business, mUnlocked);
            }

            return mUnlocked;
        }

        public static ProfitApplicationSummary ApplyProfit(ProfitData profit)
        {
            ProfitApplicationSummary summary = new ProfitApplicationSummary(profit);

            foreach (KeyValuePair<Currency, BigNumber> kvp in profit.businessProfits)
            {
                BigNumber before = CurrencyManager.Instance.GetCurrencyAmount(kvp.Key);
                CurrencyManager.Instance.AddCurrencyAmount(kvp.Key, kvp.Value);
                BigNumber after = CurrencyManager.Instance.GetCurrencyAmount(kvp.Key);
                summary.AppliedCurrencies[kvp.Key] = after - before;
            }

            foreach (KeyValuePair<Business, BigNumber> kvp in profit.businessOutputs)
            {
                BusinessHolder holder = BusinessesManager.Instance.GetOrAddHolder(kvp.Key);
                BigNumber before = holder.amount;
                BusinessesManager.Instance.ForceBuyItemsForFree(kvp.Key, kvp.Value);
                summary.AppliedBusinesses[kvp.Key] = holder.amount - before;
            }

            foreach (KeyValuePair<Business, BigNumber> kvp in profit.businessProductionTime)
            {
                var holder = BusinessesManager.Instance.GetHolder(kvp.Key);
                if (holder != null && holder.activeProductions.Count > 0)
                {
                    holder.activeProductions[0].productionTime = kvp.Value.ToDouble();
                    summary.AppliedProductionTimes[kvp.Key] = kvp.Value;
                }
            }

            foreach (KeyValuePair<Business, List<Output>> kvp in profit.businessesStockpile)
            {
                BusinessHolder holder = BusinessesManager.Instance.GetHolder(kvp.Key);
                if (holder == null)
                    continue;

                List<Output> stockpile = kvp.Value.Squash();
                holder.unclaimedOutputs.AddRange(stockpile);
                summary.AppliedStockpiles[kvp.Key] = stockpile;
            }

            if (BoostsManager.Instance)
                BoostsManager.Instance.activeBoosts = profit.updatedBoosts;

            OnProfitApplied.Invoke(summary);
            EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(ProfitCalculator));
            return summary;
        }
    }
}
