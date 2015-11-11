using Orient.Client.API.Query;
using Orient.Client.Protocol;
using Orient.Client.Protocol.Operations.Command;

// ReSharper disable UnusedMember.Global

// syntax: 
// DROP CLASS <class> 

namespace Orient.Client
{
    public class OSqlDeleteClass
    {
        private readonly Connection _connection;
        private OCommandQuery _command;

        #region Constructors

        internal OSqlDeleteClass(Connection connection)
        {
            _connection = connection;
        }

        #endregion

        #region Class

        public OSqlDeleteClass Drop(string className)
        {
            var payload = new CommandPayloadCommand { Text = $"DROP CLASS {className}" };
            _command = new OCommandQuery(_connection, payload);
            return this;
        }

        public OSqlDeleteClass Drop<T>()
        {
            return Drop(typeof(T).Name);
        }

        #endregion

        public int Run()
        {
            return _command.Run().GetScalarResult();
        }


        public override string ToString()
        {
            return _command.ToString();
        }
    }
}
