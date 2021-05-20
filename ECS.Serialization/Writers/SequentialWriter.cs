using System.Globalization;
using System.Text;

namespace ECS.Serialization.Writers
{
    public class SequentialWriter : ISequentialWriter
    {
        private readonly StringBuilder _builder;

        public SequentialWriter()
        {
            _builder = new StringBuilder();
        }

        public void WriteBool(bool value) => WriteString(value.ToString());

        public void WriteByte(byte value) => WriteString(value.ToString());

        public void WriteInt16(short value) => WriteString(value.ToString());

        public void WriteInt32(int num) => WriteString(num.ToString());

        public void WriteInt64(long value) => WriteString(value.ToString());

        public void WriteUInt16(ushort value) => WriteString(value.ToString());

        public void WriteUInt32(uint num) => WriteString(num.ToString());

        public void WriteUInt64(ulong value) => WriteString(value.ToString());

        public void WriteFloat(float num) => WriteString(num.ToString(CultureInfo.InvariantCulture));

        public void WriteDouble(double value) => WriteString(value.ToString(CultureInfo.InvariantCulture));

        public void WriteString(string token)
        {
            if (_builder.Length > 0) Space();
            Quote();
            _builder.Append(token);
            Quote();
        }

        public override string ToString() => _builder.ToString();

        private void Quote() => _builder.Append('\"');

        private void Space() => _builder.Append(' ');
    }
}
