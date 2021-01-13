namespace ECS.Core
{
    public abstract class NodeBase
    {
        private Entity _entity;

        public Entity Entity
        {
            get => _entity;
            set
            {
                _entity = value;
                OnEntityChanged();
            }
        }

        protected abstract void OnEntityChanged();
    }
}
