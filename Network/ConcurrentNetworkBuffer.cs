namespace Network
{
    public class ConcurrentNetworkBuffer : INetworkBuffer, IReadOnlyNetworkBuffer
    {
        private readonly byte[][] _messages;

        private int _start;
        private int _end;

        public bool IsEmpty
        {
            get
            {
                lock (_messages)
                {
                    return !IsFull && _start == _end;
                }
            }
        }

        public bool IsFull { get; private set; }

        public int Size { get; }

        public int Count
        {
            get
            {
                lock (_messages)
                {
                    if (IsFull) return Size;
                    if (IsEmpty) return 0;
                    return _start < _end
                        ? _end - _start
                        : Size - _start + _end + 1;
                }
            }
        }

        public ConcurrentNetworkBuffer(int size)
        {
            Size = size;
            _messages = new byte[size][];
        }

        public void Write(byte[] message)
        {
            lock (_messages)
            {
                _messages[_end] = message;

                if (IsFull)
                {
                    Move(ref _start);
                    Move(ref _end);
                }
                else
                {
                    Move(ref _end);
                    IsFull = _start == _end;
                }
            }
        }

        public byte[] Read()
        {
            lock (_messages)
            {
                if (IsEmpty) return null;
                IsFull = false;
                var start = _start;
                Move(ref _start);
                return _messages[start];
            }
        }

        public byte[] ReadLast()
        {
            lock (_messages)
            {
                if (IsEmpty) return null;
                IsFull = false;
                _start = _end;
                return _messages[PrevValue(_end)];
            }
        }

        private void Move(ref int pointer)
            => pointer = NextValue(pointer);

        private int NextValue(int pointer)
            => ++pointer == Size ? 0 : pointer;

        private int PrevValue(int pointer) 
            => pointer == 0 ? Size - 1 : --pointer;
    }
}
