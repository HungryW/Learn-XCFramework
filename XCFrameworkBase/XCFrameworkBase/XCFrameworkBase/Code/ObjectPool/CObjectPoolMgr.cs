using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    /// <summary>
    /// 
    /// 知识点
    /// typeof(CObjectPool<>).MakeGenericType(a_tObjType); 获得实际泛型类的类型变量
    /// Activator.CreateInstance(type, 构造函数参数列表),可以调用有参的构造函数 
    /// </summary>
    public partial class CObjectPoolMgr : CGameFrameworkModule, IObjPoolMgr
    {
        private const int mc_nDefaultCapacity = int.MaxValue;
        private const float mc_fDefaultExpireTime = float.MaxValue;
        private const int mc_nDefaultPriority = 0;

        private Dictionary<TypeNamePair, CObjectPoolBase> m_mapPools;

        public CObjectPoolMgr()
        {
            m_mapPools = new Dictionary<TypeNamePair, CObjectPoolBase>();
        }

        public override int Priority => 6;

        public int Count => m_mapPools.Count;

        public override void Update(float a_fElapseSed, float a_fRealElapseSed)
        {
            foreach (var item in m_mapPools)
            {
                item.Value.Update(a_fElapseSed, a_fRealElapseSed);
            }
        }

        public override void Shutdown()
        {
            foreach (var item in m_mapPools)
            {
                item.Value.Shutdown();
            }
            m_mapPools.Clear();
        }

        public bool HasObjectPool<T>() where T : CObjectBase
        {
            return _HasObject(new TypeNamePair(typeof(T)));
        }

        public bool HasObjectPool(Type a_t)
        {
            return _HasObject(new TypeNamePair(a_t));
        }

        private bool _HasObject(TypeNamePair a_tName)
        {
            return m_mapPools.ContainsKey(a_tName);
        }

        public IObjectPool<T> GetObjectPool<T>() where T : CObjectBase
        {
            return (IObjectPool<T>)_GetPool(new TypeNamePair(typeof(T)));
        }

        public CObjectPoolBase GetObjectPool(Type a_t)
        {
            return _GetPool(new TypeNamePair(a_t));
        }

        private CObjectPoolBase _GetPool(TypeNamePair a_tName)
        {
            CObjectPoolBase pool = null;
            m_mapPools.TryGetValue(a_tName, out pool);
            return pool;
        }


        public IObjectPool<T> CreateMultiSpawnObjPool<T>(string a_szName) where T : CObjectBase
        {
            return _CreatePool<T>(a_szName, true, mc_fDefaultExpireTime, mc_nDefaultCapacity, mc_fDefaultExpireTime, mc_nDefaultPriority);
        }

        public CObjectPoolBase CreateMultiSpawnObjPool(Type a_t, string a_szName)
        {
            return _CreatePool(a_t, a_szName, true, mc_fDefaultExpireTime, mc_nDefaultCapacity, mc_fDefaultExpireTime, mc_nDefaultPriority);
        }

        public IObjectPool<T> CreateMultiSpawnObjPool<T>(string name, float autoReleaseInterval, int capacity, float expireTime, int priority) where T : CObjectBase
        {
            return _CreatePool<T>(name, true, autoReleaseInterval, capacity, expireTime, priority);
        }

        public IObjectPool<T> CreateSingleSpawnObjPool<T>(string a_szName) where T : CObjectBase
        {
            return _CreatePool<T>(a_szName, false, mc_fDefaultExpireTime, mc_nDefaultCapacity, mc_fDefaultExpireTime, mc_nDefaultPriority);
        }

        public CObjectPoolBase CreateSingleSpawnObjPool(Type a_t, string a_szName)
        {
            return _CreatePool(a_t, a_szName, false, mc_fDefaultExpireTime, mc_nDefaultCapacity, mc_fDefaultExpireTime, mc_nDefaultPriority);
        }

        public IObjectPool<T> CreateSingleSpawnObjPool<T>(string name, float autoReleaseInterval, int capacity, float expireTime, int priority) where T : CObjectBase
        {
            return _CreatePool<T>(name, false, autoReleaseInterval, capacity, expireTime, priority);
        }

        private IObjectPool<T> _CreatePool<T>(string a_szName, bool a_bAllowMultiSpawn, float autoReleaseInterval, int capacity, float expireTime, int priority) where T : CObjectBase
        {
            TypeNamePair tName = new TypeNamePair(typeof(T), a_szName);
            if (_HasObject(tName))
            {
                return null;
            }
            CObjectPool<T> pool = new CObjectPool<T>(a_szName, a_bAllowMultiSpawn, autoReleaseInterval, capacity, expireTime, priority);
            m_mapPools.Add(tName, pool);
            return pool;
        }

        private CObjectPoolBase _CreatePool(Type a_tObjType, string a_szName, bool a_bAllowMultiSpawn, float autoReleaseInterval, int capacity, float expireTime, int priority)
        {
            TypeNamePair tName = new TypeNamePair(a_tObjType, a_szName);
            if (_HasObject(tName))
            {
                return null;
            }
            Type tPoolType = typeof(CObjectPool<>).MakeGenericType(a_tObjType);
            CObjectPoolBase pool = (CObjectPoolBase)Activator.CreateInstance(tPoolType, a_szName, a_bAllowMultiSpawn, autoReleaseInterval, capacity, expireTime, priority);
            m_mapPools.Add(tName, pool);
            return pool;
        }

        public bool DestroyObjPool<T>() where T : CObjectBase
        {
            return _DestroyPool(new TypeNamePair(typeof(T)));
        }

        public bool DestroyObjPool(Type a_t)
        {
            return _DestroyPool(new TypeNamePair(a_t));
        }

        private bool _DestroyPool(TypeNamePair a_tName)
        {
            CObjectPoolBase objPool = null;
            if (m_mapPools.TryGetValue(a_tName, out objPool))
            {
                objPool.Shutdown();
                return m_mapPools.Remove(a_tName);
            }

            return false;
        }


        public void Release()
        {
            foreach (var item in m_mapPools)
            {
                item.Value.Release();
            }
        }

        public void ReleaseAllUnused()
        {
            foreach (var item in m_mapPools)
            {
                item.Value.ReleaseAllUnused();
            }
        }
    }
}
