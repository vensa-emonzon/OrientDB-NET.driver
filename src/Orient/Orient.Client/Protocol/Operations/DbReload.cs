using System.Collections.Generic;
using System.Text;

namespace Orient.Client.Protocol.Operations
{
    class DbReload : BaseOperation
    {
        public DbReload(ODatabase database)
            : base(database)
        {
            _operationType = OperationType.DB_RELOAD;
        }
        public override Request Request(Request request)
        {
            base.Request(request);
            return request;
        }

        public override ODocument Response(Response response)
        {
            ODocument document = new ODocument();

            if (response == null)
            {
                return document;
            }

            var reader = response.Reader;
            if (response.Connection.ProtocolVersion > 26 && response.Connection.UseTokenBasedSession)
                ReadToken(reader);

            short clusterCount = reader.ReadInt16EndianAware();
            document.SetField("ClusterCount", clusterCount);

            if (clusterCount > 0)
            {
                List<OCluster> clusters = new List<OCluster>();

                for (int i = 1; i <= clusterCount; i++)
                {
                    int clusterNameLength = reader.ReadInt32EndianAware();
                    OCluster cluster = new OCluster
                                       {
                                           Name = Encoding.Default.GetString(reader.ReadBytes(clusterNameLength)),
                                           Id = reader.ReadInt16EndianAware()
                                       };

                    #region !!! code segment commented out to bypass obsolete warning which will break our build.  We will never use a protocol version less than 30. !!!

                    //if (OClient.ProtocolVersion < 24)
                    //{
                    //    int clusterTypeLength = reader.ReadInt32EndianAware();

                    //    string clusterType = System.Text.Encoding.Default.GetString(reader.ReadBytes(clusterTypeLength));
                    //    //cluster.Type = (OClusterType)Enum.Parse(typeof(OClusterType), clusterType, true);
                    //    if (OClient.ProtocolVersion >= 12)
                    //        cluster.DataSegmentID = reader.ReadInt16EndianAware();
                    //    else
                    //        cluster.DataSegmentID = 0;
                    //}

                    #endregion

                    clusters.Add(cluster);
                }

                document.SetField("Clusters", clusters);
            }

            return document;
        }
    }
}
