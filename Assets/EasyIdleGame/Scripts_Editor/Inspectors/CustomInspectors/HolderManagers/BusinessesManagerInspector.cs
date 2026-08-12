
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EasyIdleGame.Editor
{
    [CustomEditor(typeof(BusinessesManager))]
    public class BusinessesManagerInspector : CustomInspectorBase
    {
        public override void DrawDebugBottom()
        {
            BusinessesManager manager = (BusinessesManager)target;

            if (GUILayout.Button("ResetHolders"))
            {
                manager.holders.Clear();
            }
        }
    }
}

#endif