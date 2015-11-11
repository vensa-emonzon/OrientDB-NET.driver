using System;
using System.Collections.Generic;
using Orient.Client.Protocol;
using Orient.Client.Protocol.Operations;
using Orient.Client.Protocol.Operations.Command;
using Orient.Client.Protocol.Serializers;

namespace Orient.Client.API.Query
{
    public class PreparedQuery
    {
        private readonly string _query;
        private Connection _connection;
        private readonly string _fetchPlan;
        private Dictionary<string, object> _parameters;

        public PreparedQuery(string query, string fetchPlan = "*:0")
        {
            _query = query;
            _fetchPlan = fetchPlan;
        }

        internal void SetConnection(Connection connection)
        {
            _connection = connection;
        }

        public List<ODocument> Run(params string[] properties)
        {
            if (_connection == null)
                throw new ArgumentNullException(nameof(_connection));

            if (_parameters == null)
            {
                _parameters = new Dictionary<string, object>();
            }
            else if (_parameters.Count > 0)
            {
                throw new InvalidOperationException("Can't run prepared query with named parameters");
            }

            for (int i = 0; i < properties.Length; i++)
            {
                _parameters.Add(i.ToString(), properties[i]);
            }

            return RunInternal();

        }

        private List<ODocument> RunInternal()
        {
            try
            {
                if (_parameters == null)
                    throw new ArgumentNullException(nameof(_parameters));

                var paramsDocument = new ODocument { OClassName = "" };
                paramsDocument.SetField("params", _parameters);

                var serializer = RecordSerializerFactory.GetSerializer(_connection.Database);

                var payload = new CommandPayloadQuery
                              {
                                  Text = ToString(),
                                  NonTextLimit = -1,
                                  FetchPlan = _fetchPlan,
                                  SerializedParams = serializer.Serialize(paramsDocument)
                              };

                var operation = new Command(_connection.Database)
                                {
                                    OperationMode = OperationMode.Synchronous,
                                    CommandPayload = payload
                                };

                ODocument document = _connection.ExecuteOperation(operation);
                return document.GetField<List<ODocument>>("Content");
            }
            finally
            {
                _parameters = null;
            }
        }

        public List<ODocument> Run()
        {
            return RunInternal();
        }

        public override string ToString()
        {
            return _query;
        }

        public PreparedQuery Set(string key, object value)
        {
            if (_parameters == null)
                _parameters = new Dictionary<string, object>();

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            _parameters.Add(key, value);

            return this;
        }
    }
}
