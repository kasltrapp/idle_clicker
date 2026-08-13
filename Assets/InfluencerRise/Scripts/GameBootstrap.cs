using EasyIdleGame;
using UnityEngine;

namespace InfluencerRise
{
    /// <summary>
    /// Loads any existing save, then grants a starting Followers balance only on a fresh save
    /// (so the first purchase, SelfieSession at 1 Follower, is affordable) - matches
    /// IdleGameBootstrap in 01-create-your-first-idle-game.md, "Load or seed the starting
    /// balance". This is the only place SaveAndLoad.Load() is called for the launch, per that
    /// doc's "Do not also call Load() elsewhere for the same launch."
    ///
    /// [DefaultExecutionOrder(-1000)] is required, not decorative: LocationsManager.Start()
    /// calls TryTravelTo(defaultActiveLocation) whenever its _activeLocation field is null, which
    /// is true for every fresh component instance regardless of whether a save exists on disk.
    /// GameBootstrap sits after LocationsManager in GameManagers' component list, and no script
    /// in EasyIdleGame sets an explicit execution order, so without this attribute Unity's
    /// same-GameObject Start() ordering (undocumented, not guaranteed) could run
    /// LocationsManager.Start() before this Load() call - reapplying location starter state
    /// against un-loaded, blank in-memory holders every single session. The very negative order
    /// value guarantees this component's Start() (and therefore the Load() call within it) runs
    /// before every default-order (0) script's Start(), including LocationsManager's.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Tooltip("Currency granted on a fresh save (no existing file). Assign the Followers asset.")]
        [SerializeField] private Currency startingCurrency;

        [Tooltip("Amount of startingCurrency granted on a fresh save.")]
        [SerializeField] private BigNumber startingAmount = 10;

        /// <summary>
        /// True while this component's Load() call is in progress. AutosaveTrigger checks this
        /// to implement 05-scripting-reference.md's "suppress the handler while loading so partial
        /// state does not trigger recursive saves" - Load() applies offline profit through
        /// ProfitCalculator, which itself raises EconomyChangedEvents.OnSaveWorthyStateChanged.
        /// </summary>
        public static bool IsLoadInProgress { get; private set; }

        /// <summary>
        /// True once this component's full Start() sequence (load, then starting-grant check) has
        /// finished. AutosaveTrigger refuses to save until this is true - confirmed necessary, not
        /// theoretical: OnApplicationPause(true) can fire before Start() runs (Unity treats an
        /// unfocused/automation-driven Editor window as backgrounded immediately on entering Play
        /// Mode). Without this gate, that pause-triggered save writes a near-empty file before the
        /// starting grant happens, and the empty file then makes the grant look already-applied on
        /// the very next Load(), permanently losing the player's starting currency.
        /// </summary>
        public static bool HasCompletedInitialLoad { get; private set; }

        private void Start()
        {
            if (SaveAndLoad.Instance == null)
            {
                Debug.LogWarning("GameBootstrap: no SaveAndLoad in scene - skipping load, starting currency not granted.");
                HasCompletedInitialLoad = true;
                return;
            }

            IsLoadInProgress = true;
            SaveAndLoad.Instance.Load(out SaveFile file);
            IsLoadInProgress = false;

            if (file == null && startingCurrency != null && CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddCurrencyAmount(startingCurrency, startingAmount, SourceType.Generic);
            }

            HasCompletedInitialLoad = true;
        }
    }
}
