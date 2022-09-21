using System;
using System.Collections.Generic;
using System.IO;
using static XCFrameworkBase.SVersionList;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr
    {
        /// <summary>
        /// 校验那些资源需要 更新和删除, 创建了所有的AssetInfo和ResourceInfo
        /// 主要接口只有checkResource
        /// 读取和解析了 远端资源列表文件,读写区的资源列表文件,只读区的资源列表文件
        /// 生成每个的资源的checkInfo,根据checkInfo判断更新状态,主要判断了资源列表文件的长度和Hash值
        /// 解析远端资源列表文件时生成了所有AssetInfo,因为资源是以远端资源为标准判断的
        /// 判断更新状态时 生成了ResourceInfo,根据不同的状态 主要区别有 存在哪里(只读或读写) 和 是否准备好(是否要更新)
        /// </summary>
        private sealed partial class CResourceChecker
        {
            private readonly CResourceMgr m_refResMgr;
            private readonly Dictionary<SResourceName, CCheckInfo> m_mapCheckInfos;
            private string m_szCurVariant;
            private bool m_bIgnoreOtherVariant;
            private bool m_bRemoteVersionListReady;
            private bool m_bReadOnlyVersionListReady;
            private bool m_bReadWriteVersionListReady;

            public Action<SResourceName, string, ELoadType, int, int, int, int> m_OnResNeedUpdate;
            public Action<int, int, int, long, long> m_OnCheckComplete;

            public CResourceChecker(CResourceMgr a_refResMgr)
            {
                m_refResMgr = a_refResMgr;
                m_mapCheckInfos = new Dictionary<SResourceName, CCheckInfo>();
                m_szCurVariant = null;
                m_bIgnoreOtherVariant = false;
                m_bReadOnlyVersionListReady = false;
                m_bReadWriteVersionListReady = false;
                m_bRemoteVersionListReady = false;
                m_OnResNeedUpdate = null;
                m_OnCheckComplete = null;
            }

            public void CheckResource(string a_szCurVariant, bool a_bIgnoreOtherVariant)
            {
                if (m_refResMgr.m_ResourceHelper == null)
                {
                    return;
                }
                m_szCurVariant = a_szCurVariant;
                m_bIgnoreOtherVariant = a_bIgnoreOtherVariant;

                string szRemoteVersionListFilePath = CUtility.Path.GetRemotePath(Path.Combine(m_refResMgr.m_szReadWritePath, ms_szRemoteVersionListFileName));
                m_refResMgr.m_ResourceHelper.LoadBytes(szRemoteVersionListFilePath, new CLoadBytesCallbacks(_OnLoadRemoteVersionListSuccess, _OnLoadRemoteVersionListFail), null);
                string szReadWriteVersionListFilePath = CUtility.Path.GetRemotePath(Path.Combine(m_refResMgr.m_szReadWritePath, ms_szLocalVersionListFileName));
                m_refResMgr.m_ResourceHelper.LoadBytes(szReadWriteVersionListFilePath, new CLoadBytesCallbacks(_OnLoadReadWriteVersionListSuccess, _OnLoadReadWriteVersionListFail), null);
                string szReadOnlyVersionListFilePath = CUtility.Path.GetRemotePath(Path.Combine(m_refResMgr.m_szReadOnlyPath, ms_szLocalVersionListFileName));
                m_refResMgr.m_ResourceHelper.LoadBytes(szReadOnlyVersionListFilePath, new CLoadBytesCallbacks(_OnLoadReadOnlyVersionListSuccess, _OnLoadReadOnlyVersionListFail), null);

            }

            private void _OnLoadRemoteVersionListSuccess(string a_szFileUri, byte[] a_arrBytes, float a_fDuration, object a_oUserData)
            {
                if (m_bRemoteVersionListReady)
                {
                    return;
                }
                MemoryStream memoryStream = null;
                try
                {
                    memoryStream = new MemoryStream(a_arrBytes, false);
                    SUpdatableVersionList versionList = m_refResMgr.m_UpdatableVersionSerialize.Deserialize(memoryStream);
                    if (!versionList.IsValid)
                    {
                        return;
                    }
                    SAsset[] arrAsset = versionList.GetAssets();
                    SResource[] arrResource = versionList.GetResources();
                    SFileSystem[] arrFileSystem = versionList.GetFileSystems();
                    SResourceGroup[] arrResGroup = versionList.GetResourceGroups();

                    m_refResMgr.m_szApplicationGameVersion = versionList.ApplicableGameVersion;
                    m_refResMgr.m_nInternalResourceVersion = versionList.InternalResourceVersion;
                    m_refResMgr.m_mapAssetInfo = new Dictionary<string, CAssetInfo>();
                    m_refResMgr.m_mapResourceInfo = new Dictionary<SResourceName, CResourceInfo>();
                    m_refResMgr.m_mapReadWriteResInfo = new SortedDictionary<SResourceName, SReadWriteResourceInfo>();

                    //设置所有资源的fileSysName
                    CResourceGroup defaultResourceGroup = m_refResMgr._GetOrAddResourceGroup(string.Empty);
                    foreach (var fileSys in arrFileSystem)
                    {
                        int[] arrResIdx = fileSys.GetResourceIdxes();
                        foreach (int nResIdx in arrResIdx)
                        {
                            SResource res = arrResource[nResIdx];
                            if (res.Variant != null && res.Variant != m_szCurVariant)
                            {
                                continue;
                            }
                            _SetFileSystemName(new SResourceName(res.Name, res.Variant, res.Extension), fileSys.Name);
                        }
                    }

                    //设置所有资源的远端检查信息
                    foreach (var res in arrResource)
                    {
                        if (res.Variant != null && res.Variant != m_szCurVariant)
                        {
                            continue;
                        }

                        //这里已经把校验需要的数据设置完了
                        SResourceName resName = new SResourceName(res.Name, res.Variant, res.Extension);
                        _SetRemoteInfo(resName, (ELoadType)res.LoadType, res.Len, res.HashCode, res.CompressLen, res.CompressHashCode);

                        //下面是创建了所有的AssetInfo
                        int[] arrAssetIdx = res.GetAssetIdxes();
                        foreach (int nAssetIdx in arrAssetIdx)
                        {
                            SAsset asset = arrAsset[nAssetIdx];
                            int[] arrDependAssetIdx = asset.GetDependAssetIdxes();
                            int nIdx = 0;
                            string[] arrDependAssetName = new string[arrDependAssetIdx.Length];
                            foreach (int nDependAssetIdx in arrDependAssetIdx)
                            {
                                arrDependAssetName[nIdx++] = arrAsset[nDependAssetIdx].Name;
                            }
                            m_refResMgr.m_mapAssetInfo.Add(asset.Name, new CAssetInfo(asset.Name, resName, arrDependAssetName));
                        }

                        defaultResourceGroup.AddResource(resName, res.Len, res.CompressLen);
                    }


                    foreach (var resGroupInfo in arrResGroup)
                    {
                        CResourceGroup resGourp = m_refResMgr._GetOrAddResourceGroup(resGroupInfo.Name);
                        int[] arrResIdx = resGroupInfo.GetResourceIdxes();
                        foreach (int nResIdx in arrResIdx)
                        {
                            SResource res = arrResource[nResIdx];
                            if (res.Variant != null && res.Variant != m_szCurVariant)
                            {
                                continue;
                            }
                            resGourp.AddResource(new SResourceName(res.Name, res.Variant, res.Extension), res.Len, res.CompressLen);
                        }
                    }
                    m_bRemoteVersionListReady = true;
                    _RefreshStatus();
                }
                catch (Exception)
                {
                    throw;
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

            private void _OnLoadRemoteVersionListFail(string a_szFileUri, string a_szErrorMsg, object a_oUserData)
            {
                throw new GameFrameworkException(CUtility.Text.Format("Updatable version list '{0}' is invalid, error message is '{1}'.", a_szFileUri, string.IsNullOrEmpty(a_szErrorMsg) ? "<Empty>" : a_szErrorMsg));

            }

            private void _OnLoadReadWriteVersionListSuccess(string a_szFileUri, byte[] a_arrBytes, float a_fDuration, object a_oUserData)
            {
                if (m_bReadWriteVersionListReady)
                {
                    return;
                }
                MemoryStream memoryStream = null;
                try
                {
                    memoryStream = new MemoryStream(a_arrBytes, false);
                    SLocalVersionList versionList = m_refResMgr.m_ReadWriteVersionSerialize.Deserialize(memoryStream);
                    if (!versionList.IsValid)
                    {
                        return;
                    }
                    SResource[] arrResource = versionList.GetResources();
                    SFileSystem[] arrFileSystem = versionList.GetFileSystems();

                    foreach (var fileSys in arrFileSystem)
                    {
                        int[] arrResIdx = fileSys.GetResourceIdxes();
                        foreach (int nResIdx in arrResIdx)
                        {
                            SResource res = arrResource[nResIdx];
                            _SetFileSystemName(new SResourceName(res.Name, res.Variant, res.Extension), fileSys.Name);
                        }
                    }

                    foreach (var res in arrResource)
                    {
                        _SetReadWriteInfo(new SResourceName(res.Name, res.Variant, res.Extension), (ELoadType)res.LoadType, res.Len, res.HashCode);
                    }
                    m_bReadWriteVersionListReady = true;
                    _RefreshStatus();
                }
                catch
                {
                    throw;
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
                if (m_bReadWriteVersionListReady)
                {
                    return;
                }
                m_bReadWriteVersionListReady = true;
                _RefreshStatus();
            }

            private void _OnLoadReadOnlyVersionListSuccess(string a_szFileUri, byte[] a_arrBytes, float a_fDuration, object a_oUserData)
            {
                if (m_bReadOnlyVersionListReady)
                {
                    return;
                }
                MemoryStream memoryStream = null;
                try
                {
                    memoryStream = new MemoryStream(a_arrBytes, false);
                    SLocalVersionList versionList = m_refResMgr.m_ReadOnlyVersionSerialize.Deserialize(memoryStream);
                    if (!versionList.IsValid)
                    {
                        return;
                    }
                    SResource[] arrResource = versionList.GetResources();
                    SFileSystem[] arrFileSys = versionList.GetFileSystems();

                    foreach (var fileSys in arrFileSys)
                    {
                        int[] arrResIdx = fileSys.GetResourceIdxes();
                        foreach (int nIdx in arrResIdx)
                        {
                            SResource res = arrResource[nIdx];
                            _SetFileSystemName(new SResourceName(res.Name, res.Variant, res.Extension), fileSys.Name);
                        }
                    }

                    foreach (var res in arrResource)
                    {
                        _SetReadOnlyInfo(new SResourceName(res.Name, res.Variant, res.Extension), (ELoadType)res.LoadType, res.Len, res.HashCode);
                    }

                    m_bReadOnlyVersionListReady = true;
                    _RefreshStatus();
                }
                catch
                {
                    throw;
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

            private void _OnLoadReadOnlyVersionListFail(string a_szFileUri, string a_szErrorMsg, object a_oUserData)
            {
                if (m_bReadOnlyVersionListReady)
                {
                    return;
                }
                m_bReadOnlyVersionListReady = true;
                _RefreshStatus();
            }

            private void _RefreshStatus()
            {
                if (!m_bReadOnlyVersionListReady || !m_bReadWriteVersionListReady || !m_bRemoteVersionListReady)
                {
                    return;
                }

                int nMoveCount = 0;
                int nRemoveCount = 0;
                int nUpdateCount = 0;
                long nUpdateLen = 0;
                long nUpdateCompressLen = 0;

                //创建了所有的ResourceInfo
                foreach (var checkInfo in m_mapCheckInfos)
                {
                    CCheckInfo ci = checkInfo.Value;
                    ci.RefreshStatus(m_szCurVariant, m_bIgnoreOtherVariant);
                    if (ci.Status == ECheckStatus.StorageInReadyOnly)
                    {
                        m_refResMgr.m_mapResourceInfo.Add(ci.ResourceName, new CResourceInfo(ci.ResourceName, ci.FileSystemName, ci.LoadType, ci.Len, ci.HashCode, ci.CompressLen, true, true));
                    }
                    else if (ci.Status == ECheckStatus.StorageInReadWrite)
                    {
                        m_refResMgr.m_mapResourceInfo.Add(ci.ResourceName, new CResourceInfo(ci.ResourceName, ci.FileSystemName, ci.LoadType, ci.Len, ci.HashCode, ci.CompressLen, false, true));
                        m_refResMgr.m_mapReadWriteResInfo.Add(ci.ResourceName, new SReadWriteResourceInfo(ci.FileSystemName, ci.LoadType, ci.Len, ci.HashCode));

                        if (ci.NeedMoveToDisk || ci.NeedMoveToFileSystem)
                        {
                            nMoveCount++;
                            string szResourceFullName = ci.ResourceName.FullName;
                            string szResourcePath = CUtility.Path.GetRegularPath(Path.Combine(m_refResMgr.m_szReadWritePath, szResourceFullName));

                            if (ci.NeedMoveToDisk)
                            {
                                IFileSystem fileSystem = m_refResMgr._GetFileSystem(ci.ReadWriteFileSystemName, false);
                                fileSystem.SaveAsFile(szResourceFullName, szResourcePath);
                                fileSystem.DelFile(szResourceFullName);
                            }

                            if (ci.NeedMoveToFileSystem)
                            {
                                IFileSystem fileSystem = m_refResMgr._GetFileSystem(ci.ReadWriteFileSystemName, false);
                                fileSystem.WriteFile(szResourceFullName, szResourcePath);
                                if (File.Exists(szResourcePath))
                                {
                                    File.Delete(szResourcePath);
                                }
                            }
                        }
                    }
                    else if (ci.Status == ECheckStatus.Update)
                    {
                        m_refResMgr.m_mapResourceInfo.Add(ci.ResourceName, new CResourceInfo(ci.ResourceName, ci.FileSystemName, ci.LoadType, ci.Len, ci.HashCode, ci.CompressLen, false, false));
                        nUpdateCount++;
                        nUpdateLen += ci.Len;
                        nUpdateCompressLen += ci.CompressLen;

                        m_OnResNeedUpdate?.Invoke(ci.ResourceName, ci.FileSystemName, ci.LoadType, ci.Len, ci.HashCode, ci.CompressLen, ci.CompressHashCode);

                    }
                    else if (ci.Status == ECheckStatus.Unavailable || ci.Status == ECheckStatus.Disuse)
                    {

                    }
                    else
                    {

                    }

                    if (ci.NeedRemove)
                    {
                        nRemoveCount++;
                        if (ci.ReadWriteUseFileSystem)
                        {
                            IFileSystem fileSys = m_refResMgr._GetFileSystem(ci.ReadWriteFileSystemName, false);
                            fileSys.DelFile(ci.ResourceName.FullName);
                        }
                        else
                        {
                            string szResPath = CUtility.Path.GetRegularPath(Path.Combine(m_refResMgr.m_szReadWritePath, ci.ResourceName.FullName));
                            if (File.Exists(szResPath))
                            {
                                File.Delete(szResPath);
                            }
                        }
                    }

                    if (nMoveCount > 0 || nRemoveCount > 0)
                    {
                        _RemoveEmptyFileSystem();
                        CUtility.Path.RemoveEmptyDirectory(m_refResMgr.m_szReadWritePath);
                    }

                    m_OnCheckComplete?.Invoke(nMoveCount, nRemoveCount, nUpdateCount, nUpdateLen, nUpdateCompressLen);
                }
            }

            private void _RemoveEmptyFileSystem()
            {
                List<string> listRemoveFileSystemName = null;
                foreach (var fileSys in m_refResMgr.m_mapReadWriteFileSystem)
                {
                    if (fileSys.Value.FileCount <= 0)
                    {
                        if (listRemoveFileSystemName == null)
                        {
                            listRemoveFileSystemName = new List<string>();
                        }

                        m_refResMgr.m_refFileSysMgr.DestroyFileSystem(fileSys.Value, true);
                        listRemoveFileSystemName.Add(fileSys.Key);
                    }
                }

                if (listRemoveFileSystemName != null)
                {
                    foreach (string szName in listRemoveFileSystemName)
                    {
                        m_refResMgr.m_mapReadWriteFileSystem.Remove(szName);
                    }
                }
            }

            private void _SetFileSystemName(SResourceName a_resName, string a_szFileSystemName)
            {
                _GetOrAddInfo(a_resName).SetCachedFileSystemName(a_szFileSystemName);
            }

            private void _SetRemoteInfo(SResourceName a_resName, ELoadType a_eLoadType, int a_nLen, int a_nHashCode, int a_nCompressLen, int a_nCompressHashCode)
            {
                _GetOrAddInfo(a_resName).SetRemoteInfo(a_eLoadType, a_nLen, a_nHashCode, a_nCompressLen, a_nCompressHashCode);
            }

            private void _SetReadOnlyInfo(SResourceName a_resName, ELoadType a_eLoadType, int a_nLen, int a_nHashCode)
            {
                _GetOrAddInfo(a_resName).SetReadOnlyInfo(a_eLoadType, a_nLen, a_nHashCode);
            }

            private void _SetReadWriteInfo(SResourceName a_resName, ELoadType a_eLoadType, int a_nLen, int a_nHashCode)
            {
                _GetOrAddInfo(a_resName).SetReadWriteInfo(a_eLoadType, a_nLen, a_nHashCode);
            }

            private CCheckInfo _GetOrAddInfo(SResourceName a_resName)
            {
                if (!m_mapCheckInfos.ContainsKey(a_resName))
                {
                    CCheckInfo checkInfo = new CCheckInfo(a_resName);
                    m_mapCheckInfos.Add(a_resName, checkInfo);
                }

                return m_mapCheckInfos[a_resName];
            }
        }
    }
}
