namespace Orient.Client.Protocol.Operations
{
    internal class RecordCreate : BaseOperation
    {
        private readonly ODocument _document;

        internal OperationMode OperationMode { get; set; }

        public RecordCreate(ODocument document, ODatabase database)
            : base(database)
        {
            _document = document;
            _database = database;
            _operationType = OperationType.RECORD_CREATE;
        }

        public override Request Request(Request request)
        {
            base.Request(request);

            CorrectClassName();

            if (_document.Orid == Orid.Null)
            {
                var clusterId = _database.GetClusterIdFor(_document.OClassName);
                _document.Orid = new Orid(clusterId, -1);
            }

            if (OClient.ProtocolVersion < 24)
            {
                request.AddDataItem(-1);  // data segment id
            }

            request.AddDataItem((short)_document.Orid.ClusterId);
            request.AddDataItem(Serializer.Serialize(_document));
            request.AddDataItem((byte)ORecordType.Document);
            request.AddDataItem((byte)((OperationMode == OperationMode.Synchronous) ? 0 : 1));


            return request;
        }

        private void CorrectClassName()
        {
            if (_document.OClassName == "OVertex")
                _document.OClassName = "V";
            if (_document.OClassName == "OEdge")
                _document.OClassName = "E";
        }

        public override ODocument Response(Response response)
        {
            ODocument responseDocument = _document;


            if (response == null)
            {
                return responseDocument;
            }

            var reader = response.Reader;
            if (response.Connection.ProtocolVersion > 26 && response.Connection.UseTokenBasedSession)
                ReadToken(reader);

            _document.Orid = (OClient.ProtocolVersion > 25)
                ? Orid.Parse(reader)
                : new Orid(_document.Orid.ClusterId, reader.ReadInt64EndianAware());

            if (OClient.ProtocolVersion >= 11)
            {
                _document.OVersion = reader.ReadInt32EndianAware();
            }

            // Work around differents in storage type < version 2.0
            if (_database.ProtocolVersion >= 28 || (_database.ProtocolVersion >= 20 && _database.ProtocolVersion <= 27 && !EndOfStream(reader)))
            {
                // ReSharper disable once UnusedVariable
                int collectionChangesCount = reader.ReadInt32EndianAware();
                //for (var i = 0; i < collectionChangesCount; i++)
                //{
                //    throw new NotImplementedException("Collection changes not yet handled - failing rather than ignoring potentially significant information");
                //    //var mostSigBits = reader.ReadInt64EndianAware();
                //    //var leastSigBits = reader.ReadInt64EndianAware();
                //    //var updatedFileId = reader.ReadInt64EndianAware();
                //    //var updatedPageIndex = reader.ReadInt64EndianAware();
                //    //var updatedPageOffset = reader.ReadInt32EndianAware();
                //}
            }
            return responseDocument;
        }

    }
}
