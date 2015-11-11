using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orient.Client.Transactions
{
    internal abstract class OridUpdaterBase
    {
        protected static Type OridType = typeof (Orid);

        public abstract void UpdateOrids(object oTarget, Dictionary<Orid, Orid> replacements);

        public static OridUpdaterBase GetInstanceFor(Type t)
        {
            var mappingType = typeof(OridUpdater<>).MakeGenericType(t);
            PropertyInfo propertyInfo = mappingType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
            return (OridUpdaterBase)propertyInfo.GetValue(null, null);
        }
    }


    class OridUpdater<T> : OridUpdaterBase
    {
        private static readonly OridUpdater<T> _instance = new OridUpdater<T>();
        public static OridUpdater<T> Instance { get { return _instance; } }

        readonly List<IOridPropertyUpdater> _fields = new List<IOridPropertyUpdater>();

        private OridUpdater()
        {
            Type genericObjectType = typeof(T);

            foreach (PropertyInfo propertyInfo in genericObjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!propertyInfo.CanRead )
                    continue; // read only or write only properties can be ignored

                string propertyName = propertyInfo.Name;
                var propertyType = propertyInfo.PropertyType;

                object[] oProperties = propertyInfo.GetCustomAttributes(typeof(OProperty), true);

                if (oProperties.Any())
                {
                    OProperty oProperty = oProperties.First() as OProperty;
                    if (oProperty != null)
                    {
                        if (!oProperty.Deserializable)
                            continue;
                        propertyName = oProperty.Alias;
                    }
                }

                if (propertyType == OridType && propertyInfo.CanWrite)
                    _fields.Add(new OridSimplePropertyUpdater<T>(propertyInfo));

                if (propertyType.IsArray && propertyType.GetElementType() == OridType)
                    _fields.Add(new OridArrayPropertyUpdater<T>(propertyInfo));

                if (propertyType.IsGenericType && propertyType.GetGenericArguments().First() == OridType)
                {
                    switch (propertyType.Name)
                    {
                        case "HashSet`1":
                            _fields.Add(new OridHashSetUpdater<T>(propertyInfo));
                            break;
                        case "List`1":
                            _fields.Add(new OridListPropertyUpdater<T>(propertyInfo));
                            break;
                        default:
                            throw new NotImplementedException("Generic Orid collection not handled.");
                    }
                    
                }

            }
        }

        public override void UpdateOrids(object oTarget,  Dictionary<Orid, Orid> replacements)
        {
            foreach (var field in _fields)
                field.Update(oTarget, replacements);
        }
    }
}
