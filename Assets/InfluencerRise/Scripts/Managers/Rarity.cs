namespace InfluencerRise.Managers
{
    /// <summary>
    /// Manager rarity tiers for the future weighted "Capsule" pull system
    /// (see docs/EconomySchema.md's Managers section). Confirmed via code audit that the
    /// package has no native rarity/tier concept - see docs/EngineCapabilities.md.
    /// </summary>
    public enum Rarity
    {
        Common,
        Rare,
        Epic,
        Mythic
    }
}
