using System;
using ECS.Serialization.Converters;

namespace ECS.Serialization.Attributes
{
    class ComponentConverterAttribute : Attribute
    {
        public IComponentConverter ComponentConverter { get; }

        public ComponentConverterAttribute(IComponentConverter componentConverter) 
            => ComponentConverter = componentConverter;
    }
}
