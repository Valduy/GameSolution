using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Network;
using Xunit;

namespace UnitTests.Network
{
    public class ConcurrentNetworkBufferTests
    {
        [Theory]
        [ClassData(typeof(MessagesGroupsGenerator))]
        public void Read_MultipleMessagesWrite_ReturnWrittenMessages(
            byte[][][] messagesGroupsToWrite,
            byte[][][] messagesGroupsToRead, 
            int size)
        {
            var buffer = new ConcurrentNetworkBuffer(size);

            for (var i = 0; i < messagesGroupsToWrite.Length; i++)
            {
                foreach (var message in messagesGroupsToWrite[i])
                {
                    buffer.Write(message);
                }

                foreach (var message in messagesGroupsToRead[i])
                {
                    Assert.Equal(message, buffer.Read());
                }
            }
        }

        [Theory]
        [ClassData(typeof(MessagesGroupsGenerator))]
        public void ReadLast_MultipleMessagesWrite_ReturnLastWrittenMessage(
            byte[][][] messagesGroupsToWrite,
            byte[][][] messagesGroupsToRead,
            int size)
        {
            var buffer = new ConcurrentNetworkBuffer(size);

            for (var i = 0; i < messagesGroupsToWrite.Length; i++)
            {
                foreach (var message in messagesGroupsToWrite[i])
                {
                    buffer.Write(message);
                }

                Assert.Equal(messagesGroupsToRead[i].Last(), buffer.ReadLast());
            }
        }

        [Theory]
        [ClassData(typeof(MessagesGroupsGenerator))]
        public void IsFull_MultipleMessagesWrite_ReturnCorrectFullStatus(
            byte[][][] messagesGroupsToWrite,
            byte[][][] messagesGroupsToRead,
            int size)
        {
            var buffer = new ConcurrentNetworkBuffer(size);

            for (var i = 0; i < messagesGroupsToWrite.Length; i++)
            {
                foreach (var message in messagesGroupsToWrite[i])
                {
                    buffer.Write(message);
                }

                var isFull = buffer.IsFull;
                buffer.ReadLast();

                Assert.Equal(messagesGroupsToRead[i].Length == size, isFull);
            }
        }

        [Theory]
        [ClassData(typeof(MessagesGroupsGenerator))]
        public void IsEmpty_MultipleMessagesWrite_ReturnCorrectEmptyStatus(
            byte[][][] messagesGroupsToWrite,
            byte[][][] messagesGroupsToRead,
            int size)
        {
            var buffer = new ConcurrentNetworkBuffer(size);

            foreach (var group in messagesGroupsToWrite)
            {
                foreach (var message in group)
                {
                    buffer.Write(message);
                }

                var isEmptyAfterWrite = buffer.IsEmpty;
                buffer.ReadLast();
                var isEmptyAfterRead = buffer.IsEmpty;

                Assert.False(isEmptyAfterWrite);
                Assert.True(isEmptyAfterRead);
            }
        }

        [Theory]
        [ClassData(typeof(MessagesGroupsGenerator))]
        public void Count_MultipleMessagesWrite_ReturnCorrectMessagesCount(
            byte[][][] messagesGroupsToWrite,
            byte[][][] messagesGroupsToRead,
            int size)
        {
            var buffer = new ConcurrentNetworkBuffer(size);

            for (var i = 0; i < messagesGroupsToWrite.Length; i++)
            {
                foreach (var message in messagesGroupsToWrite[i])
                {
                    buffer.Write(message);
                }

                var count = buffer.Count;
                buffer.ReadLast();

                Assert.Equal(messagesGroupsToRead[i].Length, count);
            }
        }
    }

    public class MessagesGroupsGenerator : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            // Пустой буфер.
            yield return new object[]
            {
                new[]
                {
                    new []
                    {
                        new byte[]{}
                    }
                },
                new[]
                {
                    new []
                    {
                        new byte[]{}
                    }
                },
                5
            };

            // Одна группа, сообщений меньше, чем размер буфера.
            yield return new object[]
            {
                new[]
                {
                    new []
                    {
                        new byte[]{1, 2, 3, 4},
                        new byte[]{5, 6},
                        new byte[]{7, 8, 9},
                    }
                },
                new[]
                {
                    new []
                    {
                        new byte[]{1, 2, 3, 4},
                        new byte[]{5, 6},
                        new byte[]{7, 8, 9},
                    }
                },
                5
            };

            // Одна группа, сообщения заполняют весь буфер.
            yield return new object[]
            {
                new[]
                {
                    new []
                    {
                        new byte[]{1, 2, 3, 4},
                        new byte[]{5, 6},
                        new byte[]{7, 8, 9},
                        new byte[]{10, 11, 12, 13},
                        new byte[]{14},
                    }
                },
                new[]
                {
                    new []
                    {
                        new byte[]{1, 2, 3, 4},
                        new byte[]{5, 6},
                        new byte[]{7, 8, 9},
                        new byte[]{10, 11, 12, 13},
                        new byte[]{14},
                    }
                },
                5
            };

            // Одна группа, сообщений больше, чем может поместиться в буфер.
            yield return new object[]
            {
                new[]
                {
                    new []
                    {
                        new byte[]{1, 2, 3, 4},
                        new byte[]{5, 6},
                        new byte[]{7, 8, 9},
                        new byte[]{10, 11, 12, 13},
                        new byte[]{14},
                        new byte[]{15, 16, 17},
                    }
                },
                new[]
                {
                    new []
                    {
                        new byte[]{7, 8, 9},
                        new byte[]{10, 11, 12, 13},
                        new byte[]{14},
                        new byte[]{15, 16, 17},
                    }
                },
                4
            };

            // Несколько групп разных размеров.
            yield return new object[]
            {
                new[]
                {
                    new []
                    {
                        new byte[]{1, 2, 3, 4},
                        new byte[]{5, 6},
                        new byte[]{7, 8, 9},
                        new byte[]{10, 11, 12, 13},
                        new byte[]{14},
                        new byte[]{15, 16, 17},
                    },
                    new []
                    {
                        new byte[]{1, 2, 3, 4},
                        new byte[]{5, 6},
                        new byte[]{7, 8, 9},
                        new byte[]{10, 11, 12, 13},
                        new byte[]{14},
                        new byte[]{15, 16, 17},
                    },
                    new []
                    {
                        new byte[]{7, 8, 9},
                        new byte[]{10, 11, 12, 13},
                    },
                    new []
                    {
                        new byte[]{7, 8, 9},
                    },
                },
                new[]
                {
                    new []
                    {
                        new byte[]{7, 8, 9},
                        new byte[]{10, 11, 12, 13},
                        new byte[]{14},
                        new byte[]{15, 16, 17},
                    },
                    new []
                    {
                        new byte[]{7, 8, 9},
                        new byte[]{10, 11, 12, 13},
                        new byte[]{14},
                        new byte[]{15, 16, 17},
                    },
                    new []
                    {
                        new byte[]{7, 8, 9},
                        new byte[]{10, 11, 12, 13},
                    },
                    new []
                    {
                        new byte[]{7, 8, 9},
                    },
                },
                4
            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
