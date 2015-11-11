using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orient.Client.Mapping
{
    internal abstract class CollectionNamedFieldMapping<TTarget> : NamedFieldMapping<TTarget>
    {
        private readonly Type _targetElementType;
        protected TypeMapperBase Mapper;
        protected bool NeedsMapping;
        protected Func<object> ElementFactory;

        protected CollectionNamedFieldMapping(PropertyInfo propertyInfo, string fieldPath)
            : base(propertyInfo, fieldPath)
        {
            _targetElementType = GetTargetElementType();
            NeedsMapping = !NeedsNoConversion(_targetElementType);
            if (NeedsMapping)
            {
                Mapper = TypeMapperBase.GetInstanceFor(_targetElementType);
                ElementFactory = FastConstructor.BuildConstructor(_targetElementType);
            }
        }

        protected abstract object CreateCollectionInstance(int collectionSize);
        protected abstract void AddItemToCollection(object collection, int index, object item);

        protected override void MapToNamedField(ODocument document, TTarget typedObject)
        {
            object sourcePropertyValue = document.GetField<object>(FieldPath);

            var collection = sourcePropertyValue as IList;

            if (collection == null) // if we only have one item currently stored (but scope for more) we create a temporary list and put our single item in it.
            {
                collection = new ArrayList();
                if (sourcePropertyValue != null)
                {
                    // TODO: Implement in derived class due Different collection mapings
                    if (typeof(HashSet<object>).IsAssignableFrom(sourcePropertyValue.GetType()))
                    {
                        foreach (var item in (HashSet<object>)sourcePropertyValue)
                        {
                            collection.Add(item);
                        }
                    }
                    else
                    {
                        collection.Add(sourcePropertyValue);
                    }
                }
            }

            // create instance of property type
            var collectionInstance = CreateCollectionInstance(collection.Count);

            for (int i = 0; i < collection.Count; i++)
            {
                var t = collection[i];
                object oMapped = t;
                if (NeedsMapping)
                {
                    try
                    {
                        object element = ElementFactory();

                        Mapper.ToObject((ODocument)t, element);
                        oMapped = element;
                    }
                    catch
                    {
                        // FIX: somtimes collection of embeded documents returned as Orid Collection;
                    }
                }

                AddItemToCollection(collectionInstance, i, oMapped);
            }

            SetPropertyValue(typedObject, collectionInstance);
        }

        private Type GetTargetElementType()
        {
            if (PropertyInfo.PropertyType.IsArray)
                return PropertyInfo.PropertyType.GetElementType();
            if (PropertyInfo.PropertyType.IsGenericType)
                return PropertyInfo.PropertyType.GetGenericArguments().First();

            throw new NotImplementedException();

        }

        protected bool NeedsNoConversion(Type elementType)
        {
            return elementType.IsPrimitive ||
                   (elementType == typeof(string)) ||
                   (elementType == typeof(DateTime)) ||
                   (elementType == typeof(decimal)) ||
                   (elementType == typeof(Orid)) ||
                   (elementType.IsValueType);
        }

        protected override void MapToDocument(TTarget typedObject, ODocument document)
        {
            var targetElementType = NeedsMapping ? typeof(ODocument) : _targetElementType;
            var listType = typeof(List<>).MakeGenericType(targetElementType);
            var targetList = (IList)Activator.CreateInstance(listType);

            var sourceList = (IEnumerable)GetPropertyValue(typedObject);
            if (sourceList != null)
            {
                foreach (var item in sourceList)
                    targetList.Add(NeedsMapping ? Mapper.ToDocument(item) : item);
            }

            document.SetField(FieldPath, targetList);
        }
    }
}