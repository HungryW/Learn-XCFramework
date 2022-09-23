using System;
using System.Runtime.InteropServices;

namespace XCFrameworkBase
{
    public partial class CResourceMgr
    {

        private sealed partial class CResourceUpdater
        {
            /// <summary>
            /// 比ApplyInfo 多 Downloading标记和 retrycount
            /// </summary>
            private class CUpdateInfo
            {
                private readonly SResourceName m_ResName;
                private readonly string m_szFileSystemName;
                private readonly ELoadType m_eLoadType;
                private readonly int m_nLen;
                private readonly int m_nHashCode;
                private readonly int m_nCompressLen;
                private readonly int m_nCompressHashCode;
                private readonly string m_szResPath;
                private bool m_bDownloading;
                private int m_nRetryCount;

                public CUpdateInfo(SResourceName a_resName, string a_szFileSysName, ELoadType a_eLoadType, int a_Len, int a_nHashCode, int a_nCompressLen, int a_nCompressHashcode, string a_szResPath)
                {
                    m_ResName = a_resName;
                    m_szFileSystemName = a_szFileSysName;
                    m_szResPath = a_szResPath;
                    m_eLoadType = a_eLoadType;
                    m_nLen = a_Len;
                    m_nHashCode = a_nHashCode;
                    m_nCompressLen = a_nCompressLen;
                    m_nCompressHashCode = a_nCompressHashcode;
                    m_bDownloading = false;
                    m_nRetryCount = 0;
                }

                public SResourceName ResourceName => m_ResName;
                public string FileSystemName => m_szFileSystemName;
                public ELoadType LoadType => m_eLoadType;
                public string ResourcePath => m_szResPath;
                public int Len => m_nLen;
                public int HashCode => m_nHashCode;
                public int CompressLen => m_nLen;
                public int CompressHashCode => m_nHashCode;
                public bool UseFileSystem => !string.IsNullOrEmpty(FileSystemName);


                public bool Downloading
                {
                    get
                    {
                        return m_bDownloading;
                    }
                    set
                    {
                        m_bDownloading = value;
                    }
                }

                public int RetryCount
                {
                    get
                    {
                        return m_nRetryCount;
                    }
                    set
                    {
                        m_nRetryCount = value;
                    }
                }
            }
        }
    }
}
