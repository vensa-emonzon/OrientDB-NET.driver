using System;
using System.Collections.Generic;
using Orient.Client.Protocol.Serializers;

namespace Orient.Client.Protocol.Operations
{
    internal class SBTreeBonsaiGetEntriesMajor : BaseOperation
    {
        internal long FileId;
        internal long PageIndex;
        internal int PageOffset;
        internal Orid FirstKey;
        internal bool Inclusive;

        public SBTreeBonsaiGetEntriesMajor(ODatabase database)
            : base(database)
        {
            _operationType = OperationType.SBTREE_BONSAI_GET_ENTRIES_MAJOR;
        }

        public override Request Request(Request request)
        {
            // (collectionPointer)(key:binary)(inclusive:boolean)(pageSize:int)
            base.Request(request);

            request.OperationMode = OperationMode.Synchronous;

            // collection pointer
            request.AddDataItem(FileId);
            request.AddDataItem(PageIndex);
            request.AddDataItem(PageOffset);

            // key binary
            var cid = BinarySerializer.ToArray(FirstKey.ClusterId);
            var cpos = BinarySerializer.ToArray(FirstKey.ClusterPosition);
            byte[] Orid = new byte[cid.Length + cpos.Length];
            Array.Copy(cid, Orid, cid.Length);
            Array.Copy(cpos, 0, Orid, cid.Length, cpos.Length);
            request.AddDataItem(Orid);

            // inclusive
            request.AddDataItem((byte)(Inclusive ? 1 : 0)); // 0 - false 1 - true

            // page size
            if (OClient.ProtocolVersion >= 21)
                request.AddDataItem((int)128);

            return request;
        }

        public override ODocument Response(Response response)
        {
            Dictionary<Orid, int> entries = new Dictionary<Orid, int>();

            ODocument document = new ODocument();
            if (response == null)
            {
                return document;
            }

            var reader = response.Reader;
            if (response.Connection.ProtocolVersion > 26 && response.Connection.UseTokenBasedSession)
                ReadToken(reader);

            // (count:int)[(key:binary)(value:binary)]
            var bytesLength = reader.ReadInt32EndianAware();
            var count = reader.ReadInt32EndianAware();
            for (int i = 0; i < count; i++)
            {
                // key
                short clusterId = reader.ReadInt16EndianAware();
                long clusterPosition = reader.ReadInt64EndianAware();
                var rid = new Orid(clusterId, clusterPosition);

                // value
                var value = reader.ReadInt32EndianAware();

                entries.Add(rid, value);
            }
            document.SetField<Dictionary<Orid, int>>("entries", entries);
            return document;
        }
    }
}
