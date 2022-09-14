using System;

namespace XCFrameworkBase
{
    public delegate void LoadSceneSuccessCallback(string a_szSceneAssetName,  float a_fDuration, object a_oUserData);
    public delegate void LoadSceneFailCallback(string a_szSceneAssetName, ELoadResStatus a_eStatus, string a_szErrorMsg, object a_oUserData);
    public delegate void LoadSceneUpdateCallback(string a_szSceneAssetName, float a_fProgress, object a_oUserData);
    public delegate void LoadSceneDependencyAssetCallback(string a_szSceneAssetName, string a_szDependencyAssetName, int a_nLoadCount, int a_nTotalCount, object a_oUserData);


    public sealed class CLoadSceneCallbacks
    {
        private readonly LoadSceneSuccessCallback m_fnOnSuccess;
        private readonly LoadSceneFailCallback m_fnOnFail;
        private readonly LoadSceneUpdateCallback m_fnOnUpdate;
        private readonly LoadSceneDependencyAssetCallback m_fnOnLoadDependency;

        public CLoadSceneCallbacks(LoadSceneSuccessCallback a_fnOnSuccess, LoadSceneFailCallback a_fnOnFail, LoadSceneUpdateCallback a_fnOnUpdate, LoadSceneDependencyAssetCallback a_fnLoadDependendcy)
        {
            m_fnOnSuccess = a_fnOnSuccess;
            m_fnOnFail = a_fnOnFail;
            m_fnOnUpdate = a_fnOnUpdate;
            m_fnOnLoadDependency = a_fnLoadDependendcy;
        }

        public CLoadSceneCallbacks(LoadSceneSuccessCallback a_fnOnSuccess) : this(a_fnOnSuccess, null, null, null)
        {

        }

        public CLoadSceneCallbacks(LoadSceneSuccessCallback a_fnOnSuccess, LoadSceneFailCallback a_fnOnFail) : this(a_fnOnSuccess, a_fnOnFail, null, null)
        {

        }

        public CLoadSceneCallbacks(LoadSceneSuccessCallback a_fnOnSuccess, LoadSceneDependencyAssetCallback a_fnDependencyLoad) : this(a_fnOnSuccess, null, null, a_fnDependencyLoad)
        {

        }

        public LoadSceneSuccessCallback OnSuccess => m_fnOnSuccess;
        public LoadSceneFailCallback OnFail => m_fnOnFail;
        public LoadSceneUpdateCallback OnUpdate => m_fnOnUpdate;
        public LoadSceneDependencyAssetCallback OnDependencyLoad => m_fnOnLoadDependency;
    }
}
