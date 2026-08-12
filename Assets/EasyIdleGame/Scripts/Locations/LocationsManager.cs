using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame
{
    [AddComponentMenu("EasyIdleGame/Managers/Locations Manager")]
    public class LocationsManager : ManagerHolderBase<LocationsManager, LocationHolder, Location>,
        IBuyableHolderManager<LocationsManager, LocationHolder, Location>,
        IUnlockableHolderManager<LocationHolder, Location>
    {
        public IBuyableHolderManager<LocationsManager, LocationHolder, Location> iBuyable => this;
        public IUnlockableHolderManager<LocationHolder, Location> iUnlockable => this;

        public UnityEvent<Location> OnItemBought { get; } = new UnityEvent<Location>();
        public UnityEvent<Location> OnLocationChanged = new UnityEvent<Location>();
        public UnityEvent<Location> OnLocationAmountChanged = new UnityEvent<Location>();
        public UnityEvent<Location> OnItemUnlocked { get; } = new UnityEvent<Location>();
        public UnityEvent<LocationHolder> OnHolderBoughtEvent { get; } = new UnityEvent<LocationHolder>();
        public UnityEvent<LocationHolder> OnHolderUnlockedEvent { get; } = new UnityEvent<LocationHolder>();

        [CommentArea("Locations Manager", "Tracks unlocked locations, active travel destination, starter rewards, and location change events.", "Place one LocationsManager in the gameplay scene, set defaultActiveLocation, then call TryTravelTo(location) from a map button. Location locks can also require the player to already be in another location.")]
        [SerializeField] private string _locationsManagerComment;

        [Tooltip("Location activated by default when the game starts and no active location has been restored. Leave null if gameplay should start without an active location.")]
        public Location defaultActiveLocation;

        private Location _activeLocation;
        public Location ActiveLocation
        {
            get => _activeLocation;
            private set
            {
                if (_activeLocation != value)
                {
                    _activeLocation = value;
                    OnLocationChanged.Invoke(_activeLocation);
                    EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(LocationsManager));
                }
            }
        }

        private float _timeSinceLastCheck = 0f;
        private void Update()
        {
            _timeSinceLastCheck += Time.deltaTime;
            if (_timeSinceLastCheck >= 0.2f)
            {
                _timeSinceLastCheck = 0f;
                iUnlockable.CheckAutoUnlocks();
            }
        }

        private void Start()
        {
            if (_activeLocation == null && defaultActiveLocation != null)
            {
                TryTravelTo(defaultActiveLocation);
            }
        }

        private void OnDestroy()
        {
        }

        public void RestoreStarterOutputs()
        {
            foreach (var holder in holders)
            {
                if (holder.iUnlockable.IsUnlocked())
                {
                    holder.Item.starterOutputTables.ApplyOutputs(1);
                    Log($"Restored {holder.Item.starterOutputTables.Count} starter output table(s) for {holder.Item.GetDisplayName()}.", LogCategory.Verbose);
                }
            }

            if (_activeLocation != null)
            {
                LocationHolder h = GetHolder(_activeLocation);
                // If the holder was cleared during prestige, or if it is no longer unlocked
                if (h == null || !h.iUnlockable.IsUnlocked())
                {
                    _activeLocation = null;
                }
            }

            if (_activeLocation == null && defaultActiveLocation != null)
            {
                TryTravelTo(defaultActiveLocation);
            }
        }

        public bool TryUnlockLocation(Location location)
        {
            if (location == null) return false;

            LocationHolder holder = GetOrAddHolder(location);

            if (holder.iBuyable.Maxed) return false;

            if (!holder.iBuyable.CanBuy(1))
            {
                Log($"Cannot travel to {location.GetDisplayName()} - insufficient funds or locked.", LogCategory.Warning);
                return false;
            }

            holder.iBuyable.Pay_BuyEvent(1, SourceType.Purchase);
            holder.AddAmount(1, SourceType.Purchase);
            holder.AlreadyUnlocked = true;

            location.starterOutputTables.ApplyOutputs(1);
            Log($"Applied {location.starterOutputTables.Count} starter output table(s) for {location.GetDisplayName()}.", LogCategory.Verbose);

            OnItemUnlocked?.Invoke(location);
            EconomyChangedEvents.NotifySaveWorthyStateChanged(nameof(LocationsManager));
            return true;
        }


        public bool TryTravelTo(Location location)
        {
            if (location == null) return false;

            LocationHolder holder = GetOrAddHolder(location);

            if (!holder.iBuyable.Maxed && !TryUnlockLocation(location)) return false;

            ActiveLocation = location;
            Log($"Traveled to location: {location.GetDisplayName()}", LogCategory.Verbose);
            return true;
        }

        public override LocationHolder GetOrAddHolder(Location item)
        {
            if (item == null) throw new Exception("Location is null");

            LocationHolder h = GetHolder(item);
            if (h == null)
            {
                holders.Add(h = new LocationHolder(item));
                OnHolderAdded?.Invoke(h);
            }
            return h;
        }

        protected override void NotifyHolderReset(LocationHolder holder)
        {
            if (holder?.location == null) return;
            if (holder.Amount <= 0 && holder.BoughtAmount <= 0) return;

            OnLocationAmountChanged?.Invoke(holder.location);
        }

        public void ForceBuyItemsForFree(Location item, BigNumber amount, SourceType sourceType = SourceType.Generic)
        {
            throw new NotSupportedException("This method is not supported on Locations manager");
        }

        public List<T> GetAllLocations<T>() where T : Location
        {
            return Resources.LoadAll<T>("").OrderBy(location => location.sortOrder).ToList();
        }

        public List<T> GetUnlockedLocations<T>() where T : Location
        {
            return GetAllLocations<T>().Where(location => GetOrAddHolder(location).iUnlockable.IsUnlocked()).ToList();
        }

        public bool TryGetActiveLocation<T>(out T location) where T : Location
        {
            location = ActiveLocation as T;
            return location != null;
        }
    }
}
