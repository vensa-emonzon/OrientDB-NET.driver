using System;
using System.Reflection;

namespace Orient.Client.Mapping
{
    internal class EnumFieldMapping<TTarget> : BasicNamedFieldMapping<TTarget>
    {
        public EnumFieldMapping(PropertyInfo propertyInfo, string fieldPath)
            : base(propertyInfo, fieldPath)
        {

        }

        protected override void MapToNamedField(ODocument document, TTarget typedObject)
        {
            var field = document.GetField<object>(FieldPath);
            var value = Enum.Parse(PropertyInfo.PropertyType, field.ToString(), true);
            SetPropertyValue(typedObject, value);
        }

        protected override void MapToDocument(TTarget typedObject, ODocument document)
        {
            var value = (int)GetPropertyValue(typedObject);
            document.SetField(FieldPath, value);
        }
    }
}
