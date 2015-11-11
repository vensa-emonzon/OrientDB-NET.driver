﻿using System;
using System.Reflection;

namespace Orient.Client.Mapping
{
    internal class DecimalFieldMapping<TTarget> : BasicNamedFieldMapping<TTarget>
    {
        public DecimalFieldMapping(PropertyInfo propertyInfo, string fieldPath)
            : base(propertyInfo, fieldPath)
        {

        }
        protected override void MapToNamedField(ODocument document, TTarget typedObject)
        {
            // Only until bug #3483 will be fixed than use decimal
            object item = document.GetField<object>(FieldPath);
            if (item is IConvertible)
                SetPropertyValue(typedObject, Convert.ChangeType(item, typeof(Decimal)));
        }

    }
}
