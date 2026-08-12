using UnityEngine;

namespace EasyIdleGame
{
    [CreateAssetMenu(fileName = "newCurrencyGroup", menuName = "EasyIdleGame/Currencies/Currency Group")]
    public class CurrencyGroup : ItemGroup<Currency>
    {
        [CommentArea("Currency Group", "Groups multiple currencies under a single combined limit. -1 capacity means infinite.", "Use this for resources that share a wallet cap, such as multiple elemental shards that together cannot exceed 500. Assign this group to each Currency that should share the limit.")]
        [SerializeField] private string _currencyGroupComment;

        [Tooltip("Shared capacity across all currencies in this group. -1 means infinite; every assigned currency contributes to the same used capacity.")]
        public BigNumber capacity = -1;
    }
}
