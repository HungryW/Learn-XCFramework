using System;

namespace XCFrameworkBase
{
    public delegate void LoadAssetSuccessCallback(string a_szAssetName, object a_oAsset, float a_fDuration, object a_oUserData);
    public delegate void LoadAssetFailCallback(string a_szAssetName, ELoadResStatus a_eStatus, string a_szErrorMsg, object a_oUserData);
    public delegate void LoadAssetUpdateCallback(string a_szAssetName, float a_fProgress, object a_oUserData);
    public delegate void LoadAssetDependencyAssetCallback(string a_szAssetName, string a_szDependencyAssetName, int a_nLoadCount, int a_nTotalCount, object a_oUserData);


    public sealed class CLoadAssetCallbacks
    {
        private readonly LoadAssetSuccessCallback m_fnOnSuccess;
        private readonly LoadAssetFailCallback m_fnOnFail;
        private readonly LoadAssetUpdateCallback m_fnOnUpdate;
        private readonly LoadAssetDependencyAssetCallback m_fnOnLoadDependency;

        public CLoadAssetCallbacks(LoadAssetSuccessCallback a_fnOnSuccess, LoadAssetFailCallback a_fnOnFail, LoadAssetUpdateCallback a_fnOnUpdate, LoadAssetDependencyAssetCallback a_fnLoadDependendcy)
        {
            m_fnOnSuccess = a_fnOnSuccess;
            m_fnOnFail = a_fnOnFail;
            m_fnOnUpdate = a_fnOnUpdate;
            m_fnOnLoadDependency = a_fnLoadDependendcy;
        }

        public CLoadAssetCallbacks(LoadAssetSuccessCallback a_fnOnSuccess) : this(a_fnOnSuccess, null, null, null)
        {

        }

        public CLoadAssetCallbacks(LoadAssetSuccessCallback a_fnOnSuccess, LoadAssetFailCallback a_fnOnFail) : this(a_fnOnSuccess, a_fnOnFail, null, null)
        {

        }

        public CLoadAssetCallbacks(LoadAssetSuccessCallback a_fnOnSuccess, LoadAssetDependencyAssetCallback a_fnDependencyLoad) : this(a_fnOnSuccess, null, null, a_fnDependencyLoad)
        {

        }

        public LoadAssetSuccessCallback OnSuccess => m_fnOnSuccess;
        public LoadAssetFailCallback OnFail => m_fnOnFail;
        public LoadAssetUpdateCallback OnUpdate => m_fnOnUpdate;
        public LoadAssetDependencyAssetCallback OnDependencyLoad => m_fnOnLoadDependency;
    }
}
