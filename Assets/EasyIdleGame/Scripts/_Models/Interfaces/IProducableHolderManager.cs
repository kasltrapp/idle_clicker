using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace EasyIdleGame
{
    public interface IProducableHolderManager<E, V>
    where E : HolderBase<V>, IProducableHolder
    where V : ScriptableObject, IProducable
    {
        public UnityEvent<E> OnHolderProductionStartedEvent { get; }
        public UnityEvent<E> OnHolderProductionFinishedEvent { get; }

        /// <returns> All items that have 'currencyOutput' or 'businessOuput' or both in output </returns>
        public V[] GetAllItemsByOutput(Currency currencyOutput = null, Business businessOutput = null)
        {
            return GetAllScriptableObjects().Where(b =>
            {
                return b.OutputTables.SelectMany(t => t.outputs).Any(o =>
                    (businessOutput == null || o.businessOutput == businessOutput) &&
                    (currencyOutput == null || o.currencyOutput == currencyOutput)
                );
            }).ToArray();
        }

        /// <returns> All items that have 'currencyInput' or 'businessInput' or both in input </returns>
        public V[] GetAllItemsByInputCost(Currency currencyInput = null, Business businessInput = null)
        {
            return GetAllScriptableObjects().Where(b =>
            {
                return b.ProductionCost.Any(c =>
                    (!currencyInput || c.currencyInput == currencyInput) &&
                    (!businessInput || c.businessInput == businessInput)
                );
            }).ToArray();
        }

        // required functions
        public V[] GetAllScriptableObjects();
    }
}