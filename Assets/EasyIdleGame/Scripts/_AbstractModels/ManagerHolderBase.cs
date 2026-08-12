using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EasyIdleGame
{
    public abstract class ManagerHolderBase<T, E, V> : ManagerBase<T>
    where T : ManagerHolderBase<T, E, V>
    where E : HolderBase<V>
    where V : ScriptableObject
    {
        private void Awake()
        {
            holders.Clear();
        }

        [Tooltip("Runtime holder state for items managed by this component. Usually populated automatically and restored from save data; edit only for debugging or migration.")]
        public List<E> holders = new List<E>();

        public IEnumerable<E> Holders => holders;

        /// <returns> a holder of the item </returns>
        public E GetHolder(V item) => holders.Find(h => h.Item == item);

        public bool TryGetHolder(V item, out E holder)
        {
            holder = GetHolder(item);
            return holder != null;
        }

        public UnityEngine.Events.UnityEvent<E> OnHolderAdded { get; } = new UnityEngine.Events.UnityEvent<E>();

        /// <summary>
        /// Adds a new holder to the list if it doesn't exist yet
        /// </summary>
        /// <returns> a holder of the item </returns>
        public abstract E GetOrAddHolder(V item);

        // EXPERIMENTAL
        public virtual void ResetManagerHolders(List<V> excludedItems)
        {
            excludedItems ??= new List<V>();

            List<E> removedHolders = holders
                .Where(holder => holder == null || holder.Item == null || !excludedItems.Contains(holder.Item))
                .ToList();

            holders.RemoveAll(holder => removedHolders.Contains(holder));

            foreach (E holder in removedHolders)
            {
                NotifyHolderReset(holder);
            }
        }

        protected virtual void NotifyHolderReset(E holder) { }

        public virtual void ResetUnlockedState()
        {
            foreach (E holder in holders)
            {
                if (holder is IUnlockableHolder unlockableHolder)
                    unlockableHolder.AlreadyUnlocked = false;
            }
        }

        public static void TryResetManagerHolders(List<V> excludedItems)
        {
            if (!Instance) return;
            Instance.ResetManagerHolders(excludedItems);
        }

        public static void TryResetUnlockedState()
        {
            if (!Instance) return;
            Instance.ResetUnlockedState();
        }

        public V[] GetAllScriptableObjects() => GetAllScriptableObjects_Static();

        public static V[] GetAllScriptableObjects_Static()
        {
            return Resources.LoadAll<V>("");
        }
    }
}
