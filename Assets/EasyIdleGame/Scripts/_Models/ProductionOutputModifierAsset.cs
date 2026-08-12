using System.Collections.Generic;

namespace EasyIdleGame
{
    public abstract class ProductionOutputModifierAsset : ProductionModifierAsset, IOrderedProductionOutputModifier
    {
        public abstract bool AppliesTo(ProductionOutputContext context);
        public abstract void ModifyOutputs(ProductionOutputContext context, List<Output> outputs);
    }
}