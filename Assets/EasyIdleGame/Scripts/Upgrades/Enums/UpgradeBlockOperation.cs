using UnityEngine;

namespace EasyIdleGame
{
    /// <summary>
    /// How a contextual UpgradeBlock value changes the matched economy value.
    /// </summary>
    public enum UpgradeBlockOperation
    {
        [Tooltip("Multiply the matched value by this block's value.")]
        Multiply = 0,

        [Tooltip("Add this block's value to the matched multiplier or chance.")]
        Add = 1
    }
}
