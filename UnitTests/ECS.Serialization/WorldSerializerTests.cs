using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ECS.Core;
using ECS.Serialization;
using ECS.Serialization.Attributes;
using ECS.Serialization.Converters;
using ECS.Serialization.Readers;
using ECS.Serialization.Writers;
using Xunit;

namespace UnitTests.ECS.Serialization
{
    [ComponentConverter(typeof(TestComponent1Converter))]
    public class TestComponent1 : ComponentBase
    {
        public int Id;
        public float Angle;

        public override bool Equals(object obj) 
            => obj is TestComponent1 component && Equals(component);

        public bool Equals(TestComponent1 other) 
            => this.Id == other.Id && this.Angle == other.Angle;
    }

    [ComponentConverter(typeof(TestComponent2Converter))]
    public class TestComponent2 : ComponentBase
    {
        public int[] Values;

        public override bool Equals(object? obj) 
            => obj is TestComponent2 component && Equals(component);

        public bool Equals(TestComponent2 other)
            => this.Values.SequenceEqual(other.Values);
    }

    public class TestComponent3 : ComponentBase
    {
        public override bool Equals(object? obj) 
            => obj is TestComponent3;
    }

    public class TestComponent4 : ComponentBase
    {
        public int Number;
    }

    public class TestComponent1Converter : IComponentConverter
    {
        public void ToTokensSequence(object component, ISequentialWriter writer)
        {
            var testComponent1 = (TestComponent1) component;
            writer.WriteInt32(testComponent1.Id);
            writer.WriteFloat(testComponent1.Angle);
        }

        public ComponentBase FromTokenSequence(ISequentialReader reader)
        {
            return new TestComponent1 {Id = reader.ReadInt32(), Angle = reader.ReadFloat()};
        }
    }

    public class TestComponent2Converter : IComponentConverter
    {
        public void ToTokensSequence(object component, ISequentialWriter writer)
        {
            var testComponent2 = (TestComponent2)component;
            writer.WriteInt32(testComponent2.Values.Length);

            foreach (var value in testComponent2.Values)
            {
                writer.WriteInt32(value);
            }
        }

        public ComponentBase FromTokenSequence(ISequentialReader reader)
        {
            var result = new TestComponent2();
            var length = reader.ReadInt32();
            result.Values = new int[length];

            for (int i = 0; i < result.Values.Length; i++)
            {
                result.Values[i] = reader.ReadInt32();
            }

            return result;
        }
    }

    public class WorldSerializerFixture
    {
        public WorldSerializer WorldSerializer { get; }

        public WorldSerializerFixture()
        {
            WorldSerializer = new WorldSerializer();
            WorldSerializer.Register<TestComponent1>();
            WorldSerializer.Register<TestComponent2>();
            WorldSerializer.Register<TestComponent3>();
        }
    }

    public class WorldSerializerTests : IClassFixture<WorldSerializerFixture>
    {
        private WorldSerializerFixture _fixture;

        public WorldSerializerTests(WorldSerializerFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [ClassData(typeof(WorldStatesGenerator))]
        public void Serialize_Entities_DeserializeSerializedEntity(IEnumerable<Entity> entities)
        {
            var serialized = _fixture.WorldSerializer.Serialize(entities);
            var deserialized = _fixture.WorldSerializer.Deserialize(serialized);

            Assert.True(Equal(entities, deserialized));
        }

        private bool Equal(IEnumerable<Entity> expected, IEnumerable<Entity> actual)
        {
            var expectedSnapshot = expected as Entity[] ?? expected.ToArray();
            var actualSnapshot = actual as Entity[] ?? actual.ToArray();

            if (expectedSnapshot.Length != actualSnapshot.Length) return false;

            var pairs = expectedSnapshot.Zip(actualSnapshot, (e1, e2) => (e1, e2));

            foreach (var (e1, e2) in pairs)
            {
                foreach (var typeComponentPair in e1)
                {
                    var component1 = typeComponentPair.Value;

                    if (_fixture.WorldSerializer.Registered.Contains(component1.GetType()))
                    {
                        var component2 = e2.Get(typeComponentPair.Key);

                        if (component2 == null || !component2.Equals(component1))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }

    public class WorldStatesGenerator : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new[]
                {
                    new Entity()
                        .Add(new TestComponent1 {Angle = 5, Id = 1})
                        .Add(new TestComponent2 {Values = new [] {1, 2, 3}})
                        .Add(new TestComponent3())
                        .Add(new TestComponent4 {Number = 6}),

                    new Entity()
                        .Add(new TestComponent1 {Angle = 7, Id = 2})
                        .Add(new TestComponent2 {Values = new [] {3, 2, 1}})
                        .Add(new TestComponent4 {Number = 9}),

                    new Entity()
                        .Add(new TestComponent1() {Angle = 8, Id = 3})
                        .Add(new TestComponent2() {Values = new [] {5, 6, 7}}),
                    
                    new Entity()
                        .Add(new TestComponent4 {Number = 0}),
                }
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
