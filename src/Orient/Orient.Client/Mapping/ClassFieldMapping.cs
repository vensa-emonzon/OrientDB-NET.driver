﻿using System.Reflection;

namespace Orient.Client.Mapping
{
    internal class ClassFieldMapping<TProperty, TTarget> : FieldMapping<TTarget> where TProperty : new()
    {
        public ClassFieldMapping(PropertyInfo propertyInfo, string fieldPath) : base(propertyInfo, fieldPath)
        {
        }

        protected override void MapToObject(ODocument document, TTarget typedObject)
        {
            TProperty result = new TProperty();
            TypeMapper<TProperty>.Instance.ToObject(document.GetField<ODocument>(FieldPath), result);
            SetPropertyValue(typedObject, result);
        }

        protected override void MapToDocument(TTarget typedObject, ODocument document)
        {
            var subDoc = TypeMapper<TProperty>.Instance.ToDocument(GetPropertyValue(typedObject));
            document.SetField(FieldPath, subDoc);
        }
    }
}