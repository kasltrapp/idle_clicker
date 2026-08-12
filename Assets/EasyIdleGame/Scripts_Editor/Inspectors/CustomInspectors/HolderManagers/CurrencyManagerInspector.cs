
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace EasyIdleGame.Editor
{
    [CustomEditor(typeof(CurrencyManager))]
    public class CurrencyManagerInspector : CustomInspectorBase
    {
        private Currency selectedCurrency;
        private double matissaToAdd;
        private long exponentToAdd;

        public override void DrawDebugBottom()
        {
            CurrencyManager manager = (CurrencyManager)target;

            GUI.enabled = Application.isPlaying;

            GUILayout.Space(10);
            GUILayout.Label("--- Add Money --- (Debug)", EditorStyles.boldLabel);

            selectedCurrency = (Currency)EditorGUILayout.ObjectField("Select Currency", selectedCurrency, typeof(Currency), false);

            GUILayout.BeginHorizontal();

            GUILayout.Label("MToAdd: ");
            matissaToAdd = EditorGUILayout.DoubleField(matissaToAdd);
            if (matissaToAdd < 0) matissaToAdd = 0;

            exponentToAdd = EditorGUILayout.LongField(exponentToAdd);
            if (exponentToAdd < 0) exponentToAdd = 0;

            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (GUILayout.Button("Add Money"))
            {
                if (selectedCurrency != null) manager.AddCurrencyAmount(selectedCurrency, new BigNumber(matissaToAdd, exponentToAdd));
                else Debug.LogWarning("Please assign a currency before adding money.");
            }

            GUI.enabled = Application.isPlaying;

            if (GUILayout.Button("ResetHolders"))
            {
                manager.holders.Clear();
            }
        }
    }
}


#endif