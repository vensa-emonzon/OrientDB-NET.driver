using System;
using System.Reflection;

namespace Orient.Client.Mapping
{
    internal class DateTimeFieldMapping<TTarget> : BasicNamedFieldMapping<TTarget>
    {
        public DateTimeFieldMapping(PropertyInfo propertyInfo, string fieldPath)
            : base(propertyInfo, fieldPath)
        {

        }

        protected override void MapToNamedField(ODocument document, TTarget typedObject)
        {
            var dateTime = document.GetField<DateTime>(FieldPath);
            SetPropertyValue(typedObject, dateTime);
        }
    }
}
