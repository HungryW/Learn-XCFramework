using System;

namespace XCFrameworkBase
{
    public abstract class CObjectPoolBase
    {
        private readonly string m_szName;

        public CObjectPoolBase(string a_szName)
        {
            m_szName = a_szName ?? string.Empty;
        }

        public string Name
        {
            get
            {
                return m_szName;
            }
        }

        public abstract Type ObjectType { get; }
        public abstract int Count { get; }
        public abstract int CanReleaseCount { get; }
        public abstract bool AllowMultiSpawn { get; }


        public abstract int Priority { get; set; }
        public abstract int Capacity { get; set; }
        public abstract float AutoReleaseInterval { get; set; }
        public abstract float ExpireTime { get; set; }

        public abstract void Release();
        public abstract void Release(int a_nCount);
        public abstract void ReleaseAllUnused();

        public abstract void Update(float a_fElapseSed, float a_fRealElapseSed);
        public abstract void Shutdown();
    }
}
