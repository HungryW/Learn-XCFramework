using System;

namespace XCFrameworkBase
{
    public partial class CObjectPoolMgr
    {
        private sealed class CObject<T> : IReference where T : CObjectBase
        {
            private T m_Object;
            private int m_nSpawnCount;

            public CObject()
            {
                m_Object = null;
                m_nSpawnCount = 0;
            }

            public void Clear()
            {
                m_Object = null;
                m_nSpawnCount = 0;
            }

            public T Peek()
            {
                return m_Object;
            }

            public T Spawn()
            {
                m_nSpawnCount++;
                m_Object.SetLastUseTime(DateTime.UtcNow);
                m_Object.OnSpawn();
                return m_Object;
            }


            public void UnSpawn()
            {
                m_Object.OnUnSpawn();
                m_Object.SetLastUseTime(DateTime.UtcNow);
                m_nSpawnCount--;
                if (m_nSpawnCount < 0)
                {
                    throw new Exception(CUtility.Text.Format("Object {0} spawn count is less than 0", m_Object.Name));
                }
            }

            public void Release(bool a_bIsShutdown)
            {
                m_Object.Release(a_bIsShutdown);
                CReferencePool.Release(this);
            }

            public static CObject<T> Create(T obj, bool a_bSpawned)
            {
                if (null == obj)
                {
                    throw new Exception("Object is invalid");
                }

                CObject<T> internalObj = CReferencePool.Acquire<CObject<T>>();
                internalObj.m_Object = obj;
                internalObj.m_nSpawnCount = a_bSpawned ? 1 : 0;
                if (a_bSpawned)
                {
                    obj.OnSpawn();
                }
                return internalObj;
            }

            public void SetLocked(bool a_bLock)
            {
                m_Object.SetLocked(a_bLock);
            }

            public void SetPriority(int a_nPriority)
            {
                m_Object.SetPriority(a_nPriority);
            }

            public void SetLastUseTime(DateTime a_time)
            {
                m_Object.SetLastUseTime(a_time);
            }

            public string Name
            {
                get
                {
                    return m_Object.Name;
                }
            }

            public bool Locked
            {
                get
                {
                    return m_Object.Locked;
                }
            }

            public int Priority
            {
                get
                {
                    return m_Object.Priority;
                }
            }

            public DateTime LastUseTime
            {
                get
                {
                    return m_Object.LastUseTime;
                }
            }

            public bool IsInUse
            {
                get
                {
                    return m_nSpawnCount > 0;
                }
            }

            public int SpawnCount
            {
                get
                {
                    return m_nSpawnCount;
                }
            }

            public bool CustomCanReleaseFlag
            {
                get
                {
                    return m_Object.CustomCanReleaseFlag();
                }
            }

            public bool IsCanNotRelease()
            {
                return m_nSpawnCount > 0 || m_Object.Locked || m_Object.CustomCanReleaseFlag();
            }
        }
    }
}
