namespace InfluencerRise.Managers
{
    /// <summary>
    /// Manager rarity tiers for the future weighted "Capsule" pull system
    /// (see docs/EconomySchema.md's Managers section). Confirmed via code audit that the
    /// package has no native rarity/tier concept - see docs/EngineCapabilities.md.
    ///
    /// Locked design language is 3 tiers: Common/Rare/Legendary. Legendary is pinned to
    /// its original integer value (3, previously named Mythic) so the already-serialized
    /// ManagerRarity assets built against that value keep deserializing correctly after
    /// this rename - do not renumber without re-verifying every existing ManagerRarity asset.
    /// </summary>
    public enum Rarity
    {
        Common,
        Rare,
        Legendary = 3
    }
}
