using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EasyIdleGame
{
    public static class ProductionModifierRegistry
    {
        private sealed class Registration
        {
            public ProductionModifierAsset Modifier;
            public int Count;
            public int Order;
        }

        private static readonly List<Registration> registrations = new List<Registration>();
        private static int nextOrder;

        public static IReadOnlyList<ProductionModifierAsset> RegisteredModifiers =>
            registrations.Select(r => r.Modifier).ToList();

        public static void Register(ProductionModifierAsset modifier)
        {
            if (IsMissing(modifier)) return;

            Registration existing = registrations.Find(r => ReferenceEquals(r.Modifier, modifier));
            if (existing != null)
            {
                existing.Count++;
                return;
            }

            registrations.Add(new Registration
            {
                Modifier = modifier,
                Count = 1,
                Order = nextOrder++
            });
        }

        public static void Unregister(ProductionModifierAsset modifier)
        {
            if (IsMissing(modifier)) return;

            Registration existing = registrations.Find(r => ReferenceEquals(r.Modifier, modifier));
            if (existing == null) return;

            existing.Count--;
            if (existing.Count <= 0)
                registrations.Remove(existing);
        }

        public static void Clear()
        {
            registrations.Clear();
            nextOrder = 0;
        }

        public static void ApplyOutputModifiers(ProductionOutputContext context, List<Output> outputs)
        {
            if (context == null || outputs == null || registrations.Count == 0) return;

            List<Registration> orderedRegistrations = registrations
                .Where(r => !IsMissing(r.Modifier) && r.Modifier is IProductionOutputModifier)
                .OrderBy(r => r.Modifier.Priority)
                .ThenBy(r => r.Order)
                .ToList();

            foreach (Registration registration in orderedRegistrations)
            {
                IProductionOutputModifier modifier = registration.Modifier as IProductionOutputModifier;
                if (modifier != null && modifier.AppliesTo(context))
                    modifier.ModifyOutputs(context, outputs);
            }
        }

        private static bool IsMissing(ProductionModifierAsset modifier)
        {
            if (modifier == null) return true;
            return modifier is Object unityObject && unityObject == null;
        }
    }
}