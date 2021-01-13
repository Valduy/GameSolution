using System;
using System.Collections.Generic;

namespace ECS.Core
{
    public class Engine
    {
        private readonly List<Entity> _entities = new List<Entity>();
        private readonly SortedList<uint, SystemBase> _systems = new SortedList<uint, SystemBase>();
        private readonly Dictionary<Type, IFamily> _nodes = new Dictionary<Type, IFamily>();

        public event Action Updated;

        public void Update(double time) 
        {
            foreach (var system in _systems.Values)
            {
                system.Update(time);
            }
            Updated?.Invoke();
        }

        public void AddEntity(Entity entity)
        {
            entity.ComponentAdded += OnComponentAdded;
            entity.ComponentRemoved += OnComponentRemoved;
            _entities.Add(entity);

            foreach (var family in _nodes.Values)
            {
                family.AddEntity(entity);
            }
        }

        public void RemoveEntity(Entity entity)
        {
            entity.ComponentAdded -= OnComponentAdded;
            entity.ComponentRemoved -= OnComponentRemoved;
            _entities.Remove(entity);

            foreach (var family in _nodes.Values)
            {
                family.RemoveEntity(entity);
            }
        }

        public IEnumerable<Entity> GetEntities() => _entities;

        public void RemoveAllEntities() 
        {
            foreach (var entity in _entities)
            {
                RemoveEntity(entity);
            }
        }

        public IEnumerable<TNode> GetNodes<TNode>() where TNode : NodeBase, new()
        { 
            if (_nodes.TryGetValue(typeof(TNode), out var family)) 
            {
                return ((Family<TNode>)family);
            }

            var result = new Family<TNode>();            
            _nodes[typeof(TNode)] = result;

            foreach (var entity in _entities)
            {
                result.AddEntity(entity);
            }

            return result;
        }

        public void DeleteNodes<TNode>() 
        {
            _nodes.Remove(typeof(TNode));
        }

        public void AddSystem(SystemBase system, uint priority)
        {
            try
            {
                system.Priority = priority;
                system.Register(this);
                _systems.Add(priority, system);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException($"Система с приоритетом {priority} уже существует.");
            }
        }

        public void RemoveSystem(SystemBase systemBase)
        {
            _systems.Remove(systemBase.Priority);
            systemBase.Unregister(this);
        }

        public IEnumerable<SystemBase> GetSystems() => _systems.Values;

        public void RemoveAllSystems() => _systems.Clear();

        private void OnComponentAdded(Entity entity, ComponentBase componentBase) 
        {
            foreach (var family in _nodes.Values) 
            {
                family.OnComponentAddedToEntity(entity, componentBase);
            }
        }

        private void OnComponentRemoved(Entity entity, ComponentBase componentBase)
        {
            foreach (var family in _nodes.Values)
            {
                family.OnComponentRemovedFromEntity(entity, componentBase);
            }
        }        
    }
}
