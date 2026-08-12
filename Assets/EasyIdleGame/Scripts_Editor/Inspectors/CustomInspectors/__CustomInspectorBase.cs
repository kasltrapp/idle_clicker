#if UNITY_EDITOR

namespace EasyIdleGame.Editor
{
    public abstract class CustomInspectorBase : UnityEditor.Editor
    {
        public virtual void DrawDebugTop() { }
        public virtual void DrawDebugBottom() { }

        public virtual void DrawDefault() => DrawDefaultInspector();

        public override void OnInspectorGUI()
        {
            if (EditorMenu.InspectorMode == InspectorDebugMode.DebugOnly)
            {
                UnityEditor.EditorGUILayout.HelpBox("Debug only mode is enabled. Only debug fields will be shown. You can change this under Tools > EasyIdleGame> Inspector Mode", UnityEditor.MessageType.Info);

                DrawDebugTop();
                DrawDebugBottom();
                return;
            }

            if (EditorMenu.InspectorMode == InspectorDebugMode.NoDebug)
            {
                DrawDefault();
                return;
            }

            DrawDebugTop();
            DrawDefault();
            DrawDebugBottom();
        }
    }
}

#endif