using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace XCFrameworkBase
{
    public partial class CResourceMgr
    {
        private sealed partial class CResourceUpdater
        {
            private const int mc_nCachedHashBytesLen = 4;
            private const int mc_nCachedBytesLen = 0x1000;

            private readonly CResourceMgr m_refResMgr;
            private readonly Queue<SApplyInfo> m_queueApplyWaitingInfo;
            private readonly List<CUpdateInfo> m_listUpdateWaitingInfo;
            private readonly HashSet<CUpdateInfo> m_setUpdateWaitingInfoWhilePlaying;
            private readonly Dictionary<SResourceName, CUpdateInfo> m_mapUpdateCandidateInfo;
            private readonly SortedDictionary<string, List<int>> m_mapFileSystemForGenerateReadWriteVersionList;
            private readonly List<SResourceName> m_listCacheResourceName;
            private readonly byte[] m_arrCacheHash;
            private readonly byte[] m_arrCacheBytes;

            private IDownloadMgr m_refDownloadMgr;
            private bool m_bCheckResourceComplete;

            private string m_szApplyingResPackPath;
            private IFileSystem m_ApplyingResPackStream;

            private CResourceGroup m_UpdatingResGroup;

            private int m_nGenergateReadWriteVersionListLen;
            private int m_nCurGenergateReadWriteVersionListLen;
            private string m_szReadWrireVersionListFileName;
            private string m_szReadWriteVersionListTempFileName;

            private int m_nUpdateRetryCount;
            private bool m_bFailFlag;


            public CResourceUpdater(CResourceMgr a_refResMgr)
            {
                m_refResMgr = a_refResMgr;
                m_queueApplyWaitingInfo = new Queue<SApplyInfo>();
                m_listUpdateWaitingInfo = new List<CUpdateInfo>();
                m_setUpdateWaitingInfoWhilePlaying = new HashSet<CUpdateInfo>();
                m_mapUpdateCandidateInfo = new Dictionary<SResourceName, CUpdateInfo>();
                m_mapFileSystemForGenerateReadWriteVersionList = new SortedDictionary<string, List<int>>();
                m_listCacheResourceName = new List<SResourceName>();
                m_arrCacheHash = new byte[mc_nCachedHashBytesLen];
                m_arrCacheBytes = new byte[mc_nCachedBytesLen];
                m_refDownloadMgr = null;
                m_bCheckResourceComplete = false;
                m_szApplyingResPackPath = null;
                m_ApplyingResPackStream = null;
                m_UpdatingResGroup = null;
                m_nGenergateReadWriteVersionListLen = 0;
                m_nCurGenergateReadWriteVersionListLen = 0;
                m_nUpdateRetryCount = 3;
                m_bFailFlag = false;
                m_szReadWrireVersionListFileName = CUtility.Path.GetRegularPath(Path.Combine(m_refResMgr.m_szReadWritePath, ms_szLocalVersionListFileName));
                m_szReadWriteVersionListTempFileName = CUtility.Text.Format("{0}.{1}", m_szReadWrireVersionListFileName, ms_szTempExtension);
            }


            public void AddResourceUpdate(SResourceName a_resName, string a_szFileSysName, ELoadType a_eLoadType, int a_nLen, int a_nHash, int a_nCompressLen, int a_nCompressHash, string a_szResourcePath)
            {
                CUpdateInfo info = new CUpdateInfo(a_resName, a_szFileSysName, a_eLoadType, a_nLen, a_nHash, a_nCompressLen, a_nCompressHash, a_szResourcePath);
                m_mapUpdateCandidateInfo.Add(a_resName, info);
            }

            private bool _OnDownloadResource(CUpdateInfo a_updateInfo)
            {
                if (a_updateInfo.Downloading)
                {
                    return false;
                }
                a_updateInfo.Downloading = true;
                string szResourceFullNameWithCrc32 = a_updateInfo.ResourceName.Variant != null
                                                    ? CUtility.Text.Format("{0}.{1}.{2:x8}.{3}", a_updateInfo.ResourceName.Name, a_updateInfo.ResourceName.Variant, a_updateInfo.HashCode, a_updateInfo.ResourceName.Extentsion)
                                                    : CUtility.Text.Format("{0}.{1:x8}.{2}", a_updateInfo.ResourceName.Name, a_updateInfo.HashCode, a_updateInfo.ResourceName.Extentsion);
                string szResRemoteUri = CUtility.Path.GetRemotePath(Path.Combine(m_refResMgr.m_szUpdatePrefixUri, szResourceFullNameWithCrc32));
                m_refDownloadMgr.AddDownload(a_updateInfo.ResourcePath, szResRemoteUri, null, 0, a_updateInfo);
                return true;
            }

            private void _OnDownLoadStart(object a_oSender, CDownloadStartEventArgs a_arg)
            {
                CUpdateInfo updateInfo = a_arg.UserData as CUpdateInfo;
                if (updateInfo == null)
                {
                    return;
                }

                if (m_refDownloadMgr == null)
                {
                    return;
                }

                if (a_arg.CurrentLen > int.MaxValue)
                {
                    return;
                }
            }

            private void _OnDownloadUpdate(object a_oSender, CDownloadUpadateEventArgs a_arg)
            {
                CUpdateInfo updateInfo = a_arg.UserData as CUpdateInfo;
                if (updateInfo == null)
                {
                    return;
                }

                if (m_refDownloadMgr == null)
                {
                    return;
                }

            }

            private void _OnDownloadSuccess(object a_oSender, CDownloadSuccessEventArgs a_arg)
            {
                CUpdateInfo updateInfo = a_arg.UserData as CUpdateInfo;
                if (updateInfo == null)
                {
                    return;
                }

                try
                {
                    using (FileStream fileStream = new FileStream(a_arg.DownloadPath, FileMode.Open, FileAccess.ReadWrite))
                    {
                        bool bCompressed = updateInfo.Len != updateInfo.CompressLen
                                            || updateInfo.HashCode != updateInfo.CompressHashCode;

                        int nLen = (int)fileStream.Length;
                        if (nLen != updateInfo.CompressLen)
                        {
                            fileStream.Close();
                            _ToDownloadFail(CUtility.Text.Format("Resource compressed length error, need '{0}', downloaded '{1}'.", updateInfo.CompressLen, nLen), a_arg);
                            return;
                        }

                        if (bCompressed)
                        {
                            fileStream.Position = 0;
                            int nHashCode = CUtility.Verifier.GetCrc32(fileStream);
                            if (nHashCode != updateInfo.CompressHashCode)
                            {
                                fileStream.Close();
                                _ToDownloadFail(CUtility.Text.Format("Resource compressed hash error, need '{0}', downloaded '{1}'.", updateInfo.CompressHashCode, nHashCode), a_arg);
                                return;
                            }

                            fileStream.Position = 0;
                            m_refResMgr._PrepareCacheStream();
                            if (!CUtility.Compression.Decompress(fileStream, m_refResMgr.m_streamCached))
                            {
                                fileStream.Close();
                                _ToDownloadFail(CUtility.Text.Format("Resource Decompress fail {0}", a_arg.DownloadPath), a_arg);
                                return;
                            }

                            int nUnCompressLen = (int)m_refResMgr.m_streamCached.Length;
                            if (nUnCompressLen != updateInfo.Len)
                            {
                                fileStream.Close();
                                _ToDownloadFail(CUtility.Text.Format("Resource  length error, need '{0}', downloaded '{1}'.", updateInfo.Len, nUnCompressLen), a_arg);
                                return;
                            }

                            fileStream.Position = 0L;
                            fileStream.SetLength(0);
                            fileStream.Write(m_refResMgr.m_streamCached.GetBuffer(), 0, nUnCompressLen);
                        }
                        else
                        {
                            int nHash = 0;
                            fileStream.Position = 0;
                            if (updateInfo.LoadType == ELoadType.LoadFromBinaryAndDecrypt
                                || updateInfo.LoadType == ELoadType.LoadFromBinaryAndQuickDecrypt
                                || updateInfo.LoadType == ELoadType.LoadFromMemoryAndDecrypt
                                || updateInfo.LoadType == ELoadType.LoadFromMemoryAndQuickDecrypt)
                            {
                                CUtility.Converter.GetBytes(updateInfo.HashCode, m_arrCacheHash);

                                if (updateInfo.LoadType == ELoadType.LoadFromBinaryAndDecrypt
                                    || updateInfo.LoadType == ELoadType.LoadFromMemoryAndDecrypt)
                                {
                                    nHash = CUtility.Verifier.GetCrc32(fileStream, m_arrCacheHash, CUtility.Encryption.QuickEncryptLength);
                                }
                                else if (updateInfo.LoadType == ELoadType.LoadFromBinaryAndDecrypt
                                    || updateInfo.LoadType == ELoadType.LoadFromMemoryAndDecrypt)
                                {
                                    nHash = CUtility.Verifier.GetCrc32(fileStream, m_arrCacheHash, nLen);
                                }

                                Array.Clear(m_arrCacheHash, 0, mc_nCachedHashBytesLen);
                            }
                            else
                            {
                                nHash = CUtility.Verifier.GetCrc32(fileStream);
                            }

                            if (nHash != updateInfo.HashCode)
                            {
                                fileStream.Close();
                                _ToDownloadFail(CUtility.Text.Format("Resource  hash error, need '{0}', downloaded '{1}'.", updateInfo.HashCode, nHash), a_arg);
                                return;
                            }
                        }
                    }

                    if (updateInfo.UseFileSystem)
                    {
                        IFileSystem fileSystem = m_refResMgr._GetFileSystem(updateInfo.FileSystemName, false);
                        bool retVal = fileSystem.WriteFile(updateInfo.ResourceName.FullName, updateInfo.ResourcePath);
                        if (File.Exists(updateInfo.ResourcePath))
                        {
                            File.Delete(updateInfo.ResourcePath);
                        }

                        if (!retVal)
                        {
                            _ToDownloadFail(CUtility.Text.Format("Write resource to file system '{0}' error.", fileSystem.FullPath), a_arg);
                            return;
                        }
                    }

                    m_mapUpdateCandidateInfo.Remove(updateInfo.ResourceName);
                    m_listUpdateWaitingInfo.Remove(updateInfo);
                    m_setUpdateWaitingInfoWhilePlaying.Remove(updateInfo);
                    m_refResMgr.m_mapResourceInfo[updateInfo.ResourceName].MarkReady();
                    m_refResMgr.m_mapReadWriteResInfo.Add(updateInfo.ResourceName, new SReadWriteResourceInfo(updateInfo.FileSystemName, updateInfo.LoadType, updateInfo.Len, updateInfo.HashCode));

                }
            }

            private void _ToDownloadFail(string a_szErrorMsg, CDownloadSuccessEventArgs a_arg)
            {

            }

            public void OnCheckResourceComplete(bool a_bNeedGenerateReadWriteVersionList)
            {
                m_bCheckResourceComplete = true;
                if (a_bNeedGenerateReadWriteVersionList)
                {
                    _GenerateReadWriteVersionList();
                }
            }


            private void _GenerateReadWriteVersionList()
            {

            }
        }
    }
}
