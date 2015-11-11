using System.Reflection;

namespace Orient.Client.Mapping
{
    internal class OridFieldMapping<TTarget> : FieldMapping<TTarget>
    {
        public OridFieldMapping(PropertyInfo propertyInfo, string fieldPath) : base(propertyInfo, fieldPath)
        {
            
        }

        protected override void MapToObject(ODocument document, TTarget typedObject)
        {
            var orid = (FieldPath == "Orid")
                ? document.Orid
                : document.GetField<Orid>(FieldPath) ?? Orid.Null;

            SetPropertyValue(typedObject, orid);
        }

        protected override void MapToDocument(TTarget typedObject, ODocument document)
        {
            var orid = (Orid)GetPropertyValue(typedObject);

            if (FieldPath == "Orid")
                document.Orid = orid;
            else
                document.SetField(FieldPath, orid ?? Orid.Null);
        }
    }
}
