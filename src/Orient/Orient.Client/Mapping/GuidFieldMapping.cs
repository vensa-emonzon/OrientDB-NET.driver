using System;
using System.Reflection;

namespace Orient.Client.Mapping
{
    internal class GuidFieldMapping<TTarget> : BasicNamedFieldMapping<TTarget>
    {
        public GuidFieldMapping(PropertyInfo propertyInfo, string fieldPath)
            : base(propertyInfo, fieldPath)
        {

        }
        protected override void MapToNamedField(ODocument document, TTarget typedObject)
        {
            var fieldPathType = document.GetFieldType(FieldPath);
            if (fieldPathType == typeof(Orid))
                SetPropertyValue(typedObject, document.GetField<Guid>(FieldPath));
            else if (fieldPathType == typeof(string))
                SetPropertyValue(typedObject, new Guid(document.GetField<string>(FieldPath)));
            else
                throw new InvalidCastException("Unsupported Guid conversion");
        }
    }
}
