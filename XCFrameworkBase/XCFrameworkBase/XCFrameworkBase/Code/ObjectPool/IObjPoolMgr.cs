using System;

namespace XCFrameworkBase
{
    public interface IObjPoolMgr
    {
        int Count
        {
            get;
        }

        bool HasObjectPool<T>() where T : CObjectBase;
        bool HasObjectPool(Type a_t);

        IObjectPool<T> GetObjectPool<T>() where T : CObjectBase;
        CObjectPoolBase GetObjectPool(Type a_t);

        IObjectPool<T> CreateSingleSpawnObjPool<T>(string a_szName) where T : CObjectBase;
        CObjectPoolBase CreateSingleSpawnObjPool(Type a_t, string a_szName);
        IObjectPool<T> CreateSingleSpawnObjPool<T>(string name, float autoReleaseInterval, int capacity, float expireTime, int priority) where T : CObjectBase;

        IObjectPool<T> CreateMultiSpawnObjPool<T>(string a_szName) where T : CObjectBase;
        CObjectPoolBase CreateMultiSpawnObjPool(Type a_t, string a_szName);
        IObjectPool<T> CreateMultiSpawnObjPool<T>(string name, float autoReleaseInterval, int capacity, float expireTime, int priority) where T : CObjectBase;


        bool DestroyObjPool<T>() where T : CObjectBase;
        bool DestroyObjPool(Type a_t);

        void Release();
        void ReleaseAllUnused();

    }
}
