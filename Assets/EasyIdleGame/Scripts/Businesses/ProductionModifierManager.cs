using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame
{
    public class ProductionModifierManager : MonoBehaviour
    {
        [CommentArea("Production Modifiers", "Registers production-scoped modifier assets while this manager is enabled. Modifiers are routed by the production interfaces they implement; currently output modifiers are supported.", "Add a Production Business Output Scaling Modifier asset here to change one specific business output without changing DropTables, rewards, shops, achievements, or other non-production systems.")]
        [SerializeField] private string _productionModifierManagerComment;

        [Tooltip("Production modifier assets registered while this manager is enabled. The registry routes each asset to supported production stages, such as output modification.")]
        [SerializeField]
        private List<ProductionModifierAsset> modifiers = new List<ProductionModifierAsset>();

        public List<ProductionModifierAsset> Modifiers => modifiers;

        private void OnEnable()
        {
            RegisterModifiers();
        }

        private void OnDisable()
        {
            UnregisterModifiers();
        }

        private void RegisterModifiers()
        {
            if (modifiers == null) return;

            foreach (ProductionModifierAsset modifier in modifiers)
            {
                if (modifier != null)
                    ProductionModifierRegistry.Register(modifier);
            }
        }

        private void UnregisterModifiers()
        {
            if (modifiers == null) return;

            foreach (ProductionModifierAsset modifier in modifiers)
            {
                if (modifier != null)
                    ProductionModifierRegistry.Unregister(modifier);
            }
        }
    }
}