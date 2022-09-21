using System;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr
    {
        private sealed partial class CResourceChecker
        {
            public enum ECheckStatus : byte
            {
                Unkown = 0,
                StorageInReadyOnly,
                StorageInReadWrite,
                Unavailable,
                Update,
                Disuse,
            }
            /// <summary>
            /// 保存每一个资源的3种版本信息,远端,只读区,读写区
            /// 根据3种版本信息,确定资源的状态 不变 要更新 删除
            /// 更新是先删除再增加
            /// </summary>

            private sealed partial class CCheckInfo
            {
                private readonly SResourceName m_ResourceName;
                private ECheckStatus m_eStatus;
                private bool m_bNeedRemove;
                private bool m_bNeedMoveToDisk;
                private bool m_bNeedMoveToFileSystem;
                private SRemoteVersionInfo m_RemoteInfo;
                private SLocalVersionInfo m_ReadOnlyInfo;
                private SLocalVersionInfo m_ReadWriteInfo;
                private string m_szCacheFileSystemName;

                public CCheckInfo(SResourceName a_resourceName)
                {
                    m_ResourceName = a_resourceName;
                    m_eStatus = ECheckStatus.Unkown;
                    m_bNeedRemove = false;
                    m_bNeedMoveToDisk = false;
                    m_bNeedMoveToFileSystem = false;
                    m_RemoteInfo = default(SRemoteVersionInfo);
                    m_ReadOnlyInfo = default(SLocalVersionInfo);
                    m_ReadWriteInfo = default(SLocalVersionInfo);
                    m_szCacheFileSystemName = null;
                }

                public SResourceName ResourceName => m_ResourceName;
                public ECheckStatus Status => m_eStatus;
                public bool NeedRemove => m_bNeedRemove;
                public bool NeedMoveToDisk => m_bNeedMoveToDisk;
                public bool NeedMoveToFileSystem => m_bNeedMoveToFileSystem;

                public string FileSystemName => m_RemoteInfo.FileSystemName;
                public ELoadType LoadType => m_RemoteInfo.LoadType;
                public int Len => m_RemoteInfo.Len;
                public int HashCode => m_RemoteInfo.HashCode;
                public int CompressLen => m_RemoteInfo.CompressLen;
                public int CompressHashCode => m_RemoteInfo.CompressHashCode;

                public string ReadWriteFileSystemName => m_ReadWriteInfo.FileSystemName;
                public bool ReadWriteUseFileSystem => m_ReadWriteInfo.UseFileSystem;

                public void SetCachedFileSystemName(string a_szFileSystemName)
                {
                    m_szCacheFileSystemName = a_szFileSystemName;
                }

                public void SetRemoteInfo(ELoadType a_eLoadType, int a_nLen, int a_nHashCode, int a_nCompressLen, int a_nCompressHashCode)
                {
                    if (m_RemoteInfo.Exist)
                    {
                        return;
                    }
                    m_RemoteInfo = new SRemoteVersionInfo(m_szCacheFileSystemName, a_eLoadType, a_nLen, a_nHashCode, a_nCompressLen, a_nCompressHashCode);
                    m_szCacheFileSystemName = null;
                }

                public void SetReadOnlyInfo(ELoadType a_eLoadType, int a_nLen, int a_nHashCode)
                {
                    if (m_ReadOnlyInfo.Exist)
                    {
                        return;
                    }
                    m_ReadOnlyInfo = new SLocalVersionInfo(m_szCacheFileSystemName, a_eLoadType, a_nLen, a_nHashCode);
                    m_szCacheFileSystemName = null;
                }

                public void SetReadWriteInfo(ELoadType a_eLoadType, int a_nLen, int a_nHashCode)
                {
                    if (m_ReadWriteInfo.Exist)
                    {
                        return;
                    }

                    m_ReadWriteInfo = new SLocalVersionInfo(m_szCacheFileSystemName, a_eLoadType, a_nLen, a_nHashCode);
                    m_szCacheFileSystemName = null;
                }

                public void RefreshStatus(string a_szCurVariant, bool a_bIsIgnoreOtherVariant)
                {
                    if (!m_RemoteInfo.Exist)
                    {
                        m_eStatus = ECheckStatus.Disuse;
                        m_bNeedRemove = m_ReadWriteInfo.Exist;
                        return;
                    }

                    if (m_ResourceName.Variant != null && m_ResourceName.Variant != a_szCurVariant)
                    {
                        m_eStatus = ECheckStatus.Unavailable;
                        m_bNeedRemove = !a_bIsIgnoreOtherVariant && m_ReadWriteInfo.Exist;
                        return;
                    }

                    if (m_ReadOnlyInfo.Exist
                        && m_ReadOnlyInfo.FileSystemName == m_RemoteInfo.FileSystemName
                        && m_ReadOnlyInfo.LoadType == m_RemoteInfo.LoadType
                        && m_ReadOnlyInfo.Len == m_RemoteInfo.Len
                        && m_ReadOnlyInfo.HashCode == m_RemoteInfo.HashCode)
                    {
                        m_eStatus = ECheckStatus.StorageInReadyOnly;
                        m_bNeedRemove = m_ReadWriteInfo.Exist;
                        return;
                    }


                    if (m_ReadWriteInfo.Exist
                        && m_ReadWriteInfo.LoadType == m_RemoteInfo.LoadType
                        && m_ReadWriteInfo.Len == m_RemoteInfo.Len
                        && m_ReadOnlyInfo.HashCode == m_RemoteInfo.HashCode)
                    {
                        bool bChangeFileSystem = m_ReadWriteInfo.FileSystemName != m_RemoteInfo.FileSystemName;
                        m_eStatus = ECheckStatus.StorageInReadWrite;
                        m_bNeedMoveToDisk = m_ReadWriteInfo.UseFileSystem && bChangeFileSystem;
                        m_bNeedMoveToFileSystem = m_RemoteInfo.UseFileSystem && bChangeFileSystem;
                        return;
                    }

                    m_eStatus = ECheckStatus.Update;
                    m_bNeedRemove = m_ReadWriteInfo.Exist;
                }
            }
        }
    }
}
