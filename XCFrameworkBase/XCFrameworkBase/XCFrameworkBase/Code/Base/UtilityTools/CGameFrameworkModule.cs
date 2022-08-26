using System;

namespace XCFrameworkBase
{
    public abstract class CGameFrameworkModule
    {
        public virtual int Priority
        {
            get
            {
                return 0;
            }
        }

        public abstract void Update(float a_fElapseSed, float a_fRealElapseSed);
        public abstract void Shutdown();
    }
}
