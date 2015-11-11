namespace Orient.Client
{
    public abstract class OBaseRecord : IBaseRecord
    {
        private string _oClassName;
        private Orid _orid = Orid.Null;

        #region Orient record specific properties

        public Orid Orid
        {
            get { return _orid; }
            set { _orid = value ?? Orid.Null; }
        }

        public int OVersion { get; set; }

        public short OClassId { get; set; }

        public string OClassName {
            get
            {
                if (string.IsNullOrEmpty(_oClassName))
                {
                    return GetType().Name;
                }

                return _oClassName;
            }

            set
            {
                _oClassName = value;
            }
        }

        #endregion

        public ODocument ToDocument()
        {
            return ODocument.ToDocument(this);
        }
    }
}
