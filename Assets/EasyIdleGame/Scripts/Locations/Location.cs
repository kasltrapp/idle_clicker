using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame
{
    [CreateAssetMenu(fileName = "New Location", menuName = "EasyIdleGame/Locations/Location", order = 1)]
    public class Location : ScriptableObject, IBuyable, IUnlockable, IMetadataProvider
    {
        [CommentArea("Location", "Represents a distinct game world or map. Locations can be unlocked via locks or purchased with currencies.", "Create a Desert location, add a Coin cost and a player-level lock, then assign it as defaultActiveLocation on LocationsManager or call TryTravelTo from your world-select UI.")]
        [SerializeField] private string _locationComment;

        public IBuyable iBuyable => this;
        public IUnlockable iUnlockable => this;

        [Header("General")]
        [Tooltip("Display metadata for location UI. Fill this when maps or travel screens need a name, icon, model, or description.")]
        public Metadata metadata = new Metadata();
        public Metadata Metadata => metadata ??= new Metadata();

        [Tooltip("Typed groups this location belongs to. Used for prestige exclusions; leave empty if the location has no specific group.")]
        public List<LocationGroup> locationGroups = new List<LocationGroup>();

        [Header("Locks")]
        [ConditionalCommentArea("Location setup", "One or more locks have amount 0 or less. Those locks will be satisfied immediately.", "locks", ConditionalCommentAreaMode.AnyListBigNumberLessThanOrEqual, "inputAmount", 0)]
        [SerializeField] private string _locksInfo;
        [Tooltip("Requirements that must be met before this location can be unlocked. Empty list means only cost, if any, gates travel.")]
        public List<Lock> locks;

        [Tooltip("If enabled, this location stays unlocked after its locks are met once, even if the player later spends the required resources.")]
        public bool unlockOnlyOnce = true;
        [Tooltip("Sort order used by location lists. Lower values appear first when using LocationsManager.GetAllLocations.")]
        public int sortOrder;

        [Header("Buying")]
        [ConditionalCommentArea("Location setup", "No cost is defined. This location can be unlocked for free once its locks are satisfied.", "cost", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _costInfo;
        [Tooltip("Inputs paid to permanently unlock this location after locks are satisfied. Leave empty for free travel unlocks.")]
        public List<Input> cost;

        [Tooltip("Geometric multiplier applied to the unlock cost each time it is bought. Locations are normally bought once, so this usually stays 1.")]
        public float costMultiplier = 1;

        [Header("Starter Rewards")]
        [ConditionalCommentArea("Location setup", "No starter rewards are configured. Unlocking this location will not grant any initial outputs.", "starterOutputTables", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _starterOutputTablesInfo;
        [Tooltip("Drop tables granted once when this location is first unlocked. Leave empty if traveling here should not grant starter rewards.")]
        public List<DropTable> starterOutputTables = new List<DropTable>();

        [Header("Audio")]
        [Tooltip("Background music played when this location is active.")]
        public AudioData backgroundMusic;

        [Tooltip("Sound played when this location is unlocked via locks.")]
        public AudioData unlockSound;

        [Tooltip("Sound played when this location is bought.")]
        public AudioData buySound;

        // IBuyable
        public List<Input> Cost => cost;
        public BigNumber CostMultiplier => costMultiplier;
        public BigNumber MaxBuyableAmount => 1;
        public bool UseGeometricBoughtAmount => false;
        public AudioData BuySound => buySound;

        // IUnlockable
        public List<Lock> Locks => locks;
        public List<Lock> UpperLocks => null;
        public bool UnlockOnlyOnce => unlockOnlyOnce;
        public AudioData UnlockSound => unlockSound;

        bool IBuyable.CanPay(BigNumber amountToBuy, BigNumber currentAmount)
        {
            return cost.IsSufficent(amountToBuy, currentAmount, CostMultiplier, 1);
        }

        void IBuyable.Pay(BigNumber amountToBuy, BigNumber currentAmount)
        {
            cost.RemoveInputs(amountToBuy, currentAmount, CostMultiplier, 1);
        }

        public BigNumber GetMaxBuyableAmount(BigNumber currentAmount)
        {
            if (currentAmount >= 1) return 0;
            return 1;
        }
    }
}
