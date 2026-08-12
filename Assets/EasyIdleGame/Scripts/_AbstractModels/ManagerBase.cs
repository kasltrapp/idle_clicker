using System;
using UnityEngine;

namespace EasyIdleGame
{
    public abstract class ManagerBase<T> : MonoBehaviour, ILogger
        where T : ManagerBase<T>
    {
        /// <summary>
        /// If manager is necessary, it will throw an exception if not found
        /// Override this in constructor if you want to make the manager necessary
        /// </summary>
        protected static bool _necessary = false;

        private static T _m;

        [Header("Logging")]
        [CommentArea("Diagnostic Logging", "Controls the verbosity of this manager's logs. 'Critical' logs only failed states or missing data, whereas 'Verbose' logs everyday successful operations as well.")]
        [SerializeField] private string _loggingComment;
        [Tooltip("Log level threshold. Critical logs only errors; Verbose logs all operations.")]
        [SerializeField] private LogCategory _logLevel = LogCategory.Critical;

        public LogCategory LogLevel => _logLevel;

        public void Log(string message, LogCategory category = LogCategory.Verbose)
        {
            if (category < _logLevel) return;

            string prefix = $"[{typeof(T).Name}]";
            if (category == LogCategory.Critical)
                Debug.LogWarning($"{prefix} {message}");
            else
                Debug.Log($"{prefix} {message}");
        }

        public static T Instance
        {
            get
            {
                if (_m == null)
                {
                    _m = FindObjectOfType<T>();

                    if (_necessary && _m == null) throw new Exception($"{typeof(T).Name} not found - {typeof(T).Name} is a necessary component");
                }

                return _m;
            }
        }
    }
}
