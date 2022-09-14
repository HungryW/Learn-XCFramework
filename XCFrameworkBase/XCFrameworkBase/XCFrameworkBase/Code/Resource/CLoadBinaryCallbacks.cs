using System;

namespace XCFrameworkBase
{
    public delegate void LoadBinarySuccessCallback(string a_szBinaryAssetName, byte[] a_arrBinaryBytes, float a_fDuration, object a_oUserData);
    public delegate void LoadBinaryFailCallback(string a_szBinaryAssetName, ELoadResStatus a_eStatus, string a_szErrorMsg, object a_oUserData);


    public sealed class CLoadBinaryCallbacks
    {
        private readonly LoadBinarySuccessCallback m_fnOnSuccess;
        private readonly LoadBinaryFailCallback m_fnOnFail;

        public CLoadBinaryCallbacks(LoadBinarySuccessCallback a_fnOnSuccess, LoadBinaryFailCallback a_fnOnFail)
        {
            m_fnOnSuccess = a_fnOnSuccess;
            m_fnOnFail = a_fnOnFail;
        }

        public CLoadBinaryCallbacks(LoadBinarySuccessCallback a_fnOnSuccess) : this(a_fnOnSuccess, null)
        {

        }

        public LoadBinarySuccessCallback OnSuccess => m_fnOnSuccess;
        public LoadBinaryFailCallback OnFail => m_fnOnFail;
    }
}
