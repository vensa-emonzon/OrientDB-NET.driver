using System;
using System.Collections.Generic;
using System.Linq;
using Orient.Client.Protocol;
using Orient.Client.Protocol.Operations;
using Orient.Client.Transactions;

// ReSharper disable UnusedMember.Global

namespace Orient.Client.API
{


    public class OTransaction
    {
        private readonly Connection _connection;

        internal OTransaction(Connection connection)
        {
            _connection = connection;
            _tempClusterId = -1;
            _tempObjectId = -1;

            /*
             * using-tx-log tells if the server must use the Transaction Log to recover the transaction. 
             * 1 = true, 0 = false. 
             * Use always 1 (true) by default to assure consistency. 
             * NOTE: Disabling the log could speed up execution of transaction, 
             * but can't be rollbacked in case of error. 
             * This could bring also at inconsistency in indexes as well, 
             * because in case of duplicated keys the rollback is not called to restore the index status.
             */
            UseTransactionLog = true;
        }

        private readonly Dictionary<Orid, TransactionRecord> _records = new Dictionary<Orid, TransactionRecord>();

        internal bool UseTransactionLog { get; set; }

        private readonly short _tempClusterId;

        private long _tempObjectId;

        public void Commit()
        {
            CommitTransaction ct = new CommitTransaction(_records.Values.ToList(), _connection.Database);
            ct.UseTransactionLog = UseTransactionLog;
            var result = _connection.ExecuteOperation(ct);
            Dictionary<Orid, Orid> mapping = result.GetField<Dictionary<Orid, Orid>>("CreatedRecordMapping");

            var survivingRecords = _records.Values.Where(x => x.RecordType != RecordType.Delete).ToList();

            foreach (var kvp in mapping)
            {
                var record = _records[kvp.Key];
                record.Orid = kvp.Value;
                _records.Add(record.Orid, record);
                if (record.Document != null)
                    _connection.Database.ClientCache.Add(kvp.Value, record.Document);
            }

            var versions = result.GetField<Dictionary<Orid, int>>("UpdatedRecordVersions");
            foreach (var kvp in versions)
            {
                var record = _records[kvp.Key];
                record.Version = kvp.Value;
            }

            foreach (var record in survivingRecords)
            {
                if (record.Object != null)
                {
                    OridUpdaterBase.GetInstanceFor(record.Object.GetType()).UpdateOrids(record.Object, mapping);
                }
                else
                {
                    OridUpdaterBase.GetInstanceFor(record.Document.GetType()).UpdateOrids(record.Document, mapping);
                }
            }

            Reset();
        }

        public void Reset()
        {
            _records.Clear();
        }

        public void Add<T>(T typedObject) where T : IBaseRecord
        {
            var record = new TypedTransactionRecord<T>(RecordType.Create, typedObject);
            Insert(record);
        }

        public void AddEdge(OEdge edge, OVertex from, OVertex to)
        {
            Add(edge);
            edge.SetField("out", from.Orid);
            edge.SetField("in", to.Orid);

            appendOridToField(from, "out_" + edge.OClassName, edge.Orid);
            appendOridToField(to, "in_" + edge.OClassName, edge.Orid);

            if (!_records.ContainsKey(from.Orid))
                Update(from);

            if (!_records.ContainsKey(to.Orid))
                Update(to);
        }

        private void appendOridToField(ODocument document, string field, Orid Orid)
        {
            if (document.Contains(field))
            {
                document.GetField<HashSet<Orid>>(field).Add(Orid);
            }
            else
            {
                var Orids = new HashSet<Orid>();
                Orids.Add(Orid);
                document.SetField(field, Orids);
            }
        }

        public void Update<T>(T typedObject) where T : IBaseRecord
        {
            var record = new TypedTransactionRecord<T>(RecordType.Update, typedObject);
            Insert(record);
        }

        public void Delete<T>(T typedObject) where T : IBaseRecord
        {
            var record = new TypedTransactionRecord<T>(RecordType.Delete, typedObject);
            Insert(record);
        }

        private void Insert(TransactionRecord record)
        {
            bool hasOrid = record.Orid != Orid.Null;
            bool needsOrid = record.RecordType != RecordType.Create;

            if (hasOrid && !needsOrid)
                throw new InvalidOperationException("Objects to be added via a transaction must not already be in the database");

            if (needsOrid && !hasOrid)
                throw new InvalidOperationException("Objects to be updated or deleted via a transaction must already be in the database");

            if (!hasOrid)
            {
                record.Orid = CreateTempOrid();
                record.Orid = new Orid(_connection.Database.GetClusterIdFor(record.OClassName), record.Orid.ClusterPosition);
            }

            if (_records.ContainsKey(record.Orid))
            {
                if (record.RecordType != _records[record.Orid].RecordType)
                    throw new InvalidOperationException("Same object already part of transaction with a different CRUD intent");
                _records[record.Orid] = record;
            }
            else
            {
                _records.Add(record.Orid, record);
            }
        }

        private Orid CreateTempOrid()
        {
            return new Orid(_tempClusterId, --_tempObjectId);
        }

        public T GetPendingObject<T>(Orid Orid) where T : IBaseRecord
        {
            TransactionRecord record;
            if (_records.TryGetValue(Orid, out record))
            {
                return (T)record.Object;
            }
            return default(T);
        }

        public void AddOrUpdate<T>(T target) where T : IBaseRecord
        {
            if (target.Orid == Orid.Null)
                Add(target);
            else if (!_records.ContainsKey(target.Orid))
                Update(target);
        }
    }
}
