using System.Collections.Generic;
using System.Reflection;

namespace Orient.Client.Transactions
{
    internal class OridArrayPropertyUpdater<TTarget> : OridPropertyUpdater<TTarget, Orid[]>
    {
        public OridArrayPropertyUpdater(PropertyInfo propertyInfo) : base(propertyInfo)
        {
        }

        public override void Update(object oTarget, Dictionary<Orid, Orid> mappings)
        {
            Orid[] Orids = GetValue(oTarget);
            if (Orids == null)
                return;
            for (int i = 0; i < Orids.Length; i++)
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