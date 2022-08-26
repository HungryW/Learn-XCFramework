using System;

namespace XCFrameworkBase
{
    public interface IObjectPool<T> where T : CObjectBase
    {
        void Register(T obj, bool spawned);

        bool CanSpawn();
        bool CanSpawn(string a_szName);

        T Spawn();
        T Spawn(string a_szName);

        void UnSpawn(T obj);
        void UnSpawn(object obj);

        void SetLocked(object obj, bool a_bLock);
        void SetPriority(object obj, int a_nPriority);

        bool ReleaseObject(object target);
        void Release(int a_nCount);
        void ReleaseAllUnused();

        string Name { get; }
        string FullName { get; }
        Type ObjectType { get; }
        int Count { get; }
        int CanReleaseCount { get; }
        bool AllowMultiSpawn { get; }

        int Priority { get; set; }
        int Capacity { get; set; }

        float AutoReleaseInterval { get; set; }
        float ExpireTime { get; set; }
    }
}
