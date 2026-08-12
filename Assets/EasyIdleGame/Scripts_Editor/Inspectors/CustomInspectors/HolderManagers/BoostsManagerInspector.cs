
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EasyIdleGame.Editor
{
    [CustomEditor(typeof(BoostsManager), true)]
    public class BoostsManagerInspector : CustomInspectorBase
    {
        public override void DrawDebugTop()
        {
            _MultiplierManagersHelper.DrawMultiplierDebug((IMultiplierProviderHolderManager)target);
        }

        public override void DrawDebugBottom()
        {
            BoostsManager manager = (BoostsManager)target;
            GUI.enabled = Application.isPlaying;

            if (GUILayout.Button("ResetHolders"))
            {
                manager.holders.Clear();
            }
        }
    }
}

#endif