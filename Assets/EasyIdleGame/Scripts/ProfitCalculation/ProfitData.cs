using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that handles profit data and can do some basic operations on it
    /// </summary>
    public class ProfitData
    {
        public TimeSpan elapsedTime;

        public Dictionary<Currency, BigNumber> businessProfits;

        // xps and business xps can be calculated using this data
        public Dictionary<Business, BigNumber> businessOutputs;

        public Dictionary<Business, BigNumber> businessProductionTime;

        public Dictionary<Business, List<Output>> businessesStockpile;

        public List<ActiveBoostHolder> updatedBoosts;

        public Dictionary<Currency, BigNumber> absoluteBusinessProfits;
        public Dictionary<Business, BigNumber> absoluteBusinessOutputs;

        public ProfitData(TimeSpan time)
        {
            elapsedTime = time;
            businessProfits = new Dictionary<Currency, BigNumber>();
            businessOutputs = new Dictionary<Business, BigNumber>();
            businessProductionTime = new Dictionary<Business, BigNumber>();
            updatedBoosts = new List<ActiveBoostHolder>();
            businessesStockpile = new Dictionary<Business, List<Output>>();

            absoluteBusinessProfits = new Dictionary<Currency, BigNumber>();
            absoluteBusinessOutputs = new Dictionary<Business, BigNumber>();
        }



        public void RemoveInput(List<Input> input)
        {
            foreach (var i in input)
            {
                if (i.businessInput != null)
                {
                    if (!businessOutputs.TryAdd(i.businessInput, -i.inputAmount)) businessOutputs[i.businessInput] -= i.inputAmount;
                }
                if (i.currencyInput != null)
                {
                    if (!businessProfits.TryAdd(i.currencyInput, -i.inputAmount)) businessProfits[i.currencyInput] -= i.inputAmount;
                }
            }
        }

        public void AddOutput(List<Output> output, Business businessProvider, bool isManagerUnlocked)
        {
            foreach (var o in output)
            {
                if (o.businessOutput != null)
                {
                    if (!absoluteBusinessOutputs.TryAdd(o.businessOutput, o.GetAverageAmount())) absoluteBusinessOutputs[o.businessOutput] += o.GetAverageAmount();
                }

                if (o.currencyOutput != null)
                {
                    if (!absoluteBusinessProfits.TryAdd(o.currencyOutput, o.GetAverageAmount())) absoluteBusinessProfits[o.currencyOutput] += o.GetAverageAmount();
                }
            }

            if (!businessProvider.autoCollect && !isManagerUnlocked)
            {
                businessesStockpile.TryAdd(businessProvider, new List<Output>());
                businessesStockpile[businessProvider].AddRange(output);
                return;
            }

            foreach (var o in output)
            {
                if (o.businessOutput != null)
                {
                    if (!businessOutputs.TryAdd(o.businessOutput, o.GetAverageAmount())) businessOutputs[o.businessOutput] += o.GetAverageAmount();
                }

                if (o.currencyOutput != null)
                {
                    if (!businessProfits.TryAdd(o.currencyOutput, o.GetAverageAmount())) businessProfits[o.currencyOutput] += o.GetAverageAmount();
                }
            }
        }

        public ProfitData MultiplyProfits(float multiplier)
        {
            Dictionary<Currency, BigNumber> newBusinessProfits = new();
            Dictionary<Business, BigNumber> newBusinessOutputs = new();

            foreach (var pair in businessProfits)
                newBusinessProfits.Add(pair.Key, pair.Value * multiplier);

            foreach (var pair in businessOutputs)
                newBusinessOutputs.Add(pair.Key, pair.Value * multiplier);

            return new(elapsedTime)
            {
                businessProfits = newBusinessProfits,
                businessOutputs = newBusinessOutputs,

                businessProductionTime = businessProductionTime,
                updatedBoosts = updatedBoosts
            };
        }

        public BigNumber GetCurrencyAmount(Currency currency)
        {
            return businessProfits.TryGetValue(currency, out BigNumber amount) ? amount : 0;
        }

        public string ElapsedTime()
        {
            if (elapsedTime.Days > 0)
                return $"{elapsedTime.Days}D {elapsedTime.Hours}H";
            else if (elapsedTime.Hours > 0)
                return $"{elapsedTime.Hours}H {elapsedTime.Minutes}M";
            else
                return $"{elapsedTime.Minutes}M {elapsedTime.Seconds}S";
        }

        public BigNumber GetAdditionalBusinessAmount(Business business)
        {
            return businessOutputs.TryGetValue(business, out BigNumber amount) ? amount : new BigNumber(0);
        }

        public BigNumber GetAbsoluteCurrencyAmount(Currency currency)
        {
            return absoluteBusinessProfits.TryGetValue(currency, out BigNumber amount) ? amount : new BigNumber(0);
        }

        public BigNumber GetAbsoluteBusinessAmount(Business business)
        {
            return absoluteBusinessOutputs.TryGetValue(business, out BigNumber amount) ? amount : new BigNumber(0);
        }

        public List<Output> GetBusinessStockpile(Business business)
        {
            if (businessesStockpile.TryGetValue(business, out List<Output> stockpile))
                return stockpile.Squash();
            return new List<Output>();
        }

        public override string ToString()
        {
            string profits = string.Join("\n", businessProfits.Select(p => $"{p.Key.GetDisplayName()}: {p.Value}"));

            string businesses = string.Join("\n", businessOutputs.Select(p => $"{p.Key.GetDisplayName()}: {p.Value}"));

            string businessProductionTimes = string.Join("\n", businessProductionTime.Select(p => $"{p.Key.GetDisplayName()}: {p.Value}"));

            return "" +
                $"Time: {elapsedTime}\n" +
                $"Profits:\n{profits}\n\n" +
                $"Businesses:\n{businesses}" +
                $"Businesses updated times:\n{businessProductionTimes}";
        }
    }
}
