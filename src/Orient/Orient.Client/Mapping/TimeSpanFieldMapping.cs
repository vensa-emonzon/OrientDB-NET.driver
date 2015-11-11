﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Orient.Client.Mapping
{
    internal class TimeSpanFieldMapping<TTarget> : BasicNamedFieldMapping<TTarget>
    {
        public TimeSpanFieldMapping(PropertyInfo propertyInfo, string fieldPath)
            : base(propertyInfo, fieldPath)
        {

        }

        protected override void MapToNamedField(ODocument document, TTarget typedObject)
        {
            TimeSpan timeSpan = document.GetField<TimeSpan>(FieldPath);

            SetPropertyValue(typedObject, timeSpan);
        }
    }
}
