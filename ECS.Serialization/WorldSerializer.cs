using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ECS.Core;
using ECS.Serialization.Attributes;
using ECS.Serialization.Converters;
using ECS.Serialization.Readers;
using ECS.Serialization.Tokenizers;
using ECS.Serialization.Writers;

namespace ECS.Serialization
{
    public class WorldSerializer
    {
        private readonly Dictionary<int, Type> _idToType = new Dictionary<int, Type>();
        private readonly Dictionary<Type, int> _typeToId = new Dictionary<Type, int>();
        private readonly Dictionary<Type, IComponentConverter>
            _converters = new Dictionary<Type, IComponentConverter>();

        private readonly HashSet<Type> _registered = new HashSet<Type>();

        private readonly Tokenizer _tokenizer = new Tokenizer();
        private readonly EntitySerializer _entitySerializer = new EntitySerializer();

        public IReadOnlyDictionary<int, Type> IdToType => _idToType;
        public IReadOnlyDictionary<Type, int> TypeToId => _typeToId;
        public IReadOnlyDictionary<Type, IComponentConverter> Converters => _converters;
        public ICollection<Type> Registered => _registered;

        public int Register<T>() where T : ComponentBase
        {
            var converterAttribute = GetConverterAttribute(typeof(T));

            if (converterAttribute != null)
            {
                _converters[typeof(T)] = converterAttribute.ComponentConverter;
            }

            if (TypeToId.TryGetValue(typeof(T), out var id))
            {
                return id;
            }

            id = 0;

            if (IdToType.Any())
            {
                id = IdToType.Max(p => p.Key) + 1;
            }

            _idToType[id] = typeof(T);
            _typeToId[typeof(T)] = id;
            _registered.Add(typeof(T));
            return id;
        }

        public string Serialize(IEnumerable<Entity> entities)
        {
            var writer = new SequentialWriter();
            // Пишем число сущностей.
            writer.WriteInt32(entities.Count());

            foreach (var entity in entities)
            {
                _entitySerializer.Serialize(this, entity, writer);
            }

            return writer.ToString();
        }

        public List<Entity> Deserialize(string input)
        {
            var result = new List<Entity>();
            var tokens = _tokenizer.Parse(input);
            var reader = new SequentialReader(tokens);
            // Читаем число сущностей.
            int entityCount = reader.ReadInt32();

            for (int i = 0; i < entityCount; i++)
            {
                var entity = _entitySerializer.Deserialize(this, reader);
                result.Add(entity);
            }

            return result;
        }

        private ComponentConverterAttribute GetConverterAttribute(ICustomAttributeProvider t)
            => t.GetCustomAttributes(true)
                .FirstOrDefault(a => a is ComponentConverterAttribute) as ComponentConverterAttribute;
    }
}
