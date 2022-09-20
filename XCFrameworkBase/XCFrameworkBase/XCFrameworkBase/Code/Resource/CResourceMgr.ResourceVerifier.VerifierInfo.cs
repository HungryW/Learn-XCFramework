using System;

namespace XCFrameworkBase
{
    public partial class CResourceMgr
    {
        private sealed partial class CResourceVerifier
        {
            private struct SVerifyInfo
            {
                private readonly SResourceName m_resourceName;
                private readonly string m_szFileSystemName;
                private readonly ELoadType m_eLoadType;
                private readonly int m_nLen;
                private readonly int m_nHashCode;

                public SVerifyInfo(SResourceName a_resourceName, string a_szFileSystemName, ELoadType a_eLoadType, int a_nLen, int a_nHashCode)
                {
                    m_resourceName = a_resourceName;
                    m_szFileSystemName = a_szFileSystemName;
                    m_eLoadType = a_eLoadType;
                    m_nLen = a_nLen;
                    m_nHashCode = a_nHashCode;
                }

                public SResourceName ResourceName => m_resourceName;
                public string FileSystemName => m_szFileSystemName;
                public ELoadType LoadType => m_eLoadType;
                public int Len => m_nLen;
                public int HashCode => m_nHashCode;
                public bool UseFileSystem => !string.IsNullOrEmpty(FileSystemName);
            }
        }
    }

}
