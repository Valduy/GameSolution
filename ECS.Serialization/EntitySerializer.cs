using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ECS.Core;
using ECS.Serialization.Attributes;
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
                var converter = worldSerializer.Converters[typeComponentPair.Key];
                // Пишем id компонента.
                writer.WriteInt32(worldSerializer.TypeToId[typeComponentPair.Key]);
                converter.ToTokensSequence(typeComponentPair.Value, writer);
            }
        }

        public Entity Deserialize(WorldSerializer worldSerializer, ISequentialReader reader)
        {
            var result = new Entity();
            // Читаем число компонентов.
            int componentsCount = ReadInt32(reader);

            for (int i = 0; i < componentsCount; i++)
            {
                // Читаем id компонентов.
                int id = ReadInt32(reader);
                var componentType = worldSerializer.IdToType[id];
                var converter = worldSerializer.Converters[componentType];
                var component = converter.FromTokenSequence(reader);
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
        
        private int ReadInt32(ISequentialReader reader)
        {
            var componentsCount = 0;
            var first = reader.Peek();

            try
            {
                componentsCount = reader.ReadInt32();
            }
            catch (ArgumentException)
            {
                throw new EcsSerializationException($"Ожидалось целое число, а встречено {first}.");
            }

            return componentsCount;
        }
    }
}
