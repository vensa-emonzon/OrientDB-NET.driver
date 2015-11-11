namespace Orient.Client
{
    public class OEdge : ODocument
    {
        [OProperty(Alias = "in", Serializable = false)]
        public Orid InV => GetField<Orid>("in");

        [OProperty(Alias = "out", Serializable = false)]
        public Orid OutV => GetField<Orid>("out");

        [OProperty(Alias = "label", Serializable = false)]
        public string Label 
        {
            get
            {
                var label = GetField<string>("@OClassName");
                return string.IsNullOrEmpty(label) ? GetType().Name : label;
            }
        }
    }
}
