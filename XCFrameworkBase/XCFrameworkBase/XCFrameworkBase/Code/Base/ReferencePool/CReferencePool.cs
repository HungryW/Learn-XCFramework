using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    /// <summary>
    /// 维护一个全局map,保存所有引用池
    /// 
    /// 接口设计
    /// 设计泛型的接口,可以直接获得某一个类型的对象,不用再进行类型转换
    /// 
    /// 其他知识
    /// 反射的关键是 Type类型的对象,类的实例通过GetType()获得,类名通过typeof(T),typeof(ClassName)获得
    /// Type类型对象可以作为key值
    /// </summary>
    public static partial class CReferencePool
    {
        private static readonly Dictionary<Type, CReferenceCollection> ms_mapPools = new Dictionary<Type, CReferenceCollection>();

        public static int Count
        {
            get
            {
                return ms_mapPools.Count;
            }
        }

        public static void ClearAll()
        {
            lock (ms_mapPools)
            {
                foreach (var pool in ms_mapPools)
                {
                    pool.Value.RemoveAll();
                }
                ms_mapPools.Clear();
            }
        }

        public static T Acquire<T>() where T : class, IReference, new()
        {
            return _GetPool(typeof(T)).Acquire<T>();
        }

        public static IReference Acquire(Type a_t)
        {
            return _GetPool(a_t).Acquire();
        }

        public static void Release(IReference a_ref)
        {
            if (null == a_ref)
            {
                throw new Exception("a_ref is invalid");
            }
            _GetPool(a_ref.GetType()).Release(a_ref);
        }

        public static void Add<T>(int a_nCount) where T : class, IReference, new()
        {
            _GetPool(typeof(T)).Add<T>(a_nCount);
        }

        public static void RemoveAll<T>() where T : class, IReference, new()
        {
            _GetPool(typeof(T)).RemoveAll();
        }

        public static void RemoveAll(Type a_t)
        {
            _GetPool(a_t).RemoveAll();
        }

        private static CReferenceCollection _GetPool(Type a_t)
        {
            if (a_t == null)
            {
                throw new Exception("Type is invalid");
            }
            CReferenceCollection pool = null;
            lock (ms_mapPools)
            {
                if (!ms_mapPools.TryGetValue(a_t, out pool))
                {
                    pool = new CReferenceCollection(a_t);
                    ms_mapPools.Add(a_t, pool);
                }
            }
            return pool;
        }
    }
}
