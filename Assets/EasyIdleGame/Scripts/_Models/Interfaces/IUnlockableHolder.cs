using System;

namespace EasyIdleGame
{
    /// <summary>
    /// Interface for HOLDER OBJECTS that can be bought, holder scriptable objects have to implement IUnlockable
    /// </summary>
    public interface IUnlockableHolder
    {
        public Action<IUnlockableHolder> OnUnlocked { get; set; }

        public IUnlockable UnlockableItem { get; }

        /// <summary> If the item, was already unlocked before </summary>
        public bool AlreadyUnlocked { get; set; }

        /// <returns> If Item is unlocked </returns>
        public bool IsUnlocked()
        {
            if (UnlockableItem.UnlockOnlyOnce && AlreadyUnlocked) return UnlockableItem.UpperLocks.IsUpperUnlocked();

            if (UnlockableItem.IsCurrentlyUnlocked())
            {
                AlreadyUnlocked = true;
                OnUnlocked?.Invoke(this);
                return true;
            }

            return false;
        }
    }
}