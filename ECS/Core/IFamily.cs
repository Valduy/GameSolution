using System;
using System.Collections;

namespace ECS.Core
{
    internal interface IFamily : IEnumerable
    {
        Type GetNodeType();
        void AddEntity(Entity entity);
        void RemoveEntity(Entity entity);
        void OnComponentAddedToEntity(Entity entity, ComponentBase componentBase);
        void OnComponentRemovedFromEntity(Entity entity, ComponentBase componentBase);
        void Clear();
    }
}
