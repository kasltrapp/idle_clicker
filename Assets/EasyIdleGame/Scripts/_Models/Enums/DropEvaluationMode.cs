using UnityEngine;

namespace EasyIdleGame
{
    /// <summary>
    /// Determines how drop chances are evaluated when producing or granting multiple instances at once.
    /// </summary>
    public enum DropEvaluationMode
    {
        /// <summary>
        /// The drop chance is evaluated a single time for the entire batch.
        /// If successful, the total amount produced is multiplied by the number of instances.
        /// Useful for 'all or nothing' drops that scale with quantity.
        /// </summary>
        [InspectorName("Evaluate Once (Multiplied)")]
        EvaluateOnce,

        /// <summary>
        /// The drop chance is evaluated individually for each instance produced.
        /// For large amounts, this automatically falls back to an Expected Value calculation to maintain performance.
        /// Useful for consistent, normalized drops when producing large quantities.
        /// </summary>
        [InspectorName("Evaluate Per Instance (Normalized)")]
        EvaluatePerInstance
    }
}
