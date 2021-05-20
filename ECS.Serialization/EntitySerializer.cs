using System;
using System.Collections.Generic;
using System.Linq;
using ECS.Core;
using ECS.Serialization.Contexts;
using ECS.Serialization.Readers;
using ECS.Serialization.Tokenizers;
using ECS.Serialization.Writers;

namespace ECS.Serialization
{
    public class EntitySerializer
    {
        private readonly Tokenizer _tokenizer = new Tokenizer();
        private readonly ComponentSerializer _componentSerializer = new ComponentSerializer();

        public string Serialize(IEcsContext context, Entity entity)
        {
            var writer = new SequentialWriter();
            Serialize(context, entity, writer);
            return writer.ToString();
        }

        public void Serialize(IEcsContext context, Entity entity, ISequentialWriter writer)
        {
            var registeredTypeComponentPairs = GetRegisteredTypeComponentsPairs(context, entity);
            // Пишем число компонентов.
            writer.WriteInt32(registeredTypeComponentPairs.Count);

            foreach (var typeComponentPair in registeredTypeComponentPairs)
            {
                _componentSerializer.Serialize(context, typeComponentPair.Key, typeComponentPair.Value, writer);
            }
        }

        public Entity Deserialize(IEcsContext context, string input)
        {
            var tokens = _tokenizer.Parse(input);
            var reader = new SequentialReader(tokens);
            return Deserialize(context, reader);
        }

        public Entity Deserialize(IEcsContext context, ISequentialReader reader)
        {
            var result = new Entity();
            // Читаем число компонентов.
            int componentsCount = reader.ReadInt32();

            for (int i = 0; i < componentsCount; i++)
            {
                var component = _componentSerializer.Deserialize(context, reader);
                result.Add(component);
            }

            return result;
        }

        private List<KeyValuePair<Type, ComponentBase>> GetRegisteredTypeComponentsPairs(
            IEcsContext context,
            Entity entity) 
            => entity
                .Where(p => context.Registered.Contains(p.Key))
                .ToList();
    }
}
