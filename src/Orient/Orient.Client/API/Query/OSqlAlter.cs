using Orient.Client.API.Query;
using Orient.Client.Protocol;
using Orient.Client.Protocol.Operations.Command;

// ReSharper disable UnusedMember.Global

// syntax: 
// DROP CLASS <class> 

namespace Orient.Client
{
    public class OSqlAlter
    {
        private readonly Connection _connection;
        private OCommandQuery _command;

        #region Constructors

        internal OSqlAlter(Connection connection)
        {
            _connection = connection;
        }

        #endregion

        #region Methods

        public OSqlAlter Abstract<T>(bool makeAbstract)
        {
            var className = typeof(T).Name;
            var payload = new CommandPayloadCommand { Text = $"ALTER CLASS {className} ABSTRACT {makeAbstract}" };
            _command = new OCommandQuery(_connection, payload);
            return this;
        }

        public OSqlAlter Superclass(string className, string superclassName)
        {
            var payload = new CommandPayloadCommand { Text = $"ALTER CLASS {className} SUPERCLASS {superclassName}" };
            _command = new OCommandQuery(_connection, payload);
            return this;
        }

        public OSqlAlter Superclass<T, TU>()
        {
            return Superclass(typeof(T).Name, typeof(TU).Name);
        }

        public OSqlAlter AddCluster<T>(string clusterName)
        {
            var className = typeof(T).Name;
            var payload = new CommandPayloadCommand { Text = $"ALTER CLASS {className} ADDCLUSTER {clusterName}" };
            _command = new OCommandQuery(_connection, payload);
            return this;
        }

        public OSqlAlter RemoveCluster<T>(int clusterId)
        {
            var className = typeof(T).Name;
            var payload = new CommandPayloadCommand { Text = $"ALTER CLASS {className} REMOVECLUSTER {clusterId}" };
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
