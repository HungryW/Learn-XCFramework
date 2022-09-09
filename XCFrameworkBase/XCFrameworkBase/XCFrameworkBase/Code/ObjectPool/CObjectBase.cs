using System;

namespace XCFrameworkBase
{
    public abstract class CObjectBase : IReference
    {
        private string m_szName;
        private object m_oTarget;
        private bool m_bLocked;
        private int m_nPriority;
        private DateTime m_lastUseTime;

        public CObjectBase()
        {
            m_szName = null;
            m_oTarget = null;
            m_bLocked = false;
            m_nPriority = 0;
            m_lastUseTime = default(DateTime);
        }

        protected void Init(string a_szName, object a_oTarget)
        {
            Init(a_szName, a_oTarget, false, 0);
        }


        protected void Init(string a_szName, object a_oTarget, bool a_bLocked, int a_nPriority)
        {
            if (null == a_oTarget)
            {
                throw new GameFrameworkException(CUtility.Text.Format("Target {0} is invalid", a_szName));
            }

            m_szName = a_szName ?? string.Empty;
            m_oTarget = a_oTarget;
            m_bLocked = a_bLocked;
            m_nPriority = a_nPriority;
            m_lastUseTime = DateTime.UtcNow;
        }

        public virtual void Clear()
        {
            m_szName = null;
            m_oTarget = null;
            m_bLocked = false;
            m_nPriority = 0;
            m_lastUseTime = default(DateTime);
        }

        public virtual void OnSpawn()
        {

        }

        public virtual void OnUnSpawn()
        {

        }

        public void Release(bool a_isShutDown)
        {
            _Release(a_isShutDown);
            CReferencePool.Release(this);
        }

        protected abstract void _Release(bool a_isShutDown);

        public virtual bool CustomCanReleaseFlag()
        {
            return true;
        }



        public void SetLocked(bool a_bLock)
        {
            m_bLocked = a_bLock;
        }

        public void SetPriority(int a_nPriority)
        {
            m_nPriority = a_nPriority;
        }

        public void SetLastUseTime(DateTime a_time)
        {
            m_lastUseTime = a_time;
        }

        public string Name
        {
            get
            {
                return m_szName;
            }
        }

        public object Target
        {
            get
            {
                return m_oTarget;
            }
        }

        public bool Locked
        {
            get
            {
                return m_bLocked;
            }
        }

        public int Priority
        {
            get
            {
                return m_nPriority;
            }
        }

        public DateTime LastUseTime
        {
            get
            {
                return m_lastUseTime;
            }
        }
    }
}
