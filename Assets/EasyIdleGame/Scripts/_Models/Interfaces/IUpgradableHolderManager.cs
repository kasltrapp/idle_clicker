using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame
{
    public interface IUpgradableHolderManager<E, V>
    where E : HolderBase<V>, IUpgradableHolder
    where V : ScriptableObject, IUpgradable
    {
        public UnityEvent<E> OnHolderUpgradedEvent { get; }

        /// <returns> Whether item is at least at lvl 1 - pretty much if item is unlocked upgrade vise </returns>
        public bool IsItemAtLevelOneOrMore(V item)
        {
            return GetOrAddHolder(item).Level >= 1;
        }

        // required functions
        public E GetOrAddHolder(V manager);
    }
}
