using System.Collections.Generic;
using System.Linq;
using ECS.Core;
using ECS.Serialization.Contexts;
using ECS.Serialization.Readers;
using ECS.Serialization.Tokenizers;
using ECS.Serialization.Writers;

namespace ECS.Serialization
{
    public class WorldSerializer
    {
        private readonly Tokenizer _tokenizer = new Tokenizer();
        private readonly EntitySerializer _entitySerializer = new EntitySerializer();

        public string Serialize(IEcsContext context, IEnumerable<Entity> entities)
        {
            var writer = new SequentialWriter();
            // Пишем число сущностей.
            writer.WriteInt32(entities.Count());

            foreach (var entity in entities)
            {
                _entitySerializer.Serialize(context, entity, writer);
            }

            return writer.ToString();
        }

        public List<Entity> Deserialize(IEcsContext context, string input)
        {
            var result = new List<Entity>();
            var tokens = _tokenizer.Parse(input);
            var reader = new SequentialReader(tokens);
            // Читаем число сущностей.
            int entityCount = reader.ReadInt32();

            for (int i = 0; i < entityCount; i++)
            {
                var entity = _entitySerializer.Deserialize(context, reader);
                result.Add(entity);
            }

            return result;
        }
    }
}
