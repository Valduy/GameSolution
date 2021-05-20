using System;
using ECS.Serialization.Converters;

namespace ECS.Serialization.Attributes
{
    public class ComponentConverterAttribute : Attribute
    {
        public IComponentConverter ComponentConverter { get; }

        public ComponentConverterAttribute(Type type) 
            => ComponentConverter = (IComponentConverter)Activator.CreateInstance(type);
    }
}
