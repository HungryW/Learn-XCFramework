using System;

namespace XCFrameworkBase
{
    public interface IWebRequestAgentHelper
    {
        event EventHandler<CWebRequestAgentHelperCompleteEventArgs> OnHelperRequestComplete;
        event EventHandler<CWebRequestAgentHelperErrorEventArgs> OnHelperRequestFail;

        void Request(string a_szWebRequestUri, object a_oUserData);
        void Request(string a_szWebRequsetUri, byte[] postData, object a_oUserData);

        void Reset();
    }

    public sealed class CWebRequestAgentHelperCompleteEventArgs : CFrameWorkEventArgs
    {
        private byte[] m_arrWebResponseData;

        public CWebRequestAgentHelperCompleteEventArgs()
        {
            m_arrWebResponseData = null;
        }

        public override void Clear()
        {
            m_arrWebResponseData = null;
        }

        public byte[] GetWebResponseData()
        {
            return m_arrWebResponseData;
        }

        public static CWebRequestAgentHelperCompleteEventArgs Create(byte[] a_arrResponseData)
        {
            CWebRequestAgentHelperCompleteEventArgs arg = CReferencePool.Acquire<CWebRequestAgentHelperCompleteEventArgs>();
            arg.m_arrWebResponseData = a_arrResponseData;
            return arg;
        }
    }


    public sealed class CWebRequestAgentHelperErrorEventArgs : CFrameWorkEventArgs
    {
        public string ErrorMessage
        {
            get;
            private set;
        }

        public CWebRequestAgentHelperErrorEventArgs()
        {
            ErrorMessage = null;
        }

        public override void Clear()
        {
            ErrorMessage = null;
        }

        public static CWebRequestAgentHelperErrorEventArgs Create(string a_szErrorMsg)
        {
            CWebRequestAgentHelperErrorEventArgs arg = CReferencePool.Acquire<CWebRequestAgentHelperErrorEventArgs>();
            arg.ErrorMessage = a_szErrorMsg;
            return arg;
        }
    }
}
