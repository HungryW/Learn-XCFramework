using System;

namespace XCFrameworkBase
{
    public interface IDownloadAgentHelper
    {
        event EventHandler<CDownloadAgentHelperUpdateBytesEventArgs> EventUpdateBytes;
        event EventHandler<CDownloadAgentHelperUpdateLenEventArgs> EventUpdateLen;
        event EventHandler<CDownloadAgentHelperCompleteEventArgs> EventComplete;
        event EventHandler<CDownloadAgentHelperErrorEventArgs> EventFail;

        void Download(string a_szDownloadUri, object a_oUserData);
        void Download(string a_szDownloadUri, long a_lStartPos, object a_oUserData);
        void Download(string a_szDownloadUri, long a_lStartPos, long a_lEndPos, object a_oUserData);

        void Reset();
    }

    public sealed class CDownloadAgentHelperUpdateBytesEventArgs : CFrameWorkEventArgs
    {
        private byte[] m_arrBytes;

        public int Offset
        {
            get;
            private set;
        }

        public int Len
        {
            get;
            private set;
        }

        public byte[] GetBytes()
        {
            return m_arrBytes;
        }

        public CDownloadAgentHelperUpdateBytesEventArgs()
        {
            m_arrBytes = null;
            Offset = 0;
            Len = 0;
        }

        public override void Clear()
        {
            m_arrBytes = null;
            Offset = 0;
            Len = 0;
        }

        public static CDownloadAgentHelperUpdateBytesEventArgs Create(byte[] a_arrbytes, int a_nOffset, int a_nLen)
        {
            if (a_arrbytes == null)
            {
                throw new Exception("Bytes is invalid");
            }
            if (a_nOffset < 0 || a_nOffset >= a_arrbytes.Length)
            {
                throw new Exception("Bytes is invalid");
            }

            if (a_nLen < 0 || a_nOffset + a_nLen >= a_arrbytes.Length)
            {
                throw new Exception("Bytes is invalid");
            }
            CDownloadAgentHelperUpdateBytesEventArgs arg = CReferencePool.Acquire<CDownloadAgentHelperUpdateBytesEventArgs>();
            arg.Len = a_nLen;
            arg.Offset = a_nOffset;
            arg.m_arrBytes = a_arrbytes;
            return arg;
        }
    }

    public sealed class CDownloadAgentHelperUpdateLenEventArgs : CFrameWorkEventArgs
    {
        public int DeltaLen
        {
            get;
            private set;
        }

        public CDownloadAgentHelperUpdateLenEventArgs()
        {
            DeltaLen = 0;
        }

        public override void Clear()
        {
            DeltaLen = 0;
        }

        public static CDownloadAgentHelperUpdateLenEventArgs Create(int deltaLength)
        {
            if (deltaLength <= 0)
            {
                throw new GameFrameworkException("Delta length is invalid.");
            }
            CDownloadAgentHelperUpdateLenEventArgs arg = CReferencePool.Acquire<CDownloadAgentHelperUpdateLenEventArgs>();
            arg.DeltaLen = deltaLength;
            return arg;
        }
    }

    public sealed class CDownloadAgentHelperCompleteEventArgs : CFrameWorkEventArgs
    {
        public long Len
        {
            get;
            private set;
        }

        public CDownloadAgentHelperCompleteEventArgs()
        {
            Len = 0;
        }

        public override void Clear()
        {
            Len = 0;
        }

        public static CDownloadAgentHelperCompleteEventArgs Create(int len)
        {
            if (len <= 0)
            {
                throw new GameFrameworkException(" length is invalid.");
            }
            CDownloadAgentHelperCompleteEventArgs arg = CReferencePool.Acquire<CDownloadAgentHelperCompleteEventArgs>();
            arg.Len = len;
            return arg;
        }
    }

    public sealed class CDownloadAgentHelperErrorEventArgs : CFrameWorkEventArgs
    {
        public bool DeleteDownloading
        {
            get;
            private set;
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        public CDownloadAgentHelperErrorEventArgs()
        {
            DeleteDownloading = false;
            ErrorMessage = null;
        }

        public override void Clear()
        {
            DeleteDownloading = false;
            ErrorMessage = null;
        }

        public static CDownloadAgentHelperErrorEventArgs Create(bool deleteDownloading, string errorMessage)
        {
            CDownloadAgentHelperErrorEventArgs arg = CReferencePool.Acquire<CDownloadAgentHelperErrorEventArgs>();
            arg.DeleteDownloading = deleteDownloading;
            arg.ErrorMessage = errorMessage;
            return arg;
        }
    }

}
