using System.Collections.Generic;

namespace EasyIdleGame
{
    public interface IProductionOutputModifier
    {
        bool AppliesTo(ProductionOutputContext context);
        void ModifyOutputs(ProductionOutputContext context, List<Output> outputs);
    }
}
