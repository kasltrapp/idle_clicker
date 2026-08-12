#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EasyIdleGame.Editor
{
    [CustomEditor(typeof(PlayerStats))]
    public class PlayerStatsInspector : CustomInspectorBase
    {
        public override void DrawDebugBottom()
        {
            PlayerStats manager = (PlayerStats)target;

            GUI.enabled = Application.isPlaying;

            GUILayout.Space(16);
            GUILayout.Label("Debug", EditorStyles.boldLabel);
            if (GUILayout.Button("Add Xps"))
            {
                manager.AddXp(manager.level.TargetXp / 4);
            }
        }
    }
}

#endif