using System;
using System.Collections.Generic;
using System.Reflection;
using Orient.Client.Mapping;

namespace Orient.Client.Transactions
{
    internal interface IOridPropertyUpdater
    {
        void Update(object oTarget, Dictionary<Orid, Orid> mappings);
    }

    internal abstract class OridPropertyUpdater<TTarget, T> : IOridPropertyUpdater
    {
        private readonly PropertyInfo _propertyInfo;
        private Action<TTarget, T> _setter;
        private Func<TTarget, T> _getter;

        protected OridPropertyUpdater(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
            if (propertyInfo != null)
            {
                _getter = FastPropertyAccessor.BuildTypedGetter<TTarget, T>(propertyInfo);
                if (_propertyInfo.CanWrite)
                    _setter = FastPropertyAccessor.BuildTypedSetter<TTarget, T>(propertyInfo);
            }
        }

        protected T GetValue(object oTarget)
        {
            return _getter((TTarget) oTarget);
        }

        protected void SetValue(object oTarget, T value)
        {
            _setter((TTarget) oTarget, value);
        }

        public abstract void Update(object oTarget, Dictionary<Orid, Orid> mappings);
    }
}