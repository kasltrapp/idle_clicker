using UnityEngine;

namespace EasyIdleGame
{
    public interface IUnlockableHolderManager<E, V>
    where E : HolderBase<V>, IUnlockableHolder
    where V : ScriptableObject, IUnlockable
    {
        /// <returns> Whether item is unlocked </returns>
        public bool IsItemUnlocked(V item)
        {
            return GetOrAddHolder(item).IsUnlocked();
        }

        public UnityEngine.Events.UnityEvent<V> OnItemUnlocked { get; }
        public UnityEngine.Events.UnityEvent<E> OnHolderUnlockedEvent { get; }

        public System.Collections.Generic.IEnumerable<E> Holders { get; }

        public void CheckAutoUnlocks()
        {
            foreach (E holder in Holders)
            {
                if (!holder.AlreadyUnlocked && holder.UnlockableItem.IsCurrentlyUnlocked())
                {
                    holder.AlreadyUnlocked = true;
                    holder.OnUnlocked?.Invoke(holder);
                    OnItemUnlocked?.Invoke(holder.UnlockableItem as V);
                    OnHolderUnlockedEvent?.Invoke(holder);
                }
            }
        }

        // required functions
        public E GetOrAddHolder(V manager);
    }
}