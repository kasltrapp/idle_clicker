using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace EasyIdleGame.UI
{
    [CustomEditor(typeof(BaseDisplayer), true)]
    public class DisplayerBaseCustomEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BaseDisplayer displayer = (BaseDisplayer)target;

            if (GUILayout.Button("Redraw")) RedrawDefault(displayer);

            GUILayout.Space(8);

            DrawDefaultInspector();
        }

        private void RedrawDefault(BaseDisplayer displayer)
        {
            displayer.Redraw_Default();
        }
    }
}
#endif