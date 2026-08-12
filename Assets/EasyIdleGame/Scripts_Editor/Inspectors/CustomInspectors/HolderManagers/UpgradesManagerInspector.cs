
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EasyIdleGame.Editor
{
    [CustomEditor(typeof(UpgradesManager), true)]
    public class UpgradesManagerInspector : CustomInspectorBase
    {
        public override void DrawDebugTop()
        {
            _MultiplierManagersHelper.DrawMultiplierDebug((IMultiplierProviderHolderManager)target);
        }

        public override void DrawDebugBottom()
        {
            UpgradesManager manager = (UpgradesManager)target;
            GUI.enabled = Application.isPlaying;

            if (GUILayout.Button("ResetHolders"))
            {
                manager.holders.Clear();
            }
        }
    }
}

#endif