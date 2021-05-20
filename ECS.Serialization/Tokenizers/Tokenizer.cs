using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECS.Serialization.Tokenizers
{
    public class Tokenizer : ITokenizer
    {
        private delegate void State(Tokenizer context);

        private delegate bool Condition(Tokenizer context);

        private static readonly Dictionary<State, List<(Condition, State)>> Table = 
            new Dictionary<State, List<(Condition, State)>>
            {
                {Skip, new List<(Condition, State)>
                {
                    (IsEnd, null),
                    (IsQuote, BeginReadToken),
                }},
                {BeginReadToken, new List<(Condition, State)>
                {
                    (IsQuote, WriteEmptyString),
                    (IsTrue, ReadToken),
                }},
                {ReadToken, new List<(Condition, State)>
                {
                    (IsQuote, EndReadToken),
                    (IsTrue, ReadToken),
                }},
                {EndReadToken, new List<(Condition, State)>
                {
                    (IsTrue, CollectToken)
                }},
                {CollectToken, new List<(Condition, State)>
                {
                    (IsEnd, null),
                    (IsSpace, Skip)
                }},
                {WriteEmptyString, new List<(Condition, State)>
                {
                    (IsEnd, null),
                    (IsSpace, Skip)
                }}
            };

        private static readonly Dictionary<State, char> Expected = new Dictionary<State, char>
        {
            {Skip, '\"'},
            {BeginReadToken, '\"'},
            {ReadToken, '\"'},
            {CollectToken, ' '}
        };

        private string _input;
        private int _offset;
        private Stack<char> _stack;
        private LinkedList<string> _output;
        private State _state;

        public LinkedList<string> Parse(string input)
        {
            _input = input;
            _offset = 0;
            _stack = new Stack<char>();
            _output = new LinkedList<string>();
            _state = Skip;
            Parse();
            return _output;
        }

        private void Parse()
        {
            while (true)
            {
                var prevState = _state;

                try
                {
                    _state = GoTo();

                    if (_state == null)
                    {
                        return;
                    }

                    _state(this);
                }
                catch (InvalidOperationException)
                {
                    throw new TokenizerException($"Ожидалось: {Expected[prevState]}, а встречено: {_input[_offset]}");
                }
                catch (IndexOutOfRangeException)
                {
                    throw new TokenizerException($"Ожидалось: {Expected[prevState]}, а встречен конец строки");
                }
            }
        }

        private State GoTo() => Table[_state].First(t => t.Item1(this)).Item2;

        private static bool IsTrue(Tokenizer context) => true;
        private static bool IsSpace(Tokenizer context) => context._input[context._offset] == ' ';
        private static bool IsQuote(Tokenizer context) => context._input[context._offset] == '\"';
        private static bool IsEnd(Tokenizer context) => context._offset == context._input.Length;

        private static void BeginReadToken(Tokenizer context) => Skip(context);

        private static void ReadToken(Tokenizer context)
        {
            context._stack.Push(context._input[context._offset]);
            context._offset++;
        }

        private static void EndReadToken(Tokenizer context) => Skip(context);

        private static void WriteEmptyString(Tokenizer context)
        {
            context._output.AddLast(string.Empty);
            context._offset++;
        }

        private static void CollectToken(Tokenizer context)
        {
            var builder = new StringBuilder();

            while (context._stack.Any())
            {
                builder.Insert(0, context._stack.Pop());
            }

            context._output.AddLast(builder.ToString());
        }

        private static void Skip(Tokenizer context) => context._offset++;
    }
}
