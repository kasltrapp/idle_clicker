using System.Collections.Generic;

namespace EasyIdleGame
{
    public static class HolderListExtensionMethods
    {
        public static Dictionary<Business, BusinessHolder> ToDictionary(this List<BusinessHolder> list)
        {
            Dictionary<Business, BusinessHolder> dictionary = new();
            foreach (var item in list)
            {
                dictionary.Add(item.Item, item);
            }
            return dictionary;
        }

        public static Dictionary<Currency, CurrencyHolder> CloneToDictionary(this List<CurrencyHolder> list)
        {
            Dictionary<Currency, CurrencyHolder> dictionary = new();
            foreach (var item in list)
            {
                dictionary.Add(item.Item, new CurrencyHolder(item.Item, item.amount));
            }
            return dictionary;
        }
    }
}