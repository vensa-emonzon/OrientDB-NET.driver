using System.Collections.Generic;
using System.Reflection;

namespace Orient.Client.Transactions
{
    internal class OridListPropertyUpdater<TTarget> : OridPropertyUpdater<TTarget, List<Orid>> 
    {
        public OridListPropertyUpdater(PropertyInfo propertyInfo) : base(propertyInfo)
        {
        }

        public override void Update(object oTarget, Dictionary<Orid, Orid> mappings)
        {
            var Orids = GetValue(oTarget);
            if (Orids == null)
                return;
            for (int i = 0; i < Orids.Count; i++)
            {
                Orid replacement;
                if (mappings.TryGetValue(Orids[i], out replacement))
                {
                    Orids[i] = replacement;
                }
            }
        }
    }
}