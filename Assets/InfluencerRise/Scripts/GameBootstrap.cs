using EasyIdleGame;
using UnityEngine;

namespace InfluencerRise
{
    /// <summary>
    /// Grants a starting Followers balance so the very first purchase (SelfieSession, 1 Follower)
    /// is affordable on a fresh launch. Adapted from the package's own documented bootstrap
    /// pattern (IdleGameBootstrap in 01-create-your-first-idle-game.md, "Load or seed the
    /// starting balance").
    ///
    /// TEMPORARY SIMPLIFICATION: the documented pattern calls SaveAndLoad.Instance.Load(out file)
    /// first and only grants starting currency when no save file exists yet. This project has no
    /// SaveAndLoad component in the scene yet, so that check is skipped entirely here - this
    /// grants the starting balance unconditionally every time the scene starts. Revisit this
    /// (add the same file-exists check back) once SaveAndLoad is added, or every relaunch will
    /// re-grant Followers on top of saved progress.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Tooltip("Currency granted on scene start. Assign the Followers asset.")]
        [SerializeField] private Currency startingCurrency;

        [Tooltip("Amount of startingCurrency granted on scene start.")]
        [SerializeField] private BigNumber startingAmount = 10;

        private void Start()
        {
            if (startingCurrency == null || CurrencyManager.Instance == null) return;

            CurrencyManager.Instance.AddCurrencyAmount(startingCurrency, startingAmount, SourceType.Generic);
        }
    }
}
