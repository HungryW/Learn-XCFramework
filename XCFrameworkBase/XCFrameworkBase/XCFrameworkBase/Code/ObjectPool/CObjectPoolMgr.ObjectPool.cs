using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public partial class CObjectPoolMgr
    {
        public delegate List<T> ReleaseObjectFilterCallback<T>(List<T> a_listRangeObjs, int a_nToReleastCount, DateTime a_timeExpire) where T : CObjectBase;
        private sealed class CObjectPool<T> : CObjectPoolBase, IObjectPool<T> where T : CObjectBase
        {
            private readonly LinkedList<CObject<T>> m_listObjs;
            private readonly bool m_bAllowMultiSpawn;
            private float m_fAutoReleaseInterval;
            private int m_nCapacity;
            private float m_fExpireTime;
            private int m_nPriority;
            private float m_fAutoReleaseTime;
            private ReleaseObjectFilterCallback<T> m_fnDefaultReleaseObjFilterCallback;

            private List<T> m_listTempCanReleaseObjs;
            private List<T> m_listTempToReleaseObjs;


            public CObjectPool(string a_szName, bool a_bAllowMultiSpawn, float a_fAutoReleaseInterval, int a_nCapacity, float a_nExpireTime, int a_nPriority)
                : base(a_szName)
            {
                m_listObjs = new LinkedList<CObject<T>>();
                m_listTempCanReleaseObjs = new List<T>();
                m_listTempToReleaseObjs = new List<T>();
                m_bAllowMultiSpawn = a_bAllowMultiSpawn;
                m_nCapacity = a_nCapacity;
                m_nPriority = a_nPriority;
                m_fAutoReleaseInterval = a_fAutoReleaseInterval;
                m_fAutoReleaseTime = 0f;
                m_fnDefaultReleaseObjFilterCallback = _DefaultReleaseObjFilter;
            }

            public void Register(T obj, bool a_bSpawned)
            {
                if (null == obj)
                {
                    return;
                }
                CObject<T> internalObj = CObject<T>.Create(obj, a_bSpawned);
                m_listObjs.AddLast(internalObj);
            }


            public bool CanSpawn()
            {
                return CanSpawn(string.Empty);
            }

            public bool CanSpawn(string a_szName)
            {
                foreach (var item in m_listObjs)
                {
                    if (item.Name != a_szName)
                    {
                        continue;
                    }
                    if (m_bAllowMultiSpawn || !item.IsInUse)
                    {
                        return true;
                    }
                }
                return false;
            }

            public T Spawn()
            {
                return Spawn(string.Empty);
            }

            public T Spawn(string a_szName)
            {
                foreach (var item in m_listObjs)
                {
                    if (item.Name != a_szName)
                    {
                        continue;
                    }
                    if (m_bAllowMultiSpawn || !item.IsInUse)
                    {
                        return item.Spawn();
                    }
                }
                return null;
            }


            public void UnSpawn(T obj)
            {
                UnSpawn(obj.Target);
            }

            public void UnSpawn(object a_oTarget)
            {
                CObject<T> internalObj = _GetInternalObj(a_oTarget);
                if (null != internalObj)
                {
                    internalObj.UnSpawn();
                    if (!internalObj.IsInUse)
                    {
                        Release();
                    }
                }
            }

            public void SetLocked(T obj, bool a_bLock)
            {
                if (null == obj)
                {
                    return;
                }

                SetLocked(obj.Target, a_bLock);
            }

            public void SetLocked(object target, bool a_bLock)
            {
                if (target == null)
                {
                    return;
                }
                CObject<T> internalObj = _GetInternalObj(target);
                if (null != internalObj)
                {
                    internalObj.SetLocked(a_bLock);
                }
            }

            public void SetPriority(T obj, int a_nPriority)
            {
                if (null != obj)
                {
                    return;
                }
                SetPriority(obj.Target, a_nPriority);
            }

            public void SetPriority(object target, int a_nPriority)
            {
                if (null == target)
                {
                    return;
                }
                CObject<T> internalObj = _GetInternalObj(target);
                if (null != internalObj)
                {
                    internalObj.SetPriority(a_nPriority);
                }
            }

            public bool ReleaseObject(T obj)
            {
                if (null == obj)
                {
                    return false;
                }
                return ReleaseObject(obj.Target);
            }

            public bool ReleaseObject(object target)
            {
                if (null == target)
                {
                    return false;
                }

                CObject<T> internalObj = _GetInternalObj(target);
                if (null == internalObj)
                {
                    return false;
                }
                if (internalObj.IsCanNotRelease())
                {
                    return false;
                }

                m_listObjs.Remove(internalObj);
                internalObj.Release(false);
                return true;
            }


            public override void Release()
            {
                Release(m_listObjs.Count - m_nCapacity, m_fnDefaultReleaseObjFilterCallback);
            }


            public override void Release(int a_nCount)
            {
                Release(a_nCount, m_fnDefaultReleaseObjFilterCallback);
            }

            public override void ReleaseAllUnused()
            {
                m_fAutoReleaseTime = 0;
                _GetCanReleaseObjs(m_listTempCanReleaseObjs);
                foreach (var obj in m_listTempCanReleaseObjs)
                {
                    ReleaseObject(obj);
                }
            }

            public void Release(int a_nToReleaseCount, ReleaseObjectFilterCallback<T> a_fnReleaseObjFilterCallback)
            {
                if (a_nToReleaseCount < 0)
                {
                    a_nToReleaseCount = 0;
                }

                DateTime expireTime = DateTime.MinValue;
                if (m_fExpireTime < float.MaxValue)
                {
                    expireTime = DateTime.UtcNow.AddSeconds(-m_fExpireTime);
                }

                m_fAutoReleaseTime = 0f;
                _GetCanReleaseObjs(m_listTempCanReleaseObjs);
                List<T> listToReleaseObjs = a_fnReleaseObjFilterCallback(m_listTempCanReleaseObjs, a_nToReleaseCount, expireTime);
                if (listToReleaseObjs.Count <= 0)
                {
                    return;
                }

                foreach (var obj in listToReleaseObjs)
                {
                    ReleaseObject(obj);
                }
            }

            private CObject<T> _GetInternalObj(object a_oTarget)
            {
                foreach (var item in m_listObjs)
                {
                    if (item.Peek().Target == a_oTarget)
                    {
                        return item;
                    }
                }
                return null;
            }

            private void _GetCanReleaseObjs(List<T> a_outObj)
            {
                a_outObj.Clear();
                foreach (var item in m_listObjs)
                {
                    if (item.IsCanNotRelease())
                    {
                        continue;
                    }
                    a_outObj.Add(item.Peek());
                }
            }

            private List<T> _DefaultReleaseObjFilter(List<T> a_listRangeObjs, int a_nToReleaseCount, DateTime a_timeExpire)
            {
                m_listTempToReleaseObjs.Clear();
                if (a_timeExpire > DateTime.MinValue)
                {
                    for (int i = a_listRangeObjs.Count - 1; i >= 0; i--)
                    {
                        if (a_listRangeObjs[i].LastUseTime <= a_timeExpire)
                        {
                            m_listTempToReleaseObjs.Add(a_listRangeObjs[i]);
                            a_listRangeObjs.RemoveAt(i);
                        }
                    }
                }


                for (int i = 0; i < a_listRangeObjs.Count; i++)
                {
                    if (m_listTempToReleaseObjs.Count >= a_nToReleaseCount)
                    {
                        break;
                    }
                    for (int j = i + 1; j < a_listRangeObjs.Count; j++)
                    {
                        if (a_listRangeObjs[i].Priority > a_listRangeObjs[j].Priority
                            || a_listRangeObjs[i].Priority == a_listRangeObjs[i].Priority && a_listRangeObjs[i].LastUseTime > a_listRangeObjs[j].LastUseTime)
                        {
                            T temp = a_listRangeObjs[i];
                            a_listRangeObjs[i] = a_listRangeObjs[j];
                            a_listRangeObjs[j] = temp;
                        }
                    }

                    m_listTempToReleaseObjs.Add(a_listRangeObjs[i]);
                }

                return m_listTempToReleaseObjs;
            }


            public override void Update(float a_fElapseSed, float a_fRealElapseSed)
            {
                m_fAutoReleaseTime += a_fRealElapseSed;
                if (m_fAutoReleaseTime < m_fAutoReleaseInterval)
                {
                    return;
                }
                Release();
            }

            public override void Shutdown()
            {
                foreach (var item in m_listObjs)
                {
                    item.Release(true);
                }
                m_listObjs.Clear();
                m_listTempCanReleaseObjs.Clear();
                m_listTempToReleaseObjs.Clear();
            }

            public string FullName => new TypeNamePair(ObjectType, Name).ToString();

            public override int CanReleaseCount
            {
                get
                {
                    _GetCanReleaseObjs(m_listTempCanReleaseObjs);
                    return m_listTempCanReleaseObjs.Count;
                }
            }

            public override Type ObjectType => typeof(T);

            public override int Count => m_listObjs.Count;

            public override bool AllowMultiSpawn => m_bAllowMultiSpawn;

            public override int Priority { get => m_nPriority; set => m_nPriority = value; }
            public override int Capacity { get => m_nCapacity; set => m_nCapacity = value; }
            public override float AutoReleaseInterval { get => m_fAutoReleaseInterval; set => m_fAutoReleaseInterval = value; }
            public override float ExpireTime { get => m_fExpireTime; set => m_fExpireTime = value; }
        }
    }
}
