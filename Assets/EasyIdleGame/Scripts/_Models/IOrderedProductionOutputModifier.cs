namespace EasyIdleGame
{
    public interface IOrderedProductionOutputModifier : IProductionOutputModifier
    {
        int Priority { get; }
    }
}
