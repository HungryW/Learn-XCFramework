using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr : CGameFrameworkModule, IResourceMgr
    {
        /// <summary>
        /// filesystemName, loadType, Len, Hash
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        private struct SReadWriteResourceInfo
        {
            private readonly string m_szFileSystemName;
            private readonly ELoadType m_eLoadType;
            private readonly int m_nLen;
            private readonly int m_nHashCode;

            public SReadWriteResourceInfo(string a_szFileSystemName, ELoadType a_eLoadType, int a_nLen, int a_nHashCode)
            {
                m_szFileSystemName = a_szFileSystemName;
                m_eLoadType = a_eLoadType;
                m_nLen = a_nLen;
                m_nHashCode = a_nHashCode;
            }

            public string FileSystemName => m_szFileSystemName;
            public ELoadType LoadType => m_eLoadType;
            public int Len => m_nLen;
            public int HashCode => m_nHashCode;
            public bool UseFileSystem => !string.IsNullOrEmpty(FileSystemName);
        }
    }
}
