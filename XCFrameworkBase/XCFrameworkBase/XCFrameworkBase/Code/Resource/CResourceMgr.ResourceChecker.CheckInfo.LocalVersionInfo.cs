using System;
using System.Runtime.InteropServices;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr
    {
        private sealed partial class CResourceChecker
        {
            /// <summary>
            /// 本资源信息 是否存在,filesystemName,LoadType,len,hashcode
            /// </summary>
            private sealed partial class CCheckInfo
            {
                [StructLayout(LayoutKind.Auto)]
                private struct SLocalVersionInfo
                {
                    private readonly bool m_bExist;
                    private readonly string m_szFileSystemName;
                    private readonly ELoadType m_eLoadType;
                    private readonly int m_nLen;
                    private readonly int m_nHashCode;

                    public SLocalVersionInfo(string a_szFileSystemName, ELoadType a_eLoadType, int a_nLen, int a_nHashCode)
                    {
                        m_bExist = true;
                        m_szFileSystemName = a_szFileSystemName;
                        m_eLoadType = a_eLoadType;
                        m_nLen = a_nLen;
                        m_nHashCode = a_nHashCode;
                    }

                    public bool Exist => m_bExist;
                    public string FileSystemName => m_szFileSystemName;
                    public ELoadType LoadType => m_eLoadType;
                    public int Len => m_nLen;
                    public int HashCode => m_nHashCode;
                    public bool UseFileSystem => !string.IsNullOrEmpty(FileSystemName);
                }
            }
        }
    }
}
