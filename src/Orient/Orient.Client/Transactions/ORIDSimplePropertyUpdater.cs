using System.Collections.Generic;
using System.Reflection;

namespace Orient.Client.Transactions
{
    internal class OridSimplePropertyUpdater<TTarget> : OridPropertyUpdater<TTarget, Orid>
    {
        public OridSimplePropertyUpdater(PropertyInfo propertyInfo) : base(propertyInfo)
        {
            
        }

        public override void Update(object oTarget, Dictionary<Orid, Orid> mappings)
        {
            Orid orid = GetValue(oTarget);

            if (orid == Orid.Null)
                return;

            if (orid == null)
            {
                SetValue(oTarget, Orid.Null);
                return;
            }

            Orid replacement;
            if (mappings.TryGetValue(orid, out replacement))
                SetValue(oTarget, replacement);
        }
    }
}