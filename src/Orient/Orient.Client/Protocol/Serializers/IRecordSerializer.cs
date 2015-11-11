namespace Orient.Client.Protocol.Serializers
{
    public interface IRecordSerializer
    {
        byte[] Serialize(ODocument document);
        ODocument Deserialize(byte[] rawRecord, ODocument document);
    }
}
