using System.Collections.Generic;

namespace ECS.Serialization.Tokenizers
{
    public interface ITokenizer
    {
        LinkedList<string> Parse(string input);
    }
}
