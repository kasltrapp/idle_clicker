using System;
using UnityEngine;

namespace EasyIdleGame
{
    /// <summary>
    /// Runtime context mask for an UpgradeBlock. Any disables runtime filtering.
    /// </summary>
    [Flags]
    public enum UpgradeRuntimeFilter
    {
        [Tooltip("Apply in every runtime context.")]
        Any = 0,

        [Tooltip("Apply while the game is active, including manual actions. Can be combined with Offline or Manual.")]
        Active = 1,

        [Tooltip("Apply during offline production calculations. Can be combined with Active or Manual.")]
        Offline = 2,

        [Tooltip("Apply to manual actions such as tap, hold, or manual collection flows. Can be combined with Active or Offline.")]
        Manual = 4
    }
}
