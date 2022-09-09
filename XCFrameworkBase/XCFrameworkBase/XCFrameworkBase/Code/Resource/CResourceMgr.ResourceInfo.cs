using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr : CGameFrameworkModule, IResourceMgr
    {
        private enum ELoadType : byte
        {
            LoadFormFile = 0,

            LoadFromMemory,
            LoadFromMemoryAndQuickDecrypt,
            LoadFromMemoryAndDecrypt,

            LoadFromBinary,
            LoadFromBinaryAndQuickDecrypt,
            LoadFromBinaryAndDecrypt,

        }


        private sealed class CResourceInfo
        {
            private readonly SResourceName m_ResourceName;
            private readonly string m_szFileSystemName;
            private readonly ELoadType m_eLoadType;
            private readonly int m_nLen;
            private readonly int m_nHashCode;
            private readonly int m_nCompressLen;
            private readonly bool m_bStorageInReadOnly;
            private bool m_bReady;

            public CResourceInfo(SResourceName resourceName, string fileSystemName, ELoadType loadType, int length, int hashCode, int compressedLength, bool storageInReadOnly, bool ready)
            {
                m_ResourceName = resourceName;
                m_szFileSystemName = fileSystemName;
                m_eLoadType = loadType;
                m_nLen = length;
                m_nHashCode = hashCode;
                m_nCompressLen = compressedLength;
                m_bStorageInReadOnly = storageInReadOnly;
                m_bReady = ready;
            }

            public void MarkReady()
            {
                m_bReady = true;
            }

            public SResourceName ResourceName
            {
                get
                {
                    return m_ResourceName;
                }
            }


            public string FileSystemName
            {
                get
                {
                    return m_szFileSystemName;
                }
            }

            public ELoadType LoadType
            {
                get
                {
                    return m_eLoadType;
                }
            }

            public int Length
            {
                get
                {
                    return m_nLen;
                }
            }

            public int HashCode
            {
                get
                {
                    return m_nHashCode;
                }
            }

            public int CompressedLength
            {
                get
                {
                    return m_nCompressLen;
                }
            }

            public bool StorageInReadOnly
            {
                get
                {
                    return m_bStorageInReadOnly;
                }
            }

            public bool Ready
            {
                get
                {
                    return m_bReady;
                }
            }


            public bool UseFileSystem
            {
                get
                {
                    return !string.IsNullOrEmpty(m_szFileSystemName);
                }
            }

            public bool IsLoadFromBinary
            {
                get
                {
                    return m_eLoadType == ELoadType.LoadFromBinary
                        || m_eLoadType == ELoadType.LoadFromBinaryAndQuickDecrypt
                        || m_eLoadType == ELoadType.LoadFromBinaryAndDecrypt;
                }
            }


        }
    }
}
