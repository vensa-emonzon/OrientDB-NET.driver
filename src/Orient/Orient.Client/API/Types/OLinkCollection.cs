using System.Collections.Generic;

// ReSharper disable UnusedMember.Global

namespace Orient.Client
{
    internal class OLinkCollection : List<Orid>
    {
        private Orid _orid = Orid.Null;

        internal int PageSize { get; set; }

        internal Orid Root
        {
            get { return _orid; }
            set { _orid = value ?? Orid.Null; }
        }

        internal int KeySize { get; set; }
    }
}
