using System.Reflection;

namespace Orient.Client.Mapping
{
    internal class OClassIdFieldMapping<TTarget> : FieldMapping<TTarget>
    {
        public OClassIdFieldMapping(PropertyInfo propertyInfo)
            : base(propertyInfo, "OClassId")
        {

        }

        protected override void MapToObject(ODocument document, TTarget typedObject)
        {
            SetPropertyValue(typedObject, document.OClassId);
        }

        protected override void MapToDocument(TTarget typedObject, ODocument document)
        {
            document.OClassId = (short)GetPropertyValue(typedObject);
        }
    }
}