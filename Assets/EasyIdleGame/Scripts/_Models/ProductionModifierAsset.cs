using UnityEngine;

namespace EasyIdleGame
{
    public abstract class ProductionModifierAsset : ScriptableObject
    {
        [CommentArea("Production Modifier", "Base asset for production-scoped modifiers. Add concrete modifier assets to a ProductionModifierManager so they are registered while that manager is enabled.", "Use output modifiers here for production-specific output changes without changing shared DropTables or generic reward systems.")]
        [SerializeField] private string _productionModifierComment;

        [Tooltip("Lower priorities run earlier when multiple production modifiers are registered. Equal priorities keep registration order.")]
        public int priority;

        public virtual int Priority => priority;
    }
}