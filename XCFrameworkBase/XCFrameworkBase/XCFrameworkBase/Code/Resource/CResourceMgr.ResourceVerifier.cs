using System;
using System.Collections.Generic;
using System.IO;

namespace XCFrameworkBase
{
    public partial class CResourceMgr
    {
        private sealed partial class CResourceVerifier
        {
            private const int mc_nCacheHashBytesLen = 4;

            private readonly CResourceMgr m_refResourceMgr;
            private readonly List<SVerifyInfo> m_listVerify;
            private readonly byte[] m_arrCache;
            private bool m_bLoadReadWriteVersionListComplete;
            private int m_nVerifyResourceLenPerFrame;
            private int m_nCurVerifyResourceIdx;
            private bool m_bFail;

            public Action<int, long> m_fnResourceVerifyStart;
            public Action<SResourceName, int> m_fnResourceVerifySuccess;
            public Action<SResourceName> m_fnResourceVerifyFail;
            public Action<bool> m_fnResourceVerifyComplete;

            public CResourceVerifier(CResourceMgr a_refMgr)
            {
                m_refResourceMgr = a_refMgr;
                m_listVerify = new List<SVerifyInfo>();
                m_arrCache = new byte[mc_nCacheHashBytesLen];
                m_bLoadReadWriteVersionListComplete = false;
                m_nVerifyResourceLenPerFrame = 0;
                m_nCurVerifyResourceIdx = 0;
                m_bFail = false;

                m_fnResourceVerifyComplete = null;
                m_fnResourceVerifyFail = null;
                m_fnResourceVerifyStart = null;
                m_fnResourceVerifySuccess = null;
            }

            public void Shutdown()
            {
                m_listVerify.Clear();
                m_bLoadReadWriteVersionListComplete = false;
                m_nVerifyResourceLenPerFrame = 0;
                m_nCurVerifyResourceIdx = 0;
                m_bFail = false;
            }

            public void VerifyResource(int a_nVerifyLenPerFrame)
            {
                m_nVerifyResourceLenPerFrame = a_nVerifyLenPerFrame;
                string szReadWriteVersionListFliePath = CUtility.Path.GetRemotePath(Path.Combine(m_refResourceMgr.m_szReadWritePath, ms_szLocalVersionListFileName));
                CLoadBytesCallbacks callback = new CLoadBytesCallbacks(_OnLoadReadWriteVersionListSuccess, _OnLoadReadWriteVersionListFail);
                m_refResourceMgr.m_ResourceHelper.LoadBytes(szReadWriteVersionListFliePath, callback, null);
            }

            private void _OnLoadReadWriteVersionListSuccess(string a_szFileUri, byte[] a_arrBytes, float a_fDuration, object a_oUserData)
            {
                MemoryStream memoryStream = null;
                try
                {
                    memoryStream = new MemoryStream(a_arrBytes, false);
                    SLocalVersionList versionList = m_refResourceMgr.m_ReadWriteVersionSerialize.Deserialize(memoryStream);
                    if (!versionList.IsValid)
                    {
                        return;
                    }

                    SVersionList.SResource[] arrResource = versionList.GetResources();
                    SVersionList.SFileSystem[] arrFileSystem = versionList.GetFileSystems();
                    Dictionary<SResourceName, string> mapResourceInFileSystem = new Dictionary<SResourceName, string>();
                    foreach (var fileSystem in arrFileSystem)
                    {
                        int[] arrResourceIdxes = fileSystem.GetResourceIdxes();
                        foreach (int nResourceIdx in arrResourceIdxes)
                        {
                            SVersionList.SResource resource = arrResource[nResourceIdx];
                            mapResourceInFileSystem.Add(new SResourceName(resource.Name, resource.Variant, resource.Extension), fileSystem.Name);
                        }
                    }

                    long nTotalLen = 0L;
                    foreach (var resource in arrResource)
                    {
                        SResourceName resourceName = new SResourceName(resource.Name, resource.Variant, resource.Extension);
                        string szFileSystemName = null;
                        mapResourceInFileSystem.TryGetValue(resourceName, out szFileSystemName);
                        nTotalLen += resource.Len;
                        m_listVerify.Add(new SVerifyInfo(resourceName, szFileSystemName, (ELoadType)resource.LoadType, resource.Len, resource.HashCode));
                    }

                    m_bLoadReadWriteVersionListComplete = true;
                    m_fnResourceVerifyStart?.Invoke(m_listVerify.Count, nTotalLen);
                }
                catch
                {

                }
                finally
                {
                    if (memoryStream != null)
                    {
                        memoryStream.Dispose();
                        memoryStream = null;
                    }
                }
            }

