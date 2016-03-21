using System;
using System.Collections.Generic;

namespace Orient.Client.API.Types
{
    internal class TypeConverter
    {
        static TypeConverter()
        {
            AddType<int>(OType.Integer);
            AddType<long>(OType.Long);
            AddType<short>(OType.Short);
            AddType<string>(OType.String);
            AddType<bool>(OType.Boolean);
            AddType<float>(OType.Float);
            AddType<double>(OType.Double);
            AddType<DateTime>(OType.DateTime);
            AddType<byte[]>(OType.Binary);
            AddType<byte>(OType.Byte);
            AddType<decimal>(OType.Decimal);
            AddType<HashSet<Orid>>(OType.LinkSet);
            AddType<List<Orid>>(OType.LinkList);
            AddType<Orid>(OType.Link);
            AddType<Guid>(OType.String);
        }

        private static void AddType<T>(OType name)
        {
            _types.Add(typeof(T), name);
        }

        static Dictionary<Type, OType> _types = new Dictionary<Type, OType>();

        public static OType TypeToDbName(Type t)
        {
            OType result;
            if (_types.TryGetValue(t, out result))
                return result;

            if (t.IsEnum)
            {
                return OType.Integer;
            }

            throw new ArgumentException("propertyType " + t.Name + " is not yet supported.");
        }
    }
}
