
using Orient.Client.Protocol.Serializers;

namespace Orient.Client.Protocol.Operations.Command
{
    internal abstract class CommandPayloadBase
    {
        public string ClassName { get; protected set; }
        public string Text { get; set; }
        protected int PayLoadLength => sizeof(int) + BinarySerializer.Length(ClassName) + sizeof(int) + BinarySerializer.Length(Text);
    }
}
