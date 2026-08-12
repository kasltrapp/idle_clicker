using System;

namespace EasyIdleGame
{
    [Flags]
    public enum MultiplierSource
    {
        None = 0,
        Prestige = 1 << 0,
        Boosts = 1 << 1,
        Upgrades = 1 << 2,
        Managers = 1 << 3
    }
}
