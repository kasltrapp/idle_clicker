using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame
{
    [Serializable]
    public class OfferPoolItem
    {
        [Tooltip("Shop item that can become this temporary offer. Leave null only while authoring; null entries cannot produce a valid offer.")]
        public ShopItem item;

        [Tooltip("Group of shop items. If assigned, all items in this group will act as possible offers with this weight and duration.")]
        public ShopItemGroup group;

        [Min(1)]
        [Tooltip("Relative selection weight among currently valid offers. Higher values make this offer more likely; locked or maxed items are skipped.")]
        public int weight = 1;

        [Tooltip("Seconds this offer stays active before expiring and scheduling the next offer.")]
        public float durationSeconds = 30f;
    }

    [AddComponentMenu("EasyIdleGame/Managers/Offers Manager")]
    public class OffersManager : ManagerBase<OffersManager>
    {
        public OffersManager() { _necessary = false; }

        [Header("Offers Configuration")]
        [CommentArea("Offers List", "List of potential offers using ShopItems. The manager waits a random interval, filters out locked or maxed offers, then picks one based on weights.", "Create a discounted Boost ShopItem, add it to possibleOffers with weight 5 and duration 60, then listen to OnOfferBecameAvailable to show your limited-time offer popup.")]
        [SerializeField] private string _offersComment;

        [ConditionalCommentArea("Offers setup", "No possible offers are configured. This manager will keep scheduling offers, but none can become available.", "possibleOffers", ConditionalCommentAreaMode.EmptyList)]
        [SerializeField] private string _possibleOffersInfo;
        [Tooltip("Pool of shop items that can be shown as temporary offers. Empty list means no offer can become active.")]
        public List<OfferPoolItem> possibleOffers = new List<OfferPoolItem>();

        [Tooltip("Minimum seconds to wait before attempting to show the next offer.")]
        public float minSecondsBetweenOffers = 60f;

        [Tooltip("Maximum seconds to wait before attempting to show the next offer. Keep this >= minSecondsBetweenOffers for predictable scheduling.")]
        public float maxSecondsBetweenOffers = 120f;

        [Header("Events")]
        [Tooltip("Invoked when a valid offer becomes active and should be shown in UI.")]
        public UnityEvent<OfferPoolItem> OnOfferBecameAvailable = new UnityEvent<OfferPoolItem>();

        [Tooltip("Invoked when the active offer times out or is explicitly expired.")]
        public UnityEvent<OfferPoolItem> OnOfferExpired = new UnityEvent<OfferPoolItem>();

        public OfferPoolItem ActiveOffer { get; private set; }
        public float OfferTimerRemainingSeconds => (float)Math.Max(0, _offerTimer);
        public bool IsOfferActive => _isOfferActive;

        private double _offerTimer;
        private bool _isOfferActive;

        private void Start()
        {
            ScheduleNextOffer();
        }

        private void Update() => OnTimePassed(Time.deltaTime);

        public void OnTimePassed(float deltaTime)
        {
            if (_isOfferActive)
            {
                _offerTimer -= deltaTime;
                if (_offerTimer <= 0)
                {
                    Log("Active offer expired.", LogCategory.Verbose);
                    ExpireCurrentOffer();
                }
            }
            else
            {
                _offerTimer -= deltaTime;
                if (_offerTimer <= 0)
                {
                    ShowRandomOffer();
                }
            }
        }

        public void ScheduleNextOffer()
        {
            _offerTimer = UnityEngine.Random.Range(minSecondsBetweenOffers, maxSecondsBetweenOffers);
            _isOfferActive = false;
            Log($"Next offer scheduled in {_offerTimer} seconds.", LogCategory.Verbose);
        }

        private void ShowRandomOffer()
        {
            if (possibleOffers.Count == 0)
            {
                Log("No possible offers configured.", LogCategory.Critical);
                ScheduleNextOffer();
                return;
            }

            if (ShopManager.Instance == null)
            {
                Log("ShopManager instance not found. Cannot validate ShopItems.", LogCategory.Critical);
                ScheduleNextOffer();
                return;
            }

            List<OfferPoolItem> resolvedOffers = new List<OfferPoolItem>();
            foreach (var offer in possibleOffers)
            {
                if (offer.item != null)
                {
                    resolvedOffers.Add(offer);
                }

                if (offer.group != null)
                {
                    foreach (var holder in ShopManager.Instance.Holders)
                    {
                        if (holder.Item.shopItemGroups != null && holder.Item.shopItemGroups.Contains(offer.group))
                        {
                            resolvedOffers.Add(new OfferPoolItem { item = holder.Item, weight = offer.weight, durationSeconds = offer.durationSeconds });
                        }
                    }
                }
            }

            int totalWeight = 0;
            foreach (var offer in resolvedOffers)
            {
                ShopItemHolder holder = ShopManager.Instance.GetOrAddHolder(offer.item);
                if (holder.iBuyable.Maxed) continue;
                if (!holder.iUnlockable.IsUnlocked()) continue;

                totalWeight += offer.weight;
            }

            if (totalWeight == 0)
            {
                Log("No valid offers available currently (all maxed or locked).", LogCategory.Verbose);
                ScheduleNextOffer();
                return;
            }

            int randomWeight = UnityEngine.Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var offer in resolvedOffers)
            {
                ShopItemHolder holder = ShopManager.Instance.GetOrAddHolder(offer.item);
                if (holder.iBuyable.Maxed) continue;
                if (!holder.iUnlockable.IsUnlocked()) continue;

                currentWeight += offer.weight;
                if (randomWeight < currentWeight)
                {
                    ActiveOffer = offer;
                    _offerTimer = offer.durationSeconds;
                    _isOfferActive = true;
                    Log($"Offer became available: {offer.item.GetDisplayName()} for {offer.durationSeconds}s.", LogCategory.Verbose);
                    OnOfferBecameAvailable.Invoke(ActiveOffer);
                    return;
                }
            }
        }

        public void ExpireCurrentOffer()
        {
            if (!_isOfferActive) return;
            var expired = ActiveOffer;
            ActiveOffer = null;
            ScheduleNextOffer();
            OnOfferExpired.Invoke(expired);
        }

        [ContextMenu("Trigger Offer")]
        public void TriggerOffer()
        {
            if (_isOfferActive) ExpireCurrentOffer();
            ShowRandomOffer();
        }

        /// <summary>
        /// Call this when the player purchases or dismisses the offer early
        /// </summary>
        public bool TryConsumeCurrentOffer()
        {
            if (!_isOfferActive) return false;
            ActiveOffer = null;
            ScheduleNextOffer();
            return true;
        }
    }
}
