using ECS.Core;
using ECS.Serialization.Readers;
using ECS.Serialization.Writers;

namespace ECS.Serialization.Converters
{
    public interface IComponentConverter
    {
        void ToTokensSequence(object component, ISequentialWriter writer);
        ComponentBase FromTokenSequence(ISequentialReader reader);
    }
}
