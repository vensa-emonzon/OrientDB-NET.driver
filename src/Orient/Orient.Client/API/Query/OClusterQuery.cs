using System.Collections.Generic;
using System.Linq;
using Orient.Client.Protocol;
using Orient.Client.Protocol.Operations;

namespace Orient.Client.API.Query
{
    public class OClusterQuery
    {
        private readonly List<OCluster> _clusterIds = new List<OCluster>();
        private readonly Connection _connection;

        internal OClusterQuery(Connection _connection)
        {
            this._connection = _connection;
        }

        internal void AddClusterId(OCluster cluster)
        {
            if (!_clusterIds.Contains(cluster))
                _clusterIds.Add(cluster);
        }

        public long Count()
        {
            var operation = new DataClusterCount(_connection.Database) { Clusters = _clusterIds.Select(c => c.Id).ToList() };
            var document = _connection.ExecuteOperation(operation);
            return document.GetField<long>("count");
        }
        public ODocument Range()
        {
            var document = new ODocument();
            foreach (var cluster in _clusterIds)
            {
                var operation = new DataClusterDataRange(_connection.Database) { ClusterId = cluster.Id };
                var d = _connection.ExecuteOperation(operation);
                document.SetField(!string.IsNullOrEmpty(cluster.Name) ? cluster.Name : cluster.Id.ToString(), d.GetField<ODocument>("Content"));
            }
            return document;
        }
    }
}
