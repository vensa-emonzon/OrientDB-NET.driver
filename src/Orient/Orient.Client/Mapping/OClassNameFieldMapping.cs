using System.Reflection;

namespace Orient.Client.Mapping
{
    internal class OClassNameFieldMapping<TTarget> : FieldMapping<TTarget>
    {
        public OClassNameFieldMapping(PropertyInfo propertyInfo)
            : base(propertyInfo, "OClassName")
        {

        }

        protected override void MapToObject(ODocument document, TTarget typedObject)
        {
            SetPropertyValue(typedObject, document.OClassName);
        }

        protected override void MapToDocument(TTarget typedObject, ODocument document)
        {
            document.OClassName = (string) GetPropertyValue(typedObject);
        }
    }
}