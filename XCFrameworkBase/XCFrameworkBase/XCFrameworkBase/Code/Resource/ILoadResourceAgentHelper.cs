using System;

namespace XCFrameworkBase
{
    public interface ILoadResourceAgentHelper
    {
        event EventHandler<CLoadResAgentHelperUpdateEventArgs> OnUpdate;
        event EventHandler<CLoadResAgentHelperReadFileCompleteEventArgs> OnReadFileComplete;
        event EventHandler<CLoadResAgentHelperReadBytesCompleteEventArgs> OnReadBytesComplete;
        event EventHandler<CLoadResAgentHelperParseBytesCompleteEventArgs> OnParseBytesComplete;
        event EventHandler<CLoadResAgentHelperLoadCompleteEventArgs> OnLoadComplete;
        event EventHandler<CLoadResAgentHelperErrorEventArgs> OnLoadFail;

        void ReadFile(string a_szFullPath);
        void ReadFile(IFileSystem fileSystem, string a_szName);

        void ReadBytes(string a_szFullPath);
        void ReadBytes(IFileSystem fileSystem, string a_szName);

        void ParseBytes(byte[] bytes);

        void LoadAsset(object resource, string a_szAssetName, Type a_tAsset, bool a_bIsScene);

        void Reset();
    }


    public enum ELoadResProgress : byte
    {
        Unknown = 0,

        ReadRes,
        LoadRes,
        LoadAsset,
        LoadScene,
    }

    public enum ELoadResStatus : byte
    {
        Success = 0,
        NotExist,
        NotReady,
        DependencyError,
        TypeError,
        AssetError,
    }

    public sealed class CLoadResAgentHelperUpdateEventArgs : CFrameWorkEventArgs
    {
        public ELoadResProgress Type
        {
            get;
            private set;
        }

        public float Progress
        {
            get;
            private set;
        }

        public CLoadResAgentHelperUpdateEventArgs()
        {
            Type = ELoadResProgress.Unknown;
            Progress = 0f;
        }

        public override void Clear()
        {
            Type = ELoadResProgress.Unknown;
            Progress = 0f;
        }

        public static CLoadResAgentHelperUpdateEventArgs Create(ELoadResProgress a_eType, float a_fProgress)
        {
            CLoadResAgentHelperUpdateEventArgs arg = CReferencePool.Acquire<CLoadResAgentHelperUpdateEventArgs>();
            arg.Type = a_eType;
            arg.Progress = a_fProgress;
            return arg;
        }
    }

    public sealed class CLoadResAgentHelperReadFileCompleteEventArgs : CFrameWorkEventArgs
    {
        public object Resource
        {
            get;
            private set;
        }

        public CLoadResAgentHelperReadFileCompleteEventArgs()
        {
            Resource = null;
        }

        public override void Clear()
        {
            Resource = null;
        }

        public CLoadResAgentHelperReadFileCompleteEventArgs Create(object a_oRes)
        {
            CLoadResAgentHelperReadFileCompleteEventArgs arg = CReferencePool.Acquire<CLoadResAgentHelperReadFileCompleteEventArgs>();
            arg.Resource = a_oRes;
            return arg;
        }
    }

    public sealed class CLoadResAgentHelperReadBytesCompleteEventArgs : CFrameWorkEventArgs
    {
        private byte[] m_bytes;

        public CLoadResAgentHelperReadBytesCompleteEventArgs()
        {
            m_bytes = null;
        }

        public override void Clear()
        {
            m_bytes = null;
        }

        public byte[] GetByts()
        {
            return m_bytes;
        }

        public static CLoadResAgentHelperReadBytesCompleteEventArgs Create(byte[] a_arrbytes)
        {
            CLoadResAgentHelperReadBytesCompleteEventArgs arg = CReferencePool.Acquire<CLoadResAgentHelperReadBytesCompleteEventArgs>();
            arg.m_bytes = a_arrbytes;
            return arg;
        }
    }

    public sealed class CLoadResAgentHelperParseBytesCompleteEventArgs : CFrameWorkEventArgs
    {
        public object Resource
        {
            get;
            private set;
        }

        public CLoadResAgentHelperParseBytesCompleteEventArgs()
        {
            Resource = null;
        }

        public override void Clear()
        {
            Resource = null;
        }

        public CLoadResAgentHelperParseBytesCompleteEventArgs Create(object a_oRes)
        {
            CLoadResAgentHelperParseBytesCompleteEventArgs arg = CReferencePool.Acquire<CLoadResAgentHelperParseBytesCompleteEventArgs>();
            arg.Resource = a_oRes;
            return arg;
        }
    }

    public sealed class CLoadResAgentHelperLoadCompleteEventArgs : CFrameWorkEventArgs
    {
        public object Asset
        {
            get;
            private set;
        }

        public CLoadResAgentHelperLoadCompleteEventArgs()
        {
            Asset = null;
        }

        public override void Clear()
        {
            Asset = null;
        }

        public CLoadResAgentHelperLoadCompleteEventArgs Create(object a_oAsset)
        {
            CLoadResAgentHelperLoadCompleteEventArgs arg = CReferencePool.Acquire<CLoadResAgentHelperLoadCompleteEventArgs>();
            arg.Asset = a_oAsset;
            return arg;
        }
    }

    public sealed class CLoadResAgentHelperErrorEventArgs : CFrameWorkEventArgs
    {
        public ELoadResStatus Status
        {
            get;
            private set;
        }

        public string ErrorMsg
        {
            get;
            private set;
        }

        public CLoadResAgentHelperErrorEventArgs()
        {
            Status = ELoadResStatus.Success;
            ErrorMsg = null;
        }

        public override void Clear()
        {
            Status = ELoadResStatus.Success;
            ErrorMsg = null;
        }

        public static CLoadResAgentHelperErrorEventArgs Create(ELoadResStatus a_eStatus, string a_szErrorMsg)
        {
            CLoadResAgentHelperErrorEventArgs arg = CReferencePool.Acquire<CLoadResAgentHelperErrorEventArgs>();
            arg.Status = a_eStatus;
            arg.ErrorMsg = a_szErrorMsg;
            return arg;

        }
    }
}
