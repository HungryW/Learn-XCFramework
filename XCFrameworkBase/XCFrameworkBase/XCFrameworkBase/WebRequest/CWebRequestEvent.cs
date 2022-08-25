using System;

namespace XCFrameworkBase
{
    public sealed class CWebRequestStartEventArgs : CFrameWorkEventArgs
    {
        public int nId
        {
            get;
            private set;
        }

        public string WebRequestUri
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public CWebRequestStartEventArgs()
        {
            nId = 0;
            WebRequestUri = null;
            UserData = null;
        }

        public override void Clear()
        {
            nId = 0;
            WebRequestUri = null;
            UserData = null;
        }

        public static CWebRequestStartEventArgs Create(int a_nId, string a_szRequestUri, object a_oUserData)
        {
            CWebRequestStartEventArgs arg = CReferencePool.Acquire<CWebRequestStartEventArgs>();
            arg.nId = a_nId;
            arg.WebRequestUri = a_szRequestUri;
            arg.UserData = a_oUserData;

            return arg;
        }
    }


    public sealed class CWebRequestSuccessEventArgs : CFrameWorkEventArgs
    {
        private byte[] m_arrWebResponseData;

        public int nId
        {
            get;
            private set;
        }

        public string WebRequestUri
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public byte[] GetWebResponseData()
        {
            return m_arrWebResponseData;
        }

        public CWebRequestSuccessEventArgs()
        {
            nId = 0;
            WebRequestUri = null;
            UserData = null;
            m_arrWebResponseData = null;
        }

        public override void Clear()
        {
            nId = 0;
            WebRequestUri = null;
            UserData = null;
            m_arrWebResponseData = null;
        }

        public static CWebRequestSuccessEventArgs Create(int a_nId, string a_szRequestUri, object a_oUserData, byte[] a_arrResponseData)
        {
            CWebRequestSuccessEventArgs arg = CReferencePool.Acquire<CWebRequestSuccessEventArgs>();
            arg.nId = a_nId;
            arg.WebRequestUri = a_szRequestUri;
            arg.UserData = a_oUserData;
            arg.m_arrWebResponseData = a_arrResponseData;

            return arg;
        }
    }

    public sealed class CWebRequestFailEventArgs : CFrameWorkEventArgs
    {
        public int nId
        {
            get;
            private set;
        }

        public string WebRequestUri
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public string szErrormMsg
        {
            get;
            private set;
        }

        public CWebRequestFailEventArgs()
        {
            nId = 0;
            WebRequestUri = null;
            UserData = null;
            szErrormMsg = null;
        }

        public override void Clear()
        {
            nId = 0;
            WebRequestUri = null;
            UserData = null;
            szErrormMsg = null;
        }

        public static CWebRequestFailEventArgs Create(int a_nId, string a_szRequestUri, object a_oUserData, string a_szErrorMsg)
        {
            CWebRequestFailEventArgs arg = CReferencePool.Acquire<CWebRequestFailEventArgs>();
            arg.nId = a_nId;
            arg.WebRequestUri = a_szRequestUri;
            arg.UserData = a_oUserData;
            arg.szErrormMsg = a_szErrorMsg;

            return arg;
        }
    }
}