            private void _OnLoadReadWriteVersionListFail(string a_szFileUri, string a_szErrorMsg, object a_oUserData)
            {
                m_fnResourceVerifyComplete?.Invoke(true);
            }


            public void Update(float a_fElapseSed, float a_fRealElapseSed)
            {
                if (!m_bLoadReadWriteVersionListComplete)
                {
                    return;
                }

                int nLen = 0;
                while (m_nCurVerifyResourceIdx < m_listVerify.Count)
                {
                    SVerifyInfo verifyInfo = m_listVerify[m_nCurVerifyResourceIdx];
                    nLen += verifyInfo.Len;
                    if (_VerifyResource(verifyInfo))
                    {
                        m_nCurVerifyResourceIdx++;
                        m_fnResourceVerifySuccess?.Invoke(verifyInfo.ResourceName, verifyInfo.Len);
                    }
                    else
                    {
                        m_bFail = true;
                        m_listVerify.RemoveAt(m_nCurVerifyResourceIdx);
                        m_fnResourceVerifyFail?.Invoke(verifyInfo.ResourceName);
                    }

                    if (nLen >= m_nVerifyResourceLenPerFrame)
                    {
                        return;
                    }
                }

                m_bLoadReadWriteVersionListComplete = false;
                if (m_bFail)
                {
                    _GenerateReadWriteVersionList();
                }
                m_fnResourceVerifyComplete?.Invoke(!m_bFail);
            }

            private bool _VerifyResource(SVerifyInfo a_verifyInfo)
            {
                if (a_verifyInfo.UseFileSystem)
                {
                    IFileSystem fileSystem = m_refResourceMgr._GetFileSystem(a_verifyInfo.FileSystemName, false);
                    string szFileName = a_verifyInfo.ResourceName.FullName;
                    SFileInfo fileInfo = fileSystem.GetFileInfo(szFileName);
                    if (!fileInfo.IsValid())
                    {
                        return false;
                    }

                    int nLen = fileInfo.Len;
                    if (nLen != a_verifyInfo.Len)
                    {
                        fileSystem.DelFile(szFileName);
                        return false;
                    }

                    m_refResourceMgr._PrepareCacheStream();
                    fileSystem.ReadFile(szFileName, m_refResourceMgr.m_streamCached);
                    m_refResourceMgr.m_streamCached.Position = 0L;
                    int nHashCode = 0;
                    if (a_verifyInfo.LoadType == ELoadType.LoadFromBinaryAndDecrypt
                        || a_verifyInfo.LoadType == ELoadType.LoadFromBinaryAndQuickDecrypt
                        || a_verifyInfo.LoadType == ELoadType.LoadFromMemoryAndDecrypt
                        || a_verifyInfo.LoadType == ELoadType.LoadFromMemoryAndQuickDecrypt)
                    {
                        CUtility.Converter.GetBytes(a_verifyInfo.HashCode, m_arrCache);
                        if (a_verifyInfo.LoadType == ELoadType.LoadFromBinaryAndQuickDecrypt
                            || a_verifyInfo.LoadType == ELoadType.LoadFromMemoryAndQuickDecrypt)
                        {
                            nHashCode = CUtility.Verifier.GetCrc32(m_refResourceMgr.m_streamCached, m_arrCache, CUtility.Encryption.QuickEncryptLength);
                        }
                        else
                        {
                            nHashCode = CUtility.Verifier.GetCrc32(m_refResourceMgr.m_streamCached, m_arrCache, nLen);
                        }

                        Array.Clear(m_arrCache, 0, mc_nCacheHashBytesLen);
                    }
                    else
                    {
                        nHashCode = CUtility.Verifier.GetCrc32(m_refResourceMgr.m_streamCached);
                    }

                    return nHashCode == a_verifyInfo.HashCode;
                }
                else
                {
                    string szResourcePath = CUtility.Path.GetRegularPath(Path.Combine(m_refResourceMgr.m_szReadWritePath, a_verifyInfo.ResourceName.FullName));
                    if (!File.Exists(szResourcePath))
                    {
                        return false;
                    }
                    using (FileStream fileStream = new FileStream(szResourcePath, FileMode.Open, FileAccess.Read))
                    {
                        int nLen = (int)fileStream.Length;
                        if (nLen != a_verifyInfo.Len)
                        {
                            File.Delete(szResourcePath);
                            return false;
                        }

                        int nHashCode = 0;
                        if (a_verifyInfo.LoadType == ELoadType.LoadFromBinaryAndDecrypt
                            || a_verifyInfo.LoadType == ELoadType.LoadFromBinaryAndQuickDecrypt
                            || a_verifyInfo.LoadType == ELoadType.LoadFromMemoryAndDecrypt
                            || a_verifyInfo.LoadType == ELoadType.LoadFromMemoryAndQuickDecrypt)
                        {
                            CUtility.Converter.GetBytes(a_verifyInfo.HashCode, m_arrCache);
                            if (a_verifyInfo.LoadType == ELoadType.LoadFromBinaryAndQuickDecrypt
                                || a_verifyInfo.LoadType == ELoadType.LoadFromMemoryAndQuickDecrypt)
                            {
                                nHashCode = CUtility.Verifier.GetCrc32(fileStream, m_arrCache, CUtility.Encryption.QuickEncryptLength);
                            }
                            else
                            {
                                nHashCode = CUtility.Verifier.GetCrc32(fileStream, m_arrCache, nLen);
                            }

                            Array.Clear(m_arrCache, 0, mc_nCacheHashBytesLen);
                        }
                        else
                        {
                            nHashCode = CUtility.Verifier.GetCrc32(fileStream);
                        }

                        return nHashCode == a_verifyInfo.HashCode;
                    }
                }
            }

