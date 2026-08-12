using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame
{
    public interface ILevelupableHolderManager<E, V>
    where E : HolderBase<V>, ILevelupableHolder
    where V : ScriptableObject, ILevelupable
    {
        public UnityEvent<E> OnHolderLeveledUpEvent { get; }

        /// <returns> Current level of the holder </returns>
        public int GetCurrentItemLevel(V holder)
        {
            return GetOrAddHolder(holder).Level.currentLevel;
        }

        // required functions
        public E GetOrAddHolder(V v);
    }
}
