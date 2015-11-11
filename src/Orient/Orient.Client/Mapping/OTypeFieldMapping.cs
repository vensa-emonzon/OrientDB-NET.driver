using System.Reflection;

namespace Orient.Client.Mapping
{
    internal class OTypeFieldMapping<TTarget> : FieldMapping<TTarget>
    {
        public OTypeFieldMapping(PropertyInfo propertyInfo)
            : base(propertyInfo, "OType")
        {
            
        }

        protected override void MapToObject(ODocument document, TTarget typedObject)
        {
            SetPropertyValue(typedObject, document.OType);
        }

        protected override void MapToDocument(TTarget typedObject, ODocument document)
        {
            document.OType = (ORecordType)GetPropertyValue(typedObject);
        }
    }
}