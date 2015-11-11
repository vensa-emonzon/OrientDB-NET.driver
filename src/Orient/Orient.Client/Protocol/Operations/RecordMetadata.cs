namespace Orient.Client.Protocol.Operations
{
    internal class RecordMetadata : BaseOperation
    {
        private readonly Orid _orid;

        public RecordMetadata(Orid orid, ODatabase database)
            : base(database)
        {
            _orid = orid;
            _operationType = OperationType.RECORD_METADATA;
        }
        public override Request Request(Request request)
        {
            base.Request(request);
            request.AddDataItem(_orid);
            return request;
        }

        public override ODocument Response(Response response)
        {
            var document = new ODocument();

            if (response == null)
            {
                return document;
            }

            var reader = response.Reader;
            if (response.Connection.ProtocolVersion > 26 && response.Connection.UseTokenBasedSession)
                ReadToken(reader);

            document.Orid = Orid.Parse(reader);
            document.OVersion = reader.ReadInt32EndianAware();

            return document;
        }
    }
}
