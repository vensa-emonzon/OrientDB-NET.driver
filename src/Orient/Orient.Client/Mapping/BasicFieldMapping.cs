﻿using System.Reflection;

namespace Orient.Client.Mapping
{
    internal class BasicNamedFieldMapping<TTarget> : NamedFieldMapping<TTarget>
    {
        public BasicNamedFieldMapping(PropertyInfo propertyInfo, string fieldPath) : base(propertyInfo, fieldPath)
        {
            
        }

        protected override void MapToNamedField(ODocument document, TTarget typedObject)
        {
            SetPropertyValue(typedObject, document.GetField<object>(FieldPath));
        }

        protected override void MapToDocument(TTarget typedObject, ODocument document)
        {
            object value = GetPropertyValue(typedObject);
            if (value == null) return;
            document.SetField(FieldPath, value);
        }
    }
}