using Orient.Client.Protocol;
using Orient.Client.Protocol.Operations;

namespace Orient.Client.API.Query
{
    public class ORecordMetadata
    {
        private readonly Connection _connection;
        private Orid _Orid;

        internal ORecordMetadata(Connection connection)
        {
            _connection = connection;
        }

        public ORecordMetadata Orid(Orid Orid)
        {
            _Orid = Orid;
            return this;
        }

        public ODocument Run()
        {
            var operation = new RecordMetadata(_Orid, _connection.Database);
            return _connection.ExecuteOperation(operation);
        }
    }
}
