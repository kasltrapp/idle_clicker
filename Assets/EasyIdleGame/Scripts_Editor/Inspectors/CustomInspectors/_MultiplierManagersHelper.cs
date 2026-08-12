
#if UNITY_EDITOR

using UnityEngine;

namespace EasyIdleGame.Editor
{
    public static class _MultiplierManagersHelper
    {
        public static void DrawMultiplierDebug(IMultiplierProviderHolderManager manager)
        {
            GUI.color = new Color(1, 1, 1, .5f);
            GUILayout.Label("Production Multiplier: " + (manager).GetMultiplierOfType(UpgradeType.production, ""));
            GUILayout.Label("Speed Multiplier: " + (manager).GetMultiplierOfType(UpgradeType.speed, ""));
            GUILayout.Label("Purchase Cost Multiplier: " + (manager).GetMultiplierOfType(UpgradeType.purchaseCost, ""));

            GUILayout.Space(16);
            GUI.color = new Color(1, 1, 1, 1);
        }
    }
}

#endif