using System;
using System.Collections.Generic;
using System.Linq;
using ECS.Core;
using ECS.Serialization.Readers;
using ECS.Serialization.Writers;

namespace ECS.Serialization
{
    public class EntitySerializer
    {
        public void Serialize(WorldSerializer worldSerializer, Entity entity, ISequentialWriter writer)
        {
            var registeredTypeComponentPairs = GetRegisteredTypeComponentsPairs(worldSerializer, entity);
            // Пишем число компонентов.
            writer.WriteInt32(registeredTypeComponentPairs.Count);

            foreach (var typeComponentPair in registeredTypeComponentPairs)
            {
                // Пишем id компонента.
                writer.WriteInt32(worldSerializer.TypeToId[typeComponentPair.Key]);

                if (worldSerializer.Converters.TryGetValue(typeComponentPair.Key, out var converter))
                {
                    converter.ToTokensSequence(typeComponentPair.Value, writer);
                }
            }
        }

        public Entity Deserialize(WorldSerializer worldSerializer, ISequentialReader reader)
        {
            var result = new Entity();
            // Читаем число компонентов.
            int componentsCount = reader.ReadInt32();

            for (int i = 0; i < componentsCount; i++)
            {
                // Читаем id компонентов.
                int id = reader.ReadInt32();
                var componentType = worldSerializer.IdToType[id];
                ComponentBase component = null;

                if (worldSerializer.Converters.TryGetValue(componentType, out var converter))
                {
                    component = converter.FromTokenSequence(reader);
                }
                else
                {
                    component = Activator.CreateInstance(componentType) as ComponentBase;
                }
               
                result.Add(component);
            }

            return result;
        }

        private List<KeyValuePair<Type, ComponentBase>> GetRegisteredTypeComponentsPairs(
            WorldSerializer worldSerializer,
            Entity entity) 
            => entity
                .Where(p => worldSerializer.Registered.Contains(p.Key))
                .ToList();
    }
}
