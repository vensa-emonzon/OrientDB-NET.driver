using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Orient.Client.Mapping
{
    internal sealed class DictionaryFieldMapping<TTarget> : CollectionNamedFieldMapping<TTarget>
    {
        private readonly Func<int, object> _dictionaryFactory;
        private readonly Type _keyType;
        private readonly Type _valueType;

        public DictionaryFieldMapping(PropertyInfo propertyInfo, string fieldPath)
            : base(propertyInfo, fieldPath)
        {
            Type dictionaryType = propertyInfo.PropertyType;

            _keyType = propertyInfo.PropertyType.GetGenericArguments()[0];
            _valueType = propertyInfo.PropertyType.GetGenericArguments()[1];

            NeedsMapping = !NeedsNoConversion(_valueType);
            if (NeedsMapping)
            {
                Mapper = TypeMapperBase.GetInstanceFor(_valueType);
                ElementFactory = FastConstructor.BuildConstructor(_valueType);
            }

            if (propertyInfo.PropertyType.IsInterface)
            {
                dictionaryType = typeof(List<>).MakeGenericType(_keyType);
            }

            _dictionaryFactory = FastConstructor.BuildConstructor<int>(dictionaryType);
        }

        protected override object CreateCollectionInstance(int collectionSize)
        {
            return _dictionaryFactory(collectionSize);
        }

        protected override void MapToNamedField(ODocument document, TTarget typedObject)
        {
            var sourcePropertyValue = document.GetField<ODocument>(FieldPath);
            var collection = CreateCollectionInstance(sourcePropertyValue.Count);
            AddItemToCollection(collection, 0, sourcePropertyValue);
            SetPropertyValue(typedObject, collection);
        }

        protected override void AddItemToCollection(object collection, int index, object item)
        {
            foreach (var element in (ODocument)item)
            {
                object key = element.Key;
                object value = element.Value;

                if (_keyType == typeof(short) || _keyType == typeof(int) || _keyType == typeof(long))
                {
                    key = Convert.ChangeType(key, _keyType);
                }
                else if (_keyType == typeof(Guid))
                {
                    key = Guid.Parse(key.ToString());
                }
                else if (_keyType.IsEnum)
                {
                    key = Enum.Parse(_keyType, key.ToString());
                }

                if (_valueType == typeof(short) || _valueType == typeof(int) || _valueType == typeof(long))
                {
                    value = Convert.ChangeType(value, _valueType);
                }
                else if (_valueType == typeof(Guid))
                {
                    value = Guid.Parse(value.ToString());
                }
                else if (_valueType.IsEnum)
                {
                    value = Enum.Parse(_valueType, value.ToString());
                }

                if (NeedsMapping)
                {
                    var oMaped = ElementFactory();
                    Mapper.ToObject((ODocument)value, oMaped);
                    value = oMaped;
                }

                ((IDictionary)collection).Add(key, value);
            }
        }

        protected override void MapToDocument(TTarget typedObject, ODocument document)
        {

            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(_keyType, NeedsMapping ? typeof(ODocument) : _valueType);

            var targetDictionary = (IDictionary)Activator.CreateInstance(dictionaryType);

            var sourceList = (IDictionary)GetPropertyValue(typedObject);

            if (sourceList != null)
            {
                var enumerator = sourceList.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    targetDictionary.Add(enumerator.Key, NeedsMapping ? Mapper.ToDocument(enumerator.Value) : enumerator.Value);
                }
            }

            document.SetField(FieldPath, targetDictionary);
        }
    }
}
