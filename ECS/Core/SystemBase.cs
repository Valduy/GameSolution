namespace ECS.Core
{
    public abstract class SystemBase
    {
        public uint Priority { get; internal set; }
        public abstract void Update(double time);
        public abstract void Register(Engine engine);
        public abstract void Unregister(Engine engine);
    }
}
