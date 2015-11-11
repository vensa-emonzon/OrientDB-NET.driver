using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orient.Client.Transactions
{
    internal class OridHashSetUpdater<TTarget> : OridPropertyUpdater<TTarget, HashSet<Orid>> 
    {
        public OridHashSetUpdater(PropertyInfo propertyInfo) : base(propertyInfo)
        {
        }

        public override void Update(object oTarget, Dictionary<Orid, Orid> mappings)
        {
            var Orids = GetValue(oTarget);
            if (Orids == null)
                return;
            foreach (var Orid in Orids.ToList())
            {
                Orid replacement;
                if (mappings.TryGetValue(Orid, out replacement))
                {
                    Orids.Remove(Orid);
                    Orids.Add(replacement);
                }
            }
        }
    }
}