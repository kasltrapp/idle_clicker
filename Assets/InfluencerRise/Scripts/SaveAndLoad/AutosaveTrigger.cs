using EasyIdleGame;
using UnityEngine;

namespace InfluencerRise
{
    /// <summary>
    /// Deliberate save triggers, replacing reliance on SaveAndLoad's bare autoSaveInterval timing
    /// as the primary save mechanism (autoSaveInterval stays configured as a backup safety net).
    /// Per 04-persistence-offline-prestige.md ("Call Save() at deliberate lifecycle points") and
    /// 05-scripting-reference.md's "Coalesce autosaves with EconomyChangedEvents".
    ///
    /// Subscribes once to EconomyChangedEvents.OnSaveWorthyStateChanged rather than wiring a raw
    /// Save() call into every Business/Manager/Upgrade/Boost/ShopItem purchase path - every one of
    /// those paths already raises this event internally (confirmed by reading each holder's source,
    /// not assumed). The event itself debounces same-frame notifications to one call; the
    /// minSecondsBetweenSaves timer below adds a second coalescing layer on top so a rapid string
    /// of purchases spread across several frames still produces at most one disk write per window.
    ///
    /// IsLoadInProgress guard: SaveAndLoad.Load() applies offline profit through ProfitCalculator,
    /// which also raises OnSaveWorthyStateChanged - without this guard, every load would queue an
    /// immediate, redundant re-save of the state that was just restored.
    ///
    /// HasCompletedInitialLoad guard: confirmed necessary via testing, not theoretical.
    /// OnApplicationPause(true) can fire before GameBootstrap.Start() runs at all (observed via an
    /// unfocused/automation-driven Editor window, which Unity treats as backgrounded immediately on
    /// entering Play Mode). Without this guard, that pause-triggered save writes a near-empty file
    /// before the starting-currency grant happens, and the empty file then makes GameBootstrap's
    /// next Load() see file != null, silently skipping the grant forever.
    /// </summary>
    [AddComponentMenu("InfluencerRise/Autosave Trigger")]
    public sealed class AutosaveTrigger : MonoBehaviour
    {
        [Tooltip("Minimum real seconds between deliberate saves triggered by economy changes.")]
        [SerializeField] private float minSecondsBetweenSaves = 1f;

        private bool _pendingSave;
        private float _lastSaveRealtime = float.NegativeInfinity;

        /// <summary>Test/diagnostic observability only - mirrors SaveLoadDiagnostics' precedent for exposing otherwise-private state to automated verification.</summary>
        public bool HasPendingSave => _pendingSave;

        /// <summary>
        /// Runs the exact same coalescing check Update() runs, on demand. For automated testing
        /// where Unity's own frame loop cannot be relied on to tick (MCP-driven Play Mode sessions
        /// don't reliably advance Update() between separate tool calls); real gameplay always goes
        /// through Update() itself, this just gives tests a way to force the same code path.
        /// </summary>
        public void CheckAndFlushIfDue() => Update();

        private void OnEnable()
        {
            EconomyChangedEvents.OnSaveWorthyStateChanged.AddListener(HandleSaveWorthyStateChanged);
        }

        private void OnDisable()
        {
            EconomyChangedEvents.OnSaveWorthyStateChanged.RemoveListener(HandleSaveWorthyStateChanged);
        }

        private void HandleSaveWorthyStateChanged(string source)
        {
            if (GameBootstrap.IsLoadInProgress) return;
            _pendingSave = true;
        }

        private void Update()
        {
            if (!_pendingSave) return;
            if (Time.realtimeSinceStartup - _lastSaveRealtime < minSecondsBetweenSaves) return;

            FlushImmediately();
        }

        /// <summary>Standard Unity lifecycle callback - mobile OS backgrounding the app.</summary>
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) FlushImmediately();
        }

        /// <summary>Standard Unity lifecycle callback - app process terminating.</summary>
        private void OnApplicationQuit()
        {
            FlushImmediately();
        }

        private void FlushImmediately()
        {
            if (!GameBootstrap.HasCompletedInitialLoad) return;
            if (SaveAndLoad.Instance == null) return;

            SaveAndLoad.Instance.Save();
            _pendingSave = false;
            _lastSaveRealtime = Time.realtimeSinceStartup;
        }
    }
}
