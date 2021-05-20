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

        public void WriteString(string token)
        {
            if (_builder.Length > 0) Space();
            Quote();
            _builder.Append(token);
            Quote();
        }

        public void WriteInt32(int num) => WriteString(num.ToString());

        public void WriteFloat(float num) => WriteString(num.ToString(CultureInfo.InvariantCulture));

        public override string ToString() => _builder.ToString();

        private void Quote() => _builder.Append('\"');

        private void Space() => _builder.Append(' ');
    }
}
