using System.Collections.Generic;

namespace Orient.Client.Mapping
{
    internal class AllFieldMapping<TTarget> : FieldMapping<TTarget> 
    {
        public AllFieldMapping() : base(null, null)
        {
        }

        protected override void MapToObject(ODocument document, TTarget typedObject)
        {
            var target = (ODocument) (object) typedObject;
            foreach (KeyValuePair<string, object> item in document)
            {
                target.SetField(item.Key, item.Value);
            }
        }

        protected override void MapToDocument(TTarget typedObject, ODocument document)
        {
            var source = (ODocument)(object)typedObject;
            foreach (KeyValuePair<string, object> item in source)
            {
                document.SetField(item.Key, item.Value);
            }
        }
    }
}