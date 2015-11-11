using Orient.Client.API.Query.Interfaces;
using Orient.Client.Protocol;
using Orient.Client.Protocol.Operations;
using Orient.Client.Protocol.Operations.Command;

// syntax: 
// CREATE EDGE [<class>] 
// [CLUSTER <cluster>] 
// FROM <rid>|(<query>)|[<rid>]* 
// TO <rid>|(<query>)|[<rid>]* 
// [SET <field> = <expression>[,]*]

namespace Orient.Client
{
    public class OSqlCreateEdge : IOCreateEdge
    {
        private SqlQuery _sqlQuery;
        private Connection _connection;

        public OSqlCreateEdge()
        {
            _sqlQuery = new SqlQuery(null);
        }
        internal OSqlCreateEdge(Connection connection)
        {
            _connection = connection;
            _sqlQuery = new SqlQuery(connection);
        }

        #region Edge

        public IOCreateEdge Edge(string className)
        {
            _sqlQuery.Edge(className);

            return this;
        }

        public IOCreateEdge Edge<T>(T obj)
        {
            ODocument document;

            if (obj is ODocument)
            {
                document = obj as ODocument;
            }
            else
            {
                document = ODocument.ToDocument(obj);
            }

            string className = document.OClassName;

            if (typeof(T) == typeof(OEdge))
            {
                className = "E";
            }
            else if (string.IsNullOrEmpty(document.OClassName))
            {
                throw new OException(OExceptionType.Query, "Document doesn't contain OClassName value.");
            }

            _sqlQuery.Edge(className);
            _sqlQuery.Set(document);

            return this;
        }

        public IOCreateEdge Edge<T>()
        {
            return Edge(typeof(T).Name);
        }

        #endregion

        #region Cluster

        public IOCreateEdge Cluster(string clusterName)
        {
            _sqlQuery.Cluster(clusterName);

            return this;
        }

        public IOCreateEdge Cluster<T>()
        {
            return Cluster(typeof(T).Name);
        }

        #endregion

        #region From

        public IOCreateEdge From(Orid Orid)
        {
            _sqlQuery.From(Orid);

            return this;
        }

        public IOCreateEdge From<T>(T obj)
        {
            ODocument document;

            if (obj is ODocument)
            {
                document = obj as ODocument;
            }
            else
            {
                document = ODocument.ToDocument(obj);
            }

            if (document.Orid == Orid.Null)
            {
                throw new OException(OExceptionType.Query, "Document doesn't contain Orid value.");
            }

            _sqlQuery.From(document.Orid);

            return this;
        }

        #endregion

        #region To

        public IOCreateEdge To(Orid Orid)
        {
            _sqlQuery.To(Orid);

            return this;
        }

        public IOCreateEdge To<T>(T obj)
        {
            ODocument document;

            if (obj is ODocument)
            {
                document = obj as ODocument;
            }
            else
            {
                document = ODocument.ToDocument(obj);
            }
            
            if (document.Orid == Orid.Null)
            {
                throw new OException(OExceptionType.Query, "Document doesn't contain Orid value.");
            }

            _sqlQuery.To(document.Orid);

            return this;
        }

        #endregion

        #region Set

        public IOCreateEdge Set<T>(string fieldName, T fieldValue)
        {
            _sqlQuery.Set<T>(fieldName, fieldValue);

            return this;
        }

        public IOCreateEdge Set<T>(T obj)
        {
            _sqlQuery.Set(obj);

            return this;
        }

        #endregion

        #region Run

        public OEdge Run()
        {
            CommandPayloadCommand payload = new CommandPayloadCommand();
            payload.Text = ToString();

            var operation = new Command(_connection.Database)
                                {
                                    OperationMode = OperationMode.Synchronous,
                                    CommandPayload = payload
                                };

            var result = new OCommandResult(_connection.ExecuteOperation(operation));
            return result.ToSingle()?.To<OEdge>();
        }

        public T Run<T>() where T : class, new()
        {
            return Run().To<T>();
        }

        #endregion

        public override string ToString()
        {
            return _sqlQuery.ToString(QueryType.CreateEdge);
        }
    }
}
