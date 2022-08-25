using System;

namespace XCFrameworkBase
{
    public sealed class CDownloadStartEventArgs : CFrameWorkEventArgs
    {
        public int nId
        {
            get;
            private set;
        }

        public string DownloadPath
        {
            get;
            private set;
        }

        public string DownloadUri
        {
            get;
            private set;
        }

        public long CurrentLen
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public override void Clear()
        {
            nId = 0;
            DownloadPath = null;
            DownloadUri = null;
            CurrentLen = 0L;
            UserData = null;
        }

        public static CDownloadStartEventArgs Create(int serialId, string downloadPath, string downloadUri, long currentLength, object userData)
        {
            CDownloadStartEventArgs arg = CReferencePool.Acquire<CDownloadStartEventArgs>();
            arg.nId = serialId;
            arg.DownloadPath = downloadPath;
            arg.DownloadUri = downloadUri;
            arg.CurrentLen = currentLength;
            arg.UserData = userData;
            return arg;
        }
    }

    public sealed class CDownloadUpadateEventArgs : CFrameWorkEventArgs
    {
        public int nId
        {
            get;
            private set;
        }

        public string DownloadPath
        {
            get;
            private set;
        }

        public string DownloadUri
        {
            get;
            private set;
        }

        public long CurrentLen
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public override void Clear()
        {
            nId = 0;
            DownloadPath = null;
            DownloadUri = null;
            CurrentLen = 0L;
            UserData = null;
        }

        public static CDownloadUpadateEventArgs Create(int serialId, string downloadPath, string downloadUri, long currentLength, object userData)
        {
            CDownloadUpadateEventArgs arg = CReferencePool.Acquire<CDownloadUpadateEventArgs>();
            arg.nId = serialId;
            arg.DownloadPath = downloadPath;
            arg.DownloadUri = downloadUri;
            arg.CurrentLen = currentLength;
            arg.UserData = userData;
            return arg;
        }
    }

    public sealed class CDownloadSuccessEventArgs : CFrameWorkEventArgs
    {
        public int nId
        {
            get;
            private set;
        }

        public string DownloadPath
        {
            get;
            private set;
        }

        public string DownloadUri
        {
            get;
            private set;
        }

        public long CurrentLen
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public override void Clear()
        {
            nId = 0;
            DownloadPath = null;
            DownloadUri = null;
            CurrentLen = 0L;
            UserData = null;
        }

        public static CDownloadSuccessEventArgs Create(int serialId, string downloadPath, string downloadUri, long currentLength, object userData)
        {
            CDownloadSuccessEventArgs arg = CReferencePool.Acquire<CDownloadSuccessEventArgs>();
            arg.nId = serialId;
            arg.DownloadPath = downloadPath;
            arg.DownloadUri = downloadUri;
            arg.CurrentLen = currentLength;
            arg.UserData = userData;
            return arg;
        }
    }


    public sealed class CDownloadFailEventArgs : CFrameWorkEventArgs
    {
        public int nId
        {
            get;
            private set;
        }

        public string DownloadPath
        {
            get;
            private set;
        }

        public string DownloadUri
        {
            get;
            private set;
        }

        public string ErrorMessage
        {
            get;
            private set;
        }

        public object UserData
        {
            get;
            private set;
        }

        public override void Clear()
        {
            nId = 0;
            DownloadPath = null;
            DownloadUri = null;
            UserData = null;
            ErrorMessage = null;
        }

        public static CDownloadFailEventArgs Create(int serialId, string downloadPath, string downloadUri, object userData, string a_szErrorMsg)
        {
            CDownloadFailEventArgs arg = CReferencePool.Acquire<CDownloadFailEventArgs>();
            arg.nId = serialId;
            arg.DownloadPath = downloadPath;
            arg.DownloadUri = downloadUri;
            arg.UserData = userData;
            arg.ErrorMessage = a_szErrorMsg;
            return arg;
        }
    }

}
