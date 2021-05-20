using System;
using System.Collections.Generic;
using System.Text;
using ECS.Core;
using ECS.Serialization.Contexts;
using ECS.Serialization.Readers;
using ECS.Serialization.Writers;

namespace ECS.Serialization
{
    public class ComponentSerializer
    {
        public void Serialize(IEcsContext context, Type componentType, ComponentBase component, ISequentialWriter writer)
        {
            // Пишем id компонента.
            writer.WriteInt32(context.TypeToId[componentType]);

            if (context.Converters.TryGetValue(componentType, out var converter))
            {
                converter.ToTokensSequence(component, writer);
            }
        }

        public ComponentBase Deserialize(IEcsContext context, ISequentialReader reader)
        {
            ComponentBase result;
            // Читаем id компонента.
            int id = reader.ReadInt32();
            var componentType = context.IdToType[id];

            if (context.Converters.TryGetValue(componentType, out var converter))
            {
                result = converter.FromTokenSequence(reader);
            }
            else
            {
                result = Activator.CreateInstance(componentType) as ComponentBase;
            }

            return result;
        }
    }
}
