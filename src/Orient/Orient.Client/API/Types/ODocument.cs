using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Orient.Client.Mapping;

// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global

namespace Orient.Client
{
    public class ODocument : IEnumerable<KeyValuePair<string, object>>, IBaseRecord
    {
        #region Private Variables

        private static ConcurrentDictionary<Type, bool> ImplementsMap { get; } = new ConcurrentDictionary<Type, bool>();
        private Dictionary<string, object> Fields { get; }

        #endregion

        #region Automatic Properties

        public int Count => Fields.Count;
        internal Dictionary<string, object>.KeyCollection Keys => Fields.Keys;
        internal Dictionary<string, object>.ValueCollection Values => Fields.Values;

        #endregion

        #region Constructors

        public ODocument()
        {
            Fields = new Dictionary<string, object>();
        }

        #endregion


        #region Properties which holds orient specific fields

        public Orid Orid
        {
            get { return Contains("@Orid") ? GetField<Orid>("@Orid") : Orid.Null; }
            set { SetField("@Orid", value ?? Orid.Null); }
        }

        public int OVersion
        {
            get { return GetField<int>("@OVersion"); }
            set { SetField("@OVersion", value); }
        }

        public ORecordType OType
        {
            get { return GetField<ORecordType>("@OType"); }
            set { SetField("@OType", value); }
        }

        public short OClassId
        {
            get { return GetField<short>("@OClassId"); }
            set { SetField("@OClassId", value); }
        }

        public string OClassName
        {
            get { return GetField<string>("@OClassName"); }
            set { SetField("@OClassName", value); }
        }

        #endregion


        #region Methods

        public bool Contains(string fieldPath)
        {
            var document = this;
            string[] fieldKeys;
            return (FindField(ref document, fieldPath, out fieldKeys));
        }

        public T GetField<T>(string fieldPath)
        {
            var document = this;
            string[] fieldKeys;
            var type = typeof(T);

            if (FindField(ref document, fieldPath, out fieldKeys))
            {
                return ConvertField<T>(document, fieldKeys.Last());
            }

            // Field not found; setup a new field to default value.
            var result = (type.IsPrimitive || type == typeof(string) || type.IsArray)
                ? default(T)
                : (T)Activator.CreateInstance(type);

            SetField(fieldPath, result);
            return result;
        }

        public Type GetFieldType(string fieldPath)
        {
            var document = this;
            string[] fieldKeys;

            return FindField(ref document, fieldPath, out fieldKeys) ? Fields[fieldKeys.Last()].GetType() : null;
        }

        public ODocument SetField<T>(string fieldPath, T value)
        {
            var document = this;
            string[] fieldKeys;

            if (!FindField(ref document, fieldPath, out fieldKeys))
            {
                document = this;
                for (var ii = 0; ii < fieldKeys.Length - 1; ++ii)
                {
                    if (!document.Fields.ContainsKey(fieldKeys[ii]))
                        document.Fields.Add(fieldKeys[ii], new ODocument());
                    document = (ODocument)document.Fields[fieldKeys[ii]];
                }
            }

            document.AddOrUpdate(fieldKeys.Last(), value);
            return this;
        }

        public void RemoveField(string fieldPath)
        {
            var document = this;
            string[] fieldKeys;
            if (FindField(ref document, fieldPath, out fieldKeys))
            {
                document.Fields.Remove(fieldKeys.Last());
            }
        }

        public T To<T>() where T : class, new()
        {
            var genericObject = new T();
            genericObject = ToObject(genericObject);
            return genericObject;
        }

        public T ToUnique<T>(ICreationContext store) where T : class
        {

            if (store.AlreadyCreated(Orid))
                return (T)store.GetExistingObject(Orid);

            T genericObject = (T)store.CreateObject(OClassName);
            var result = ToObject(genericObject);
            store.AddObject(Orid, result);
            return result;
        }

        public static ODocument ToDocument<T>(T genericObject)
        {
            return TypeMapperBase.GetInstanceFor(genericObject.GetType()).ToDocument(genericObject);
        }

        #endregion


        #region Overrides

        public override string ToString()
        {
            return Fields.ToString();
        }

        #endregion

        #region Indexer

        public object this[string key]
        {
            get { return GetField<object>(key); }
            set { SetField(key, value); }
        }

        #endregion


