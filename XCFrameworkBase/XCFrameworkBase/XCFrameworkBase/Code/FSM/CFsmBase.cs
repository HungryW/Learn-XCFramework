using System;

namespace XCFrameworkBase
{
    public abstract class CFsmBase
    {
        private string m_szName;

        public CFsmBase()
        {
            m_szName = string.Empty;
        }

        public abstract void Update(float elapseSed, float realElapseSed);
        public abstract void Shutdown();

        public void SetName(string a_szName)
        {
            m_szName = a_szName;
        }

        public string Name
        {
            get
            {
                return m_szName;
            }
        }

        public string FullName
        {
            get
            {
                return new TypeNamePair(OwnerType, Name).ToString();
            }
        }

        public abstract Type OwnerType
        {
            get;
        }

        public abstract int StateCount
        {
            get;
        }

        public abstract bool IsRunning
        {
            get;
        }

        public abstract bool IsDestoryed
        {
            get;
        }
    }
}
