using System;
using System.Collections.Generic;
using ECS.Core;
using ECS.Serialization;
using ECS.Serialization.Attributes;
using ECS.Serialization.Contexts;
using ECS.Serialization.Converters;
using ECS.Serialization.Readers;
using ECS.Serialization.Writers;

namespace SerializationDemo
{
    [ComponentConverter(typeof(IdComponentConverter))]
    public class IdComponent : ComponentBase
    {
        public uint Id { get; set; }
    }

    public class IdComponentConverter : IComponentConverter
    {
        public void ToTokensSequence(object component, ISequentialWriter writer)
        {
            var idComponent = (IdComponent) component;
            writer.WriteUInt32(idComponent.Id);
        }

        public ComponentBase FromTokenSequence(ISequentialReader reader)
        {
            throw new NotImplementedException();
        }
    }

    [ComponentConverter(typeof(VelocityComponentConverter))]
    public class VelocityComponent : ComponentBase
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public class VelocityComponentConverter : IComponentConverter
    {
        public void ToTokensSequence(object component, ISequentialWriter writer)
        {
            var velocityComponent = (VelocityComponent) component;
            writer.WriteFloat(velocityComponent.X);
            writer.WriteFloat(velocityComponent.Y);
        }

        public ComponentBase FromTokenSequence(ISequentialReader reader)
        {
            throw new NotImplementedException();
        }
    }

    [ComponentConverter(typeof(HealthComponentConverter))]
    public class HealthComponent : ComponentBase
    {
        public uint CurrentHealth { get; set; }
        public uint MaxHealth { get; set; }
    }

    public class HealthComponentConverter : IComponentConverter
    {
        public void ToTokensSequence(object component, ISequentialWriter writer)
        {
            var healthComponent = (HealthComponent) component;
            writer.WriteUInt32(healthComponent.CurrentHealth);
            writer.WriteUInt32(healthComponent.MaxHealth);
        }

        public ComponentBase FromTokenSequence(ISequentialReader reader)
        {
            throw new NotImplementedException();
        }
    }

    public class IsAliveComponent : ComponentBase { }

    class Program
    {
        static void Main(string[] args)
        {
            var context = new EcsContext();
            context.Register<IdComponent>();
            context.Register<VelocityComponent>();
            context.Register<HealthComponent>();
            context.Register<IsAliveComponent>();

            var serializer = new WorldSerializer();

            var entity1 = new Entity()
                .Add(new IdComponent {Id = 1})
                .Add(new VelocityComponent {X = 10, Y = 5})
                .Add(new HealthComponent {CurrentHealth = 100, MaxHealth = 100})
                .Add(new IsAliveComponent());

            var entity2 = new Entity()
                .Add(new IdComponent { Id = 2 })
                .Add(new VelocityComponent { X = 0, Y = 0 })
                .Add(new HealthComponent { CurrentHealth = 0, MaxHealth = 100 });

            var entities = new List<Entity> {entity1, entity2};
            var data = serializer.Serialize(context, entities);

            foreach (var s in data.Split(' '))
            {
                Console.WriteLine($"\t{s}");
            }

            Console.ReadKey();
        }
    }
}
