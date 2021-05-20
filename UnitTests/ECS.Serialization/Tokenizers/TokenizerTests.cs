using System.Collections;
using System.Collections.Generic;
using ECS.Serialization.Tokenizers;
using Xunit;

namespace UnitTests.ECS.Serialization.Tokenizers
{
    public class TokenizerTests
    {
        [Theory]
        [ClassData(typeof(CorrectTokensSequencesGenerator))]
        public void Parse_ValidInputSequence_CorrectTokenSequence(string input, IEnumerable<string> expected)
        {
            var tokenizer = new Tokenizer();

            var result = tokenizer.Parse(input);

            Assert.Equal(expected, result);
        }

        [Theory]
        [ClassData(typeof(IncorrectTokensSequencesGenerator))]
        public void Parse_InvalidInputSequences_ThrowTokenizerException(string input)
        {
            var tokenizer = new Tokenizer();

            Assert.Throws<TokenizerException>(() => tokenizer.Parse(input));
        }
    }

    public class CorrectTokensSequencesGenerator : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                "\"1\" \"2\" \"string\"",
                new List<string> {"1", "2", "string"},
            };
            yield return new object[]
            {
                "\"1\" \"\" \"string\" \"8.652342\"",
                new List<string> {"1", "", "string", "8.652342"},
            };
            yield return new object[]
            {
                "",
                new List<string>(),
            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class IncorrectTokensSequencesGenerator : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                "\"1\"\"2\" \"string\"",
            };
            yield return new object[]
            {
                "\"1\" \"\"  \"string\" \"8.652342\"",
            };
            yield return new object[]
            {
                "\"1\" \"\"  \"string\" \"\"8.652342\"",
            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
