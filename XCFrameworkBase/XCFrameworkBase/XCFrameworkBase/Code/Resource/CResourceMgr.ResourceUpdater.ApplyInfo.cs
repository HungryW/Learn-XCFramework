using System;
using System.Runtime.InteropServices;

namespace XCFrameworkBase
{
    public partial class CResourceMgr
    {
     
        private sealed partial class CResourceUpdater
        {
            /// <summary>
            /// 初始化资源应用信息的新实例。
            /// <param name="resourceName">资源名称。</param>
            /// <param name="fileSystemName">资源所在的文件系统名称。</param>
            /// <param name="loadType">资源加载方式。</param>
            /// <param name="offset">资源偏移。</param>
            /// <param name="length">资源大小。</param>
            /// <param name="hashCode">资源哈希值。</param>
            /// <param name="compressedLength">压缩后大小。</param>
            /// <param name="compressedHashCode">压缩后哈希值。</param>
            /// <param name="resourcePath">资源路径。</param>
            /// </summary>

            [StructLayout(LayoutKind.Auto)]
            private struct SApplyInfo
            {
                private readonly SResourceName m_ResName;
                private readonly string m_szFileSystemName;
                private readonly ELoadType m_eLoadType;
                private readonly long m_nOffset;
                private readonly int m_nLen;
                private readonly int m_nHashCode;
                private readonly int m_nCompressLen;
                private readonly int m_nCompressHashCode;
                private readonly string m_szResPath;

                public SApplyInfo(SResourceName a_resName, string a_szFileSysName, ELoadType a_eLoadType, long a_nOffset, int a_Len, int a_nHashCode, int a_nCompressLen, int a_nCompressHashcode, string a_szResPath)
                {
                    m_ResName = a_resName;
                    m_szFileSystemName = a_szFileSysName;
                    m_szResPath = a_szResPath;
                    m_eLoadType = a_eLoadType;
                    m_nLen = a_Len;
                    m_nOffset = a_nOffset;
                    m_nHashCode = a_nHashCode;
                    m_nCompressLen = a_nCompressLen;
                    m_nCompressHashCode = a_nCompressHashcode;
                }

                public SResourceName ResourceName => m_ResName;
                public string FileSystemName => m_szFileSystemName;
                public ELoadType LoadType => m_eLoadType;
                public string ResourcePath => m_szResPath;
                public long Offset => m_nOffset;
                public int Len => m_nLen;
                public int HashCode => m_nHashCode;
                public int CompressLen => m_nLen;
                public int CompressHashCode => m_nHashCode;
                public bool UseFileSystem => !string.IsNullOrEmpty(FileSystemName);
            }
        }
    }
}
