﻿using System.Collections.Generic;
using Orient.Client.Protocol.Operations;
using Orient.Client.Protocol.Serializers;

namespace Orient.Client.Protocol
{
    internal class Request
    {
        internal OperationMode OperationMode { get; set; }
        internal List<RequestDataItem> DataItems { get; }
        internal Connection Connection { get; private set; }
        internal int SessionId { get; private set; }

        internal Request(Connection connection)
        {
            OperationMode = OperationMode.Synchronous;
            DataItems = new List<RequestDataItem>();
            Connection = connection;
        }

        internal void AddDataItem(byte b)
        {
            DataItems.Add(new RequestDataItem { Type = "byte", Data = BinarySerializer.ToArray(b) });
        }
        internal void AddDataItem(byte[] b)
        {
            DataItems.Add(new RequestDataItem { Type = "bytes", Data = b });
        }

        internal void AddDataItem(short s)
        {
            DataItems.Add(new RequestDataItem { Type = "short", Data = BinarySerializer.ToArray(s) });
        }
        internal void AddDataItem(int i)
        {
            DataItems.Add(new RequestDataItem { Type = "int", Data = BinarySerializer.ToArray(i) });
        }
        internal void AddDataItem(long l)
        {
            DataItems.Add(new RequestDataItem { Type = "long", Data = BinarySerializer.ToArray(l) });
        }
        internal void AddDataItem(string s)
        {
            DataItems.Add(new RequestDataItem { Type = "string", Data = BinarySerializer.ToArray(s) });
        }
        internal void AddDataItem(Orid orid)
        {
            if (Orid.ClusterIdSize == sizeof(short))
                AddDataItem((short)orid.ClusterId);
            else
                AddDataItem(orid.ClusterId);

            AddDataItem(orid.ClusterPosition);
       }

        internal void SetSessionId(int sessionId)
        {
            SessionId = sessionId;
        }
    }
}
