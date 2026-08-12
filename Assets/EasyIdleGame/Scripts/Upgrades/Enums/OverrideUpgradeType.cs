
namespace EasyIdleGame
{
    /// <summary>
    /// For this upgrade type, the highest value will override any previous values
    /// </summary>
    public enum OverrideUpgradeType
    {
        maxCurrencyAmount = 0,
        // by default, this is in hours, but can be changed when the idle data is calculated
        idleProductionTimeCap = 1,
        /// <summary> Raises the maximum number of a specific business the player can own. Scope to a business via OverrideUpgradeBlock.targetBusiness. </summary>
        maxBusinessAmount = 2,
        // !! DO NOT CHANGE THE ORDER OF THESE VALUES AS THEY ARE BEING SAVED AS INTEGER IN THE UNITY EDITOR
        // for the custom upgrade types, start from 1000
        // eg, customOverrideUpgradeType1 = 1000,
    }
}