            private void _GenerateReadWriteVersionList()
            {
                string szReadWriteVersionListFileName = CUtility.Path.GetRegularPath(Path.Combine(m_refResourceMgr.m_szReadWritePath, ms_szLocalVersionListFileName));
                string szReadWriteVersionListTempFileName = CUtility.Text.Format("{0}.{1}", szReadWriteVersionListFileName, ms_szTempExtension);
                SortedDictionary<string, List<int>> mapFileSystemForGenerateReadWriteVersionList = new SortedDictionary<string, List<int>>();
                FileStream fileStream = null;
                try
                {
                    fileStream = new FileStream(szReadWriteVersionListTempFileName, FileMode.Create, FileAccess.Write);
                    SVersionList.SResource[] arrResourece = m_listVerify.Count > 0 ? new SVersionList.SResource[m_listVerify.Count] : null;
                    if (arrResourece != null)
                    {
                        int nIdx = 0;
                        foreach (var verifyInfo in m_listVerify)
                        {
                            arrResourece[nIdx] = new SVersionList.SResource(verifyInfo.ResourceName.Name, verifyInfo.ResourceName.Variant, verifyInfo.ResourceName.Extentsion, (byte)verifyInfo.LoadType, verifyInfo.Len, verifyInfo.HashCode, verifyInfo.Len, verifyInfo.HashCode, null);
                            if (verifyInfo.UseFileSystem)
                            {
                                List<int> listResourceIdx = null;
                                if (!mapFileSystemForGenerateReadWriteVersionList.TryGetValue(verifyInfo.FileSystemName, out listResourceIdx))
                                {
                                    listResourceIdx = new List<int>();
                                    mapFileSystemForGenerateReadWriteVersionList.Add(verifyInfo.FileSystemName, listResourceIdx);
                                }
                                listResourceIdx.Add(nIdx);
                            }
                            nIdx++;
                        }
                    }
                    SVersionList.SFileSystem[] arrFileSystem = mapFileSystemForGenerateReadWriteVersionList.Count > 0 ? new SVersionList.SFileSystem[mapFileSystemForGenerateReadWriteVersionList.Count] : null;
                    if (null != arrFileSystem)
                    {
                        int nIdx = 0;
                        foreach (var val in mapFileSystemForGenerateReadWriteVersionList)
                        {
                            arrFileSystem[nIdx++] = new SVersionList.SFileSystem(val.Key, val.Value.ToArray());
                            val.Value.Clear();
                        }
                    }

                    SLocalVersionList vesionList = new SLocalVersionList(arrResourece, arrFileSystem);
                    if (!m_refResourceMgr.m_ReadWriteVersionSerialize.Serialize(fileStream, vesionList))
                    {

                    }
                }
                catch
                {
                    if (File.Exists(szReadWriteVersionListTempFileName))
                    {
                        File.Delete(szReadWriteVersionListTempFileName);
                    }
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Dispose();
                        fileStream = null;
                    }
                }

                if (File.Exists(szReadWriteVersionListFileName))
                {
                    File.Delete(szReadWriteVersionListFileName);
                }
                File.Move(szReadWriteVersionListTempFileName, szReadWriteVersionListFileName);
            }
        }
    }

}
