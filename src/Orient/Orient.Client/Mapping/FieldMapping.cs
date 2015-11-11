using System;
using System.Reflection;

namespace Orient.Client.Mapping
{
    interface IFieldMapping
    {
        void MapToObject(ODocument document, object typedObject);
        void MapToDocument(object typedObject, ODocument document);
    }

    internal abstract class FieldMapping<TTarget> : IFieldMapping
    {
        private readonly Action<TTarget, object> _setter;
        private readonly Func<TTarget, object> _getter;

        protected string FieldPath { get; private set; }

        protected PropertyInfo PropertyInfo { get; private set; }

        protected FieldMapping(PropertyInfo propertyInfo, string fieldPath)
        {
            if (propertyInfo != null)
            {
                _setter = FastPropertyAccessor.BuildUntypedSetter<TTarget>(propertyInfo);
                _getter = FastPropertyAccessor.BuildUntypedGetter<TTarget>(propertyInfo);
            }

            FieldPath = fieldPath;
            PropertyInfo = propertyInfo;
        }

        protected object GetPropertyValue(TTarget target)
        {
            return _getter(target);
        }

        protected void SetPropertyValue(TTarget target, object value)
        {
                _setter(target, value);
        }

        protected abstract void MapToObject(ODocument document, TTarget typedObject);

        protected abstract void MapToDocument(TTarget typedObject, ODocument document);

        public void MapToObject(ODocument document, object typedObject)
        {
            MapToObject(document, (TTarget)typedObject);
        }

        public void MapToDocument(object typedObject, ODocument document)
        {
            MapToDocument((TTarget)typedObject, document);
        }
    }
    
    internal abstract class NamedFieldMapping<TTarget> : FieldMapping<TTarget>
    {
        protected NamedFieldMapping(PropertyInfo propertyInfo, string fieldPath)
            : base(propertyInfo, fieldPath)
        {
        }

        protected override void MapToObject(ODocument document, TTarget typedObject)
        {
            if (document.Contains(FieldPath))
                MapToNamedField(document, typedObject);
        }

        protected abstract void MapToNamedField(ODocument document, TTarget typedObject);
    }
}