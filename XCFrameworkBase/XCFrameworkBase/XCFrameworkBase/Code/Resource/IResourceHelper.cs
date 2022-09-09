using System;

namespace XCFrameworkBase
{
    public interface IResourceHelper
    {

        void LoadBytes(string a_szFileUri, CLoadBytesCallbacks a_callbacks, object a_oUserData);

        void UnloadScene(string a_szSceneAssetName, CUnloadSceneCallbacks a_callbacks, object a_oUserData);

        void Release(object a_oObjToRelease);
    }






    public sealed class CLoadBytesCallbacks
    {
        public delegate void LoadBytesSuccessCallback(string a_szFileUri, byte[] a_arrBytes, float a_fDuration, object a_oUserdata);
        public delegate void LoadBytesFailCallback(string a_szFileUri, string a_szErrorMsg, object a_oUserData);

        private readonly LoadBytesSuccessCallback m_fnOnSuccess;
        private readonly LoadBytesFailCallback m_fnFail;

        public CLoadBytesCallbacks(LoadBytesSuccessCallback a_fnOnSuccess, LoadBytesFailCallback a_fnOnFail)
        {
            m_fnOnSuccess = a_fnOnSuccess;
            m_fnFail = a_fnOnFail;
        }

        public CLoadBytesCallbacks(LoadBytesSuccessCallback a_fnOnSuccess)
            : this(a_fnOnSuccess, null)
        {

        }

        public LoadBytesSuccessCallback SuccessCallback
        {
            get
            {
                return m_fnOnSuccess;
            }
        }

        public LoadBytesFailCallback FailCallback
        {
            get
            {
                return m_fnFail;
            }
        }
    }





    public sealed class CUnloadSceneCallbacks
    {
        public delegate void UnloadSceneSuccessCallback(string a_szSceneAssetName, object a_oUserData);
        public delegate void UnloadSceneFailCallback(string a_szSceneAssetName, object a_oUserData);

        private readonly UnloadSceneSuccessCallback m_fnOnSuccess;
        private readonly UnloadSceneFailCallback m_fnFail;

        public CUnloadSceneCallbacks(UnloadSceneSuccessCallback a_fnOnSuccess, UnloadSceneFailCallback a_fnOnFail)
        {
            m_fnOnSuccess = a_fnOnSuccess;
            m_fnFail = a_fnOnFail;
        }

        public CUnloadSceneCallbacks(UnloadSceneSuccessCallback a_fnSuccess)
            : this(a_fnSuccess, null)
        {

        }

        public UnloadSceneSuccessCallback OnUnloadSuccess
        {
            get
            {
                return m_fnOnSuccess;
            }
        }

        public UnloadSceneFailCallback OnUnloadFail
        {
            get
            {
                return m_fnFail;
            }
        }
    }
}
