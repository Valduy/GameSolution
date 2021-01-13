using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ECS.Core
{
    /// <summary>
    /// Класс, описывающий семейство узлов, определяющее принадлежность семье по компонентам.
    /// </summary>
    /// <typeparam name="TNode">Наследники интерфейса <see cref="NodeBase"/>.</typeparam>
    public class Family<TNode> : IFamily, IEnumerable<TNode> where TNode : NodeBase, new()
    {
        private static readonly Type ComponentBaseType = typeof(ComponentBase);
        private static readonly Type[] ComponentsTypes;
        private readonly Dictionary<Entity, TNode> _nodes = new Dictionary<Entity, TNode>();

        static Family()
        {
            var properties = typeof(TNode).GetTypeInfo().DeclaredProperties
                .Where(o => o.PropertyType.GetTypeInfo().IsSubclassOf(ComponentBaseType));

            ComponentsTypes = properties.Select(property => property.PropertyType).ToArray();
        }

        internal Family() 
        { 
        
        }

        public Type GetNodeType() => typeof(TNode);

        public void AddEntity(Entity entity)
        {
            if (IsMatch(entity)) 
            {
                RegisterEntity(entity);
            }
        }

        public void RemoveEntity(Entity entity) => _nodes.Remove(entity);

        public void OnComponentAddedToEntity(Entity entity, ComponentBase componentBase) 
        { 
            if (_nodes.ContainsKey(entity))
            {
                return;
            }
            
            if (IsMatch(entity)) 
            {
                RegisterEntity(entity);
            }
        }

        public void OnComponentRemovedFromEntity(Entity entity, ComponentBase componentBase)
        {
            if (!_nodes.ContainsKey(entity))
            {
                return;
            }

            if (ComponentsTypes.Contains(componentBase.GetType()))
            {
                _nodes.Remove(entity);
            }
        }

        public void Clear() => _nodes.Clear();

        public IEnumerator<TNode> GetEnumerator() => _nodes.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private bool IsMatch(Entity entity) 
            => ComponentsTypes.All(type => entity.Get(type) != null);

        private void RegisterEntity(Entity entity) 
            => _nodes[entity] = new TNode { Entity = entity };
    }
}
