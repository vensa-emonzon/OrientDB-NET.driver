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


    internal class TimeSpanFieldMapping<TTarget> : BasicNamedFieldMapping<TTarget>
    {
        public TimeSpanFieldMapping(PropertyInfo propertyInfo, string fieldPath)
            : base(propertyInfo, fieldPath)
        {

        }

        protected override void MapToDocument(TTarget typedObject, ODocument document)
        {
            var value = (TimeSpan)GetPropertyValue(typedObject);
            document.SetField(FieldPath, value.Ticks);
        }

        protected override void MapToNamedField(ODocument document, TTarget typedObject)
        {
            var timeSpan = document.GetField<TimeSpan>(FieldPath);
            SetPropertyValue(typedObject, timeSpan);
        }
    }
}
