using System.Collections.Generic;

namespace EasyIdleGame
{
    public class ProfitApplicationSummary
    {
        public readonly ProfitData SourceProfit;
        public readonly Dictionary<Currency, BigNumber> AppliedCurrencies = new Dictionary<Currency, BigNumber>();
        public readonly Dictionary<Business, BigNumber> AppliedBusinesses = new Dictionary<Business, BigNumber>();
        public readonly Dictionary<Business, BigNumber> AppliedProductionTimes = new Dictionary<Business, BigNumber>();
        public readonly Dictionary<Business, List<Output>> AppliedStockpiles = new Dictionary<Business, List<Output>>();

        public ProfitApplicationSummary(ProfitData sourceProfit)
        {
            SourceProfit = sourceProfit;
        }
    }
}
