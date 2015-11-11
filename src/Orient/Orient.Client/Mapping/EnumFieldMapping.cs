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
            var value = Enum.Parse(PropertyInfo.PropertyType, document.GetField<string>(FieldPath), true);
            SetPropertyValue(typedObject, value);
        }
    }
}
