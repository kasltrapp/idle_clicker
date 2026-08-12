using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame
{
    public static class EconomyChangedEvents
    {
        public static readonly UnityEvent<string> OnSaveWorthyStateChanged = new UnityEvent<string>();

        private static readonly HashSet<string> saveWorthySourcesThisFrame = new HashSet<string>();
        private static int lastSaveWorthyDispatchFrame = int.MinValue;
        private static int suppressedSaveWorthyChangesThisFrame;

        public static int SuppressedSaveWorthyChangesThisFrame
        {
            get
            {
                return Application.isPlaying && Time.frameCount == lastSaveWorthyDispatchFrame
                    ? suppressedSaveWorthyChangesThisFrame
                    : 0;
            }
        }

        public static string SaveWorthySourcesThisFrame
        {
            get
            {
                if (!Application.isPlaying || Time.frameCount != lastSaveWorthyDispatchFrame || saveWorthySourcesThisFrame.Count == 0)
                {
                    return "none";
                }

                return string.Join(", ", saveWorthySourcesThisFrame.OrderBy(source => source));
            }
        }

        public static void ResetSaveWorthyDebounce()
        {
            saveWorthySourcesThisFrame.Clear();
            lastSaveWorthyDispatchFrame = int.MinValue;
            suppressedSaveWorthyChangesThisFrame = 0;
        }

        public static void NotifySaveWorthyStateChanged(string source)
        {
            source = string.IsNullOrWhiteSpace(source) ? "Unknown" : source;

            if (Application.isPlaying)
            {
                int frame = Time.frameCount;
                if (frame == lastSaveWorthyDispatchFrame)
                {
                    saveWorthySourcesThisFrame.Add(source);
                    suppressedSaveWorthyChangesThisFrame++;
                    return;
                }

                lastSaveWorthyDispatchFrame = frame;
                suppressedSaveWorthyChangesThisFrame = 0;
                saveWorthySourcesThisFrame.Clear();
                saveWorthySourcesThisFrame.Add(source);
            }

            OnSaveWorthyStateChanged.Invoke(source);
        }
    }
}