        #region IEnumerable

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, object>>)Fields).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion 

        #region Helpers

        private static T ConvertField<T>(ODocument document, string fieldKey)
        {
            var type = typeof(T);
            var fieldValue = document.Fields[fieldKey];

            if (fieldValue == null || fieldValue.GetType() == type)
                return (T)fieldValue;

#if false
            var collection = fieldValue as ICollection;
            if (collection?.Count == 1)
            {
                var first = collection.OfType<object>().First();
                if (first is T)
                    return (T)first;
            }
#endif

            // if value is list or set type, get element type and enumerate over its elements
            if (!type.IsPrimitive && IsIListImplemented(type) && !type.IsArray)
            {
                var value = (T)Activator.CreateInstance(type);
                Type elementType = type.GetGenericArguments()[0];

                foreach (var element in EnumerableFromField(fieldValue))
                {
                    // if current element is ODocument type which is Dictionary<string, object>
                    // map its dictionary data to element instance
                    var current = element as ODocument;
                    if (current != null)
                    {
                        var instance = Activator.CreateInstance(elementType);
                        current.Map(ref instance);

                        ((IList)value).Add(instance);
                    }
                    else
                    {
                        try
                        {
                            ((IList)value).Add(element);
                        }
                        catch
                        {
                            ((IList)value).Add(Convert.ChangeType(element, elementType));
                        }
                    }

                }

                return value;
            }

            if (type.Name == "HashSet`1")
            {
                var value = (T)Activator.CreateInstance(type);
                var elementType = ((IEnumerable)value).GetType().GetGenericArguments()[0];
                var addMethod = type.GetMethod("Add");

                foreach (var element in (IEnumerable)fieldValue)
                {
                    // if current element is ODocument type which is Dictionary<string, object>
                    // map its dictionary data to element instance
                    var current = element as ODocument;
                    if (current != null)
                    {
                        var instance = Activator.CreateInstance(elementType);
                        current.Map(ref instance);
                        addMethod.Invoke(value, new[] { instance });
                    }
                    else
                    {
                        addMethod.Invoke(value, new[] { element });
                    }
                }
                return value;
            }

            if (type == typeof(DateTime))
            {
                DateTime parsedValue;
                if (DateTime.TryParse((string)fieldValue, out parsedValue))
                {
                    return (T)(object)parsedValue;
                }
            }
            else if (type == typeof(Guid))
            {
                if (fieldValue is Orid)
                {
                    return (T)(object)(Guid)(Orid)fieldValue;
                }

                if (fieldValue is string)
                {
                    Guid parsedValue;
                    if (Guid.TryParse((string)fieldValue, out parsedValue))
                    {
                        return (T)(object)parsedValue;
                    }
                }
            }
            else if (type == typeof(decimal))
            {
                return (T)Convert.ChangeType(fieldValue, typeof(T));
            }
            else if (type == typeof(string) && fieldValue is int)
            {
                return (T)(object)fieldValue.ToString();
            }

            return (T)fieldValue;
        }

        private static bool FindField(ref ODocument document, string fieldPath, out string[] fieldKeys)
        {
            fieldKeys = fieldPath.Split('.');

            for (var ii = 0; ii < fieldKeys.Length; ++ii)
            {
                if (document.Fields.ContainsKey(fieldKeys[ii]))
                {
                    if (ii < fieldKeys.Length - 1)
                    {
                        document = (ODocument)document.Fields[fieldKeys[ii]];
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private void AddOrUpdate<T>(string fieldPart, T value)
        {
            if (Fields.ContainsKey(fieldPart))
            {
                Fields[fieldPart] = value;
            }
            else
            {
                Fields.Add(fieldPart, value);
            }
        }

        private static bool IsIListImplemented(Type type)
        {
            bool result;
            if (ImplementsMap.TryGetValue(type, out result))
                return result;

            result = type.GetInterfaces().Contains(typeof(IList));
            ImplementsMap.AddOrUpdate(type, type1 => result, (type1, b) => result);
            return result;
        }

        private static IEnumerable EnumerableFromField(object oField)
        {
            var field = oField as IEnumerable;
            if (field != null)
                return field;
            if (oField == null)
                return (new object[0]);

            return new[] { oField };
        }

        private void Map(ref object obj)
        {
            if (obj is ODocument)
            {
                obj = this;
            }
            else
            {
                Type objType = obj.GetType();

                foreach (KeyValuePair<string, object> item in this)
                {
                    var property = objType.GetProperty(item.Key);
                    property?.SetValue(obj, item.Value, null);
                }
            }
        }

        private T ToObject<T>(T genericObject) where T : class
        {

            TypeMapper<T>.Instance.ToObject(this, genericObject);
            return genericObject;

        }

        #endregion
    }
}