namespace EasyIdleGame
{
    /// <summary>
    /// Interface for managers that support diagnostic logging with configurable verbosity.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Current log level threshold. Messages at or above this level are logged.
        /// </summary>
        LogCategory LogLevel { get; }

        void Log(string message, LogCategory category = LogCategory.Verbose);
    }
}
