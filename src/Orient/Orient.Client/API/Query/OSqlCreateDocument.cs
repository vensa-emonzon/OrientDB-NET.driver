using Orient.Client.API.Query.Interfaces;
using Orient.Client.Protocol;
using Orient.Client.Protocol.Operations;
using Orient.Client.Protocol.Operations.Command;

// shorthand for INSERT INTO for documents

namespace Orient.Client
{
    public class OSqlCreateDocument : IOCreateDocument
    {
        private SqlQuery _sqlQuery;
        private Connection _connection;

        public OSqlCreateDocument()
        {
            _sqlQuery = new SqlQuery(null);
        }

        internal OSqlCreateDocument(Connection connection)
        {
            _connection = connection;
            _sqlQuery = new SqlQuery(connection);
        }

        #region Document

        public IOCreateDocument Document(string className)
        {
            _sqlQuery.Class(className);

            return this;
        }

        public IOCreateDocument Document<T>(T obj)
        {
            // check for OClassName shouldn't have be here since INTO clause might specify it

            _sqlQuery.Insert(obj);

            return this;
        }

        public IOCreateDocument Document<T>()
        {
            return Document(typeof(T).Name);
        }

        #endregion

        #region Cluster

        public IOCreateDocument Cluster(string clusterName)
        {
            _sqlQuery.Cluster(clusterName);

            return this;
        }

        public IOCreateDocument Cluster<T>()
        {
            return Cluster(typeof(T).Name);
        }

        #endregion

        #region Set

        public IOCreateDocument Set<T>(string fieldName, T fieldValue)
        {
            _sqlQuery.Set(fieldName, fieldValue);
            return this;
        }

        public IOCreateDocument Set<T>(T obj)
        {
            _sqlQuery.Set(obj);

            return this;
        }

        #endregion

        #region Run

        public ODocument Run()
        {
            return Run<ODocument>();
        }

        public T Run<T>() where T : class, new()
        {
            var operation = new Command(_connection.Database)
                                {
                                    OperationMode = OperationMode.Synchronous,
                                    CommandPayload = new CommandPayloadCommand { Text = ToString() }
                                };

            var result = new OCommandResult(_connection.ExecuteOperation(operation));
            return result.ToSingle()?.To<T>();
        }

        #endregion

        public override string ToString()
        {
            return _sqlQuery.ToString(QueryType.Insert);
        }
    }
}
