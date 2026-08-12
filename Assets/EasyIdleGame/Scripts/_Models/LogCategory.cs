namespace EasyIdleGame
{
    /// <summary>
    /// Log categories for manager diagnostic logging.
    /// Higher values are more severe; messages at or above the configured level are logged.
    /// </summary>
    public enum LogCategory
    {
        /// <summary>
        /// Logs all messages including routine success operations.
        /// Use for debugging and development.
        /// </summary>
        Verbose = 0,

        /// <summary>
        /// Logs failed user-invoked actions (e.g., TryBuyItems returning false).
        /// These are expected behaviors, not errors.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Logs only negative path events: missing assignments,
        /// configuration errors, and issues requiring attention.
        /// Default setting for clean console output.
        /// </summary>
        Critical = 2
    }
}
