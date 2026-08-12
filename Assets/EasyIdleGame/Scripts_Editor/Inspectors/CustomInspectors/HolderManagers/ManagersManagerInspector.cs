
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EasyIdleGame.Editor
{
    [CustomEditor(typeof(ManagersManager))]
    public class ManagersManagerInspector : CustomInspectorBase
    {
        public override void DrawDebugBottom()
        {
            ManagersManager manager = (ManagersManager)target;

            if (GUILayout.Button("ResetHolders"))
            {
                manager.holders.Clear();
            }
        }
    }
}

#endif