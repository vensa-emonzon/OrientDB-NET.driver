using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orient.Client.API;
using Orient.Client.API.Query;
using Orient.Client.API.Query.Interfaces;
using Orient.Client.Protocol;
using Orient.Client.Protocol.Operations;
using Orient.Client.Protocol.Operations.Command;

// ReSharper disable UnusedMember.Global

namespace Orient.Client
{
    public class ODatabase : IDisposable
    {
        private bool _containsConnection;
        private ODocument _databaseProperties;

        public IDictionary<Orid, ODocument> ClientCache { get; private set; }

        public OCreate Create => new OCreate(Connection);
        public OSqlAlter Alter => new OSqlAlter(Connection);
        public OSqlDelete Delete => new OSqlDelete(Connection);
        public OLoadRecord Load => new OLoadRecord(Connection);
        public ORecordMetadata Metadata => new ORecordMetadata(Connection);
        public OSqlSchema Schema => new OSqlSchema(Connection);
        public OSqlTruncate Truncate => new OSqlTruncate(Connection);
        public ODocument DatabaseProperties => _databaseProperties ?? (_databaseProperties = RetrieveDatabaseProperties().Result);
        public int ProtocolVersion => Connection.ProtocolVersion;

        internal Connection Connection { get; }

        public OTransaction Transaction { get; private set; }

        internal Connection GetConnection()
        {
            return Connection;
        }

        public ODatabase(string alias)
        {
            Connection = OClient.ReleaseConnection(alias);
            Connection.Database = this;
            ClientCache = new Dictionary<Orid, ODocument>();
            Transaction = new OTransaction(Connection);
            _containsConnection = true;
        }

        private Task<ODocument> RetrieveDatabaseProperties()
        {
            return Task.Factory.StartNew(() =>
            {
                var document = Load.Orid(new Orid(0, 0)).Run();
                var str = Encoding.UTF8.GetString(document.GetField<byte[]>("RawBytes"));
                var values = str.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                var doc = new ODocument();
                doc.SetField("Version", values[0]);
                doc.SetField("LocaleLanguage", values[5]);
                doc.SetField("LocaleCountry", values[6]);
                doc.SetField("DateFormat", values[7]);
                doc.SetField("DateTimeFormat", values[8]);
                doc.SetField("Timezone", values[9]);
                doc.SetField("Charset", values[10]);
                return doc;
            });
        }

        public List<OCluster> GetClusters(bool reload = false)
        {
            while (true)
            {
                if (!reload)
                {
                    return Connection.Document.GetField<List<OCluster>>("Clusters");
                }

                Connection.Reload();
                reload = false;
            }
        }

        public short GetClusterIdFor(string className)
        {
            return Schema.GetDefaultClusterForClass(className);
        }

        public string GetClusterNameFor(short clusterId)
        {
            OCluster oCluster = GetClusters().FirstOrDefault(x => x.Id == clusterId);
            if (oCluster == null)
            {
                Connection.Reload();
                oCluster = GetClusters().FirstOrDefault(x => x.Id == clusterId);
            }
            return oCluster.Name;
        }

        internal OCluster AddCluster(OCluster cluster)
        {
            var clusters = Connection.Document.GetField<List<OCluster>>("Clusters");
            if (!clusters.Contains(cluster))
                clusters.Add(cluster);
            return cluster;
        }

        internal void RemoveCluster(short clusterid)
        {
            var clusters = Connection.Document.GetField<List<OCluster>>("Clusters");
            var cluster = clusters.SingleOrDefault(c => c.Id == clusterid);

            if (cluster != null)
                clusters.Remove(cluster);
        }

        public OSqlSelect Select(params string[] projections)
        {
            return new OSqlSelect(Connection).Select(projections);
        }

        #region Insert

        public IOInsert Insert()
        {
            return new OSqlInsert(Connection);
        }

        public IOInsert Insert<T>(T obj)
        {
            return new OSqlInsert(Connection)
                .Insert(obj);
        }

        #endregion

        #region Update

        public OSqlUpdate Update()
        {
            return new OSqlUpdate(Connection);
        }

        public OSqlUpdate Update(Orid Orid)
        {
            return new OSqlUpdate(Connection)
                .Update(Orid);
        }

        public OSqlUpdate Update<T>(T obj)
        {
            return new OSqlUpdate(Connection)
                .Update(obj);
        }

        #endregion

        #region Query

        public PreparedQuery PreparedQuery(string sql)
        {
            return Query(new PreparedQuery(sql));
        }

        public List<T> Query<T>(string sql, string fetchPlan = "*:0") where T : class, new()
        {
            var docs = Query(sql, fetchPlan);
            return docs.Select(d => d.To<T>()).ToList();
        }

        public List<ODocument> Query(string sql, string fetchPlan = "*:0")
        {
            var payload = new CommandPayloadQuery
                                          {
                                              Text = sql,
                                              NonTextLimit = -1,
                                              FetchPlan = fetchPlan
                                          };

            var operation = new Command(Connection.Database)
                                {
                                    OperationMode = OperationMode.Asynchronous,
                                    CommandPayload = payload
                                };

            ODocument document = Connection.ExecuteOperation(operation);
            return document.GetField<List<ODocument>>("Content");
        }

        public PreparedQuery Query(PreparedQuery query)
        {
            query.SetConnection(Connection);
            return query;
        }

        #endregion

        public OCommandQuery SqlBatch(string batch)
        {
            var payload = new CommandPayloadScript { Language = "sql", Text = batch };
            return new OCommandQuery(Connection, payload);
        }

        public OCommandResult Gremlin(string query)
        {
            var payload = new CommandPayloadScript { Language = "gremlin", Text = query };
            var operation = new Command(Connection.Database) { OperationMode = OperationMode.Synchronous, CommandPayload = payload };
            ODocument document = Connection.ExecuteOperation(operation);
            return new OCommandResult(document);
        }

        public OCommandQuery JavaScript(string query)
        {
            var payload = new CommandPayloadScript { Language = "javascript", Text = query };
            return new OCommandQuery(Connection, payload);
        }

        public OCommandResult Command(string sql)
        {
            var payload = new CommandPayloadCommand { Text = sql };
            var query = new OCommandQuery(Connection, payload);
            return query.Run();
        }

        public PreparedCommand PreparedCommand(string sql)
        {
            return Command(new PreparedCommand(sql));
        }

        public PreparedCommand Command(PreparedCommand command)
        {
            command.SetConnection(Connection);
            return command;
        }

        public long Size
        {
            get
            {
                var operation = new DBSize(Connection.Database);
                var document = Connection.ExecuteOperation(operation);
                return document.GetField<long>("size");
            }
        }

        public long CountRecords
        {
            get
            {
                var operation = new DBCountRecords(Connection.Database);
                var document = Connection.ExecuteOperation(operation);
                return document.GetField<long>("count");
            }
        }

        public void Close()
        {
            if (_containsConnection)
            {
                Connection.Database = null;

                if (Connection.IsReusable)
                {
                    OClient.ReturnConnection(Connection);
                }
                else
                {
                    Connection.Dispose();
                }

                _containsConnection = false;
            }
        }

        public void Dispose()
        {
            Close();
        }

        public OClusterQuery Clusters(params string[] clusterNames)
        {
            return Clusters(clusterNames.Select(n => new OCluster { Name = n, Id = Schema.GetDefaultClusterForClass(n) }));
        }

        private OClusterQuery Clusters(IEnumerable<OCluster> clusters)
        {
            var query = new OClusterQuery(Connection);
            foreach (var id in clusters)
            {
                query.AddClusterId(id);
            }
            return query;
        }

        public OClusterQuery Clusters(params short[] clusterIds)
        {
            return Clusters(clusterIds.Select(id => new OCluster { Id = id }));
        }
    }
}
