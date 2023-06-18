namespace SupplyDrop.CoreModules
{
    /// <summary>
    /// Basic holder class to allow easy initialization of core modules for Supply Drop
    /// </summary>
    public abstract class CoreModule
    {
        public abstract string Name { get; }
        public abstract void Init();
    }
}