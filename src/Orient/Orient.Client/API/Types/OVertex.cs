using System.Collections.Generic;

namespace Orient.Client
{
    public class OVertex : ODocument
    {
        [OProperty(Alias = "in_", Serializable = false)]
        public HashSet<Orid> InE => GetField<HashSet<Orid>>("in_");

        [OProperty(Alias = "out_", Serializable = false)]
        public HashSet<Orid> OutE => GetField<HashSet<Orid>>("out_");

        public OVertex()
        {
            OClassName = "V";
        }
    }
}
