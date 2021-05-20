using System;
using System.Collections.Generic;
using ECS.Core;
using ECS.Serialization.Converters;

namespace ECS.Serialization.Contexts
{
    public interface IEcsContext
    {
        IReadOnlyDictionary<int, Type> IdToType { get; }
        IReadOnlyDictionary<Type, int> TypeToId { get; }
        IReadOnlyDictionary<Type, IComponentConverter> Converters { get; }
        ICollection<Type> Registered { get; }

        int Register<T>() where T : ComponentBase;
    }
}
