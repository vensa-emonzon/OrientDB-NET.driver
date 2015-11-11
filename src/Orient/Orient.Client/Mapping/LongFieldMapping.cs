using System;
using System.Reflection;

namespace Orient.Client.Mapping
{
    internal class LongFieldMapping<TTarget> : BasicNamedFieldMapping<TTarget>
    {
        public LongFieldMapping(PropertyInfo propertyInfo, string fieldPath)
            : base(propertyInfo, fieldPath)
        {

        }
        protected override void MapToNamedField(ODocument document, TTarget typedObject)
        {
            object item = document.GetField<object>(FieldPath);
            if (item is IConvertible)
                SetPropertyValue(typedObject, Convert.ChangeType(item, typeof(long)));
        }
    }
}
