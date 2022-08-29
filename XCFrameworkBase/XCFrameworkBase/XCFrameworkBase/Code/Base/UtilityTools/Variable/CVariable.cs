using System;

namespace XCFrameworkBase
{
    public abstract class CVariable : IReference
    {
        public CVariable()
        {

        }

        public abstract Type Type
        {
            get;
        }

        public abstract object GetValue();
        public abstract void SetValue(object a_val);
        public abstract void Clear();
    }
}
