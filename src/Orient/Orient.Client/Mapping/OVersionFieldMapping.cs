using System.Reflection;

namespace Orient.Client.Mapping
{
    internal class OVersionFieldMapping<TTarget> : FieldMapping<TTarget>
    {
        public OVersionFieldMapping(PropertyInfo propertyInfo)
            : base(propertyInfo, "OVersion")
        {

        }

        protected override void MapToObject(ODocument document, TTarget typedObject)
        {
            SetPropertyValue(typedObject, document.OVersion);
        }

        protected override void MapToDocument(TTarget typedObject, ODocument document)
        {
            document.OVersion = (int)GetPropertyValue(typedObject);
        }
    }
}