using System.Collections.Generic;

namespace EasyIdleGame
{
    /// <summary>
    /// Interface for SCRIPTABLE OBJECTS that can be bought
    /// </summary>
    public interface IUnlockable
    {
        public List<Lock> Locks { get; }

        public List<Lock> UpperLocks { get; }

        /// <summary> If true, the item will be unlocked only once - it won't be locked again </summary>
        public bool UnlockOnlyOnce { get; }

        public AudioData UnlockSound { get; }

        /// <summary> If the item is currently unlocked </summary>
        public bool IsCurrentlyUnlocked() => Locks.IsUnlocked() && UpperLocks.IsUpperUnlocked();
    }
}