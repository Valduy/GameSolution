namespace ECS.Core
{
    public abstract class ComponentBase
    {
        public Entity Owner { get; internal set; }
    }
}
