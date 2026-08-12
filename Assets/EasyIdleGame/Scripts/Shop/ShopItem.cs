using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame
{
    [CreateAssetMenu(fileName = "New Shop Item", menuName = "EasyIdleGame/Shop/Shop Item", order = 1)]
    public class ShopItem : ScriptableObject, IBuyable, IUnlockable, IMetadataProvider
    {
        [CommentArea("Shop Item", "Represents an item in the store. It can be purchased with real money, an ad, in-game currency, or granted by code.", "For a starter pack, set productId for IAP, add rewardTables for Coins and Boosts, then subscribe to ShopManager.OnAttemptRealMoneyPurchase to open your platform purchase flow.")]
        [SerializeField] private string _shopItemComment;

        public IBuyable iBuyable => this;
        public IUnlockable iUnlockable => this;

        [Header("General")]
        [Tooltip("Display metadata shown by shop UI. Fill this when the shop item needs its own name, icon, model, or description.")]
        public Metadata metadata = new Metadata();
        public Metadata Metadata => metadata ??= new Metadata();

        [Tooltip("Typed groups this shop item belongs to. Used for shared locks and targeting; leave empty if the shop item has no specific group.")]
        public List<ShopItemGroup> shopItemGroups = new List<ShopItemGroup>();

        [Header("Monetization")]
        [Tooltip("Product ID used for real-money in-app purchases (IAP). Leave empty for non-IAP items or items purchased only by ad/currency/code.")]
        public string productId;

        [Tooltip("If enabled, this item can be purchased through the ad purchase event. Disable for items that require IAP, in-game cost, or code-only grants.")]
        public bool buyWithAd;

        [SerializeField, HideInInspector] private BigNumber _multipleCostsFlag;
        [SerializeField, HideInInspector] private BigNumber _zeroFlag;

        [ConditionalCommentArea("Shop item setup", "WARNING: This shop item is misconfigured! It has multiple payment methods configured. The system will prioritize them in this order: Ad > Real Money > In-Game Cost. Please configure only one primary cost type per item.", "_multipleCostsFlag", ConditionalCommentAreaMode.BigNumberGreaterThan, "_zeroFlag")]
        [SerializeField] private string _multipleCostsInfo;

        [Header("In-Game Cost")]
        [Tooltip("In-game inputs paid to consume or purchase this item. Leave empty if the item is free, IAP-only, ad-only, or granted externally.")]
        public List<Input> inGameCost;

        [Header("Rewards")]
        [ConditionalCommentArea("Shop item setup", "No rewards are configured. This shop item can still be purchased, but it will not grant anything.", "rewardTables", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _rewardTablesInfo;
        [Tooltip("Drop tables granted after a successful purchase or external completion. Empty rewards are allowed but the shop item will grant nothing.")]
        public List<DropTable> rewardTables = new List<DropTable>();

        [Header("Limits & Locks")]
        [Tooltip("Maximum times this shop item can be purchased or completed. 0 means infinite.")]
        public int maxPurchases = 0;

        [ConditionalCommentArea("Shop item setup", "One or more locks have amount 0 or less. Those locks will be satisfied immediately.", "locks", ConditionalCommentAreaMode.AnyListBigNumberLessThanOrEqual, "inputAmount", 0)]
        [SerializeField] private string _locksInfo;
        [Tooltip("Requirements that must be met before this shop item is shown or purchasable. Empty list means it is available immediately.")]
        public List<Lock> locks;
        [ConditionalCommentArea("Shop item setup", "One or more upper locks have amount 0 or less. Those upper locks may hide this item immediately.", "upperLocks", ConditionalCommentAreaMode.AnyListBigNumberLessThanOrEqual, "inputAmount", 0)]
        [SerializeField] private string _upperLocksInfo;
        [Tooltip("Upper-bound requirements that hide this shop item once exceeded. Use for limited early-game or progression-window offers.")]
        public List<Lock> upperLocks;
        [Tooltip("If enabled, this item stays unlocked after its locks are met once, even if the player later spends the required resources.")]
        public bool unlockOnlyOnce = true;

        [Header("Audio")]
        [Tooltip("Sound played when this item is bought.")]
        public AudioData buySound;
        [Tooltip("Sound played when this item is unlocked.")]
        public AudioData unlockSound;

        // IBuyable
        public List<Input> Cost => inGameCost;
        public BigNumber CostMultiplier => 1;
        public BigNumber MaxBuyableAmount => maxPurchases <= 0 ? BigNumber.MaxValue : maxPurchases;
        public bool UseGeometricBoughtAmount => false;
        public AudioData BuySound => buySound;

        // IUnlockable
        public List<Lock> Locks => locks;
        public List<Lock> UpperLocks => upperLocks;
        public bool UnlockOnlyOnce => unlockOnlyOnce;
        public AudioData UnlockSound => unlockSound;

        bool IBuyable.CanPay(BigNumber amountToBuy, BigNumber currentAmount)
        {
            if (inGameCost == null || inGameCost.Count == 0) return true;
            return inGameCost.IsSufficent(amountToBuy, currentAmount, CostMultiplier, 1);
        }

        void IBuyable.Pay(BigNumber amountToBuy, BigNumber currentAmount)
        {
            if (inGameCost != null && inGameCost.Count > 0)
            {
                inGameCost.RemoveInputs(amountToBuy, currentAmount, CostMultiplier, 1);
            }
        }

        public BigNumber GetMaxBuyableAmount(BigNumber currentAmount)
        {
            if (maxPurchases <= 0) return BigNumber.MaxValue;
            BigNumber remaining = maxPurchases - currentAmount;
            return remaining > 0 ? remaining : 0;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _zeroFlag = 0;
            int costCount = 0;
            if (buyWithAd) costCount++;
            if (!string.IsNullOrEmpty(productId)) costCount++;
            if (inGameCost != null && inGameCost.Count > 0) costCount++;

            _multipleCostsFlag = costCount > 1 ? 1 : 0;
        }
#endif
    }
}
