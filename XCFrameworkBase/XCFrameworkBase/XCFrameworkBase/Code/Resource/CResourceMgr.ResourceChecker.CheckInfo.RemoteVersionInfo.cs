using System;
using System.Runtime.InteropServices;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr
    {
        private sealed partial class CResourceChecker
        {
            private sealed partial class CCheckInfo
            {
                /// <summary>
                /// 远程资源信息 是否存在,filesystemName,LoadType,len,hashcode,压缩长度,压缩hashcode
                /// </summary>
                [StructLayout(LayoutKind.Auto)]
                private struct SRemoteVersionInfo
                {
                    private readonly bool m_bExist;
                    private readonly string m_szFileSystemName;
                    private readonly ELoadType m_eLoadType;
                    private readonly int m_nLen;
                    private readonly int m_nHashCode;
                    private readonly int m_nCompressedLen;
                    private readonly int m_nCompressedHashCode;

                    public SRemoteVersionInfo(string a_szFileSystemName, ELoadType a_eLoadType, int a_nLen, int a_nHashCode, int a_nCompressedLen, int a_nCompressHashCode)
                    {
                        m_bExist = true;
                        m_szFileSystemName = a_szFileSystemName;
                        m_eLoadType = a_eLoadType;
                        m_nLen = a_nLen;
                        m_nHashCode = a_nHashCode;
                        m_nCompressedHashCode = a_nCompressHashCode;
                        m_nCompressedLen = a_nCompressedLen;
                    }

                    public bool Exist => m_bExist;
                    public string FileSystemName => m_szFileSystemName;
                    public ELoadType LoadType => m_eLoadType;
                    public int Len => m_nLen;
                    public int HashCode => m_nHashCode;
                    public int CompressLen => m_nLen;
                    public int CompressHashCode => m_nHashCode;
                    public bool UseFileSystem => !string.IsNullOrEmpty(FileSystemName);
                }
            }
        }
    }
}
