using System.Text;
using Orient.Client.API.Query;
using Orient.Client.Protocol;
using Orient.Client.Protocol.Operations.Command;

// ReSharper disable UnusedMember.Global

// syntax: 
// DROP CLASS <class> 

namespace Orient.Client
{
    public class OSqlTruncate
    {
        private readonly Connection _connection;
        private readonly StringBuilder _sql;
        private OCommandQuery _command;

        #region Constructors

        internal OSqlTruncate(Connection connection)
        {
            _connection = connection;
            _sql = new StringBuilder("TRUNCATE");
        }

        #endregion

        #region Methods

        public OSqlTruncate Class(string className)
        {
            _sql.Append($" CLASS {className}");
            return this;
        }

        public OSqlTruncate Class<T>()
        {
            return Class(typeof(T).Name);
        }

        public OSqlTruncate Cluster(string clusterName)
        {
            _sql.Append($" CLUSTER {clusterName}");
            var payload = new CommandPayloadCommand { Text = _sql.ToString() };
            _command = new OCommandQuery(_connection, payload);
            return this;
        }

        public OSqlTruncate Polymorphic()
        {
            _sql.Append(" POLYMORPHIC");
            var payload = new CommandPayloadCommand { Text = _sql.ToString() };
            _command = new OCommandQuery(_connection, payload);
            return this;
        }

        public OSqlTruncate Unsafe()
        {
            _sql.Append(" UNSAFE");
            var payload = new CommandPayloadCommand { Text = _sql.ToString() };
            _command = new OCommandQuery(_connection, payload);
            return this;
        }

        public int Run()
        {
            return _command.Run().GetScalarResult();
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return _command.ToString();
        }

        #endregion
    }
}
