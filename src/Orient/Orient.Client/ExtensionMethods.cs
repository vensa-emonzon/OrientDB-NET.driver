using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Orient.Client.API.Attributes;

namespace Orient.Client
{
    static class ExtensionMethods
    {
        public static OProperty GetOPropertyAttribute(this PropertyInfo property)
        {
            return property.GetCustomAttributes(typeof(OProperty), true).OfType<OProperty>().FirstOrDefault();

        }
        public static ClassProperty GetClassPropertyAttribute(this PropertyInfo property)
        {
            try
            {
                return property.GetCustomAttributes(typeof(ClassProperty), true).OfType<ClassProperty>().First();
            }
            catch
            {
                return null;
            }
        }

        public static string ToInvarianCultureString(this object value)
        {
            var formattable = value as IFormattable;
            if (value is float)
                return ((float)value).ToString("R", CultureInfo.InvariantCulture);
            else if (value is double)
                return ((double)value).ToString("R", CultureInfo.InvariantCulture);
            else
                return formattable?.ToString(null, CultureInfo.InvariantCulture) ?? value.ToString();
        }
    }
}
