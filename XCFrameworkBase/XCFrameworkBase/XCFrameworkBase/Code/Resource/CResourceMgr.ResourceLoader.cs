using System;
using System.Collections.Generic;
using System.IO;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr : CGameFrameworkModule, IResourceMgr
    {
        private sealed partial class CResourceLoader
        {
            private const int mc_nCachedHashBytesLen = 4;

            private readonly CResourceMgr m_refResMgr;
            private readonly Dictionary<object, int> m_mapAssetDependencyCount;
            private readonly Dictionary<object, int> m_mapResourceDependencyCount;
            private readonly Dictionary<object, object> m_mapAssetToResource;
            private readonly Dictionary<string, object> m_mapSceneToAsset;

            private readonly CLoadBytesCallbacks m_LoadBytesCallbacks;
            private readonly byte[] m_cacheHashBytes;

            private CTaskPool<CLoadResourceTaskBase> m_taskPool;
            private IObjectPool<CAssetObject> m_AssetPool;
            private IObjectPool<CResourceObj> m_ResourcePool;

            public CResourceLoader(CResourceMgr a_refResMgr)
            {
                m_refResMgr = a_refResMgr;
                m_mapAssetDependencyCount = new Dictionary<object, int>();
                m_mapResourceDependencyCount = new Dictionary<object, int>();
                m_mapAssetToResource = new Dictionary<object, object>();
                m_mapSceneToAsset = new Dictionary<string, object>();
                m_cacheHashBytes = new byte[mc_nCachedHashBytesLen];
                m_taskPool = new CTaskPool<CLoadResourceTaskBase>();
                m_LoadBytesCallbacks = new CLoadBytesCallbacks(_OnLoadBinarySuccess, _OnLoadBinaryFail);
                m_AssetPool = null;
                m_ResourcePool = null;
            }

            public void SetObjectPoolMgr(IObjPoolMgr a_refObjectPoolMgr)
            {
                m_AssetPool = a_refObjectPoolMgr.CreateMultiSpawnObjPool<CAssetObject>("Asset Pool");
                m_ResourcePool = a_refObjectPoolMgr.CreateMultiSpawnObjPool<CResourceObj>("Resource Pool");
            }

            public void AddLoadResourceAgentHelper(ILoadResourceAgentHelper a_refAgentHelper, IResourceHelper a_refResourceHelper, string a_szReadOnlyPath, string a_szReadWritePath, DecrptResourceCallback a_fnDecrptCallback)
            {
                CLoadResourceAgent agent = new CLoadResourceAgent(a_refAgentHelper, a_refResourceHelper, this, a_szReadOnlyPath, a_szReadWritePath, a_fnDecrptCallback);
                m_taskPool.AddAgent(agent);
            }

            public void Shutdown()
            {
                m_taskPool.Shutdown();
                m_mapAssetDependencyCount.Clear();
                m_mapAssetToResource.Clear();
                m_mapResourceDependencyCount.Clear();
                m_mapSceneToAsset.Clear();
                CLoadResourceAgent.Clear();
            }

            public void Update(float a_fElapseSed, float a_fRealElapseSed)
            {
                m_taskPool.Update(a_fElapseSed, a_fRealElapseSed);
            }

            public void LoadAsset(string a_szAssetName, Type a_tAsset, CLoadAssetCallbacks a_LoadCallbacks, int a_nPriority, object a_oUserData)
            {
                CResourceInfo resourceInfo = null;
                string[] arrDependencyAssetNames = null;
                if (!_CheckAsset(a_szAssetName, out resourceInfo, out arrDependencyAssetNames))
                {
                    string szErrorMsg = CUtility.Text.Format("Can not Load Asset {0}", a_szAssetName);
                    ELoadResStatus eStatus = resourceInfo != null && !resourceInfo.Ready ? ELoadResStatus.NotReady : ELoadResStatus.NotExist;
                    a_LoadCallbacks.OnFail?.Invoke(a_szAssetName, eStatus, szErrorMsg, a_oUserData);
                }

                if (resourceInfo.IsLoadFromBinary)
                {
                    string szErrorMsg = CUtility.Text.Format("Can not Load Asset {0} which is a binary asset.", a_szAssetName);
                    ELoadResStatus eStatus = ELoadResStatus.TypeError;
                    a_LoadCallbacks.OnFail?.Invoke(a_szAssetName, eStatus, szErrorMsg, a_oUserData);
                }

                CLoadAssetTask mainTask = CLoadAssetTask.Create(a_szAssetName, a_tAsset, resourceInfo, arrDependencyAssetNames, a_LoadCallbacks, a_oUserData, a_nPriority);
                foreach (string szDependencyAssetName in arrDependencyAssetNames)
                {
                    if (!_LoadDependencyAsset(szDependencyAssetName, mainTask, a_nPriority, a_oUserData))
                    {
                        string szErrorMsg = CUtility.Text.Format("Can not load dependency asset '{0}' when load asset '{1}'.", szDependencyAssetName, a_szAssetName);
                        ELoadResStatus eStatus = ELoadResStatus.DependencyError;
                        a_LoadCallbacks.OnFail?.Invoke(a_szAssetName, eStatus, szErrorMsg, a_oUserData);
                    }
                }

                m_taskPool.AddTask(mainTask);
                if (!resourceInfo.Ready)
                {
                    m_refResMgr._UpdateResource(resourceInfo.ResourceName);
                }
            }

            public void UnloadAsset(object a_refAsset)
            {
                m_AssetPool.UnSpawn(a_refAsset);
            }

            public void LoadScene(string a_szSceneName, CLoadSceneCallbacks a_LoadCallbacks, int a_nPriority, object a_oUserData)
            {
                CResourceInfo resourceInfo = null;
                string[] arrDependencyAssetNames = null;
                if (!_CheckAsset(a_szSceneName, out resourceInfo, out arrDependencyAssetNames))
                {
                    string szErrorMsg = CUtility.Text.Format("Can not Load Scene {0}", a_szSceneName);
                    ELoadResStatus eStatus = resourceInfo != null && !resourceInfo.Ready ? ELoadResStatus.NotReady : ELoadResStatus.NotExist;
                    a_LoadCallbacks.OnFail?.Invoke(a_szSceneName, eStatus, szErrorMsg, a_oUserData);
                }

                if (resourceInfo.IsLoadFromBinary)
                {
                    string szErrorMsg = CUtility.Text.Format("Can not Load Scene {0} which is a binary asset.", a_szSceneName);
                    ELoadResStatus eStatus = ELoadResStatus.TypeError;
                    a_LoadCallbacks.OnFail?.Invoke(a_szSceneName, eStatus, szErrorMsg, a_oUserData);
                }

                CLoadSceneTask mainTask = CLoadSceneTask.Create(a_szSceneName, resourceInfo, arrDependencyAssetNames, a_LoadCallbacks, a_oUserData, a_nPriority);
                foreach (string szDependencyAssetName in arrDependencyAssetNames)
                {
                    if (!_LoadDependencyAsset(szDependencyAssetName, mainTask, a_nPriority, a_oUserData))
                    {
                        string szErrorMsg = CUtility.Text.Format("Can not load dependency asset '{0}' when load Scene '{1}'.", szDependencyAssetName, a_szSceneName);
                        ELoadResStatus eStatus = ELoadResStatus.DependencyError;
                        a_LoadCallbacks.OnFail?.Invoke(a_szSceneName, eStatus, szErrorMsg, a_oUserData);
                    }
                }

                m_taskPool.AddTask(mainTask);
                if (!resourceInfo.Ready)
                {
                    m_refResMgr._UpdateResource(resourceInfo.ResourceName);
                }
            }

            public void UnloadScene(string a_szSceneName, CUnloadSceneCallbacks a_fnUnloadCallbacks, object a_oUserData)
            {
                object asset = null;
                if (m_mapSceneToAsset.TryGetValue(a_szSceneName, out asset))
                {
                    m_mapSceneToAsset.Remove(a_szSceneName);
                    m_AssetPool.UnSpawn(asset);
                    m_AssetPool.ReleaseObject(asset);
                }

                m_refResMgr.m_ResourceHelper.UnloadScene(a_szSceneName, a_fnUnloadCallbacks, a_oUserData);
            }

            public void LoadBinary(string a_szBinaryAssetName, CLoadBinaryCallbacks a_LoadCallbacks, object a_oUserData)
            {
                CResourceInfo resourceInfo = _GetResourceInfo(a_szBinaryAssetName);
                if (null == resourceInfo)
                {
                    string szErrorMsg = CUtility.Text.Format("Can not load binary  '{0}' which is not exist", a_szBinaryAssetName);
                    ELoadResStatus eStatus = ELoadResStatus.NotExist;
                    a_LoadCallbacks.OnFail?.Invoke(a_szBinaryAssetName, eStatus, szErrorMsg, a_oUserData);
                }

                if (!resourceInfo.Ready)
                {
                    string szErrorMsg = CUtility.Text.Format("Can not load binary  '{0}' which is not ready", a_szBinaryAssetName);
                    ELoadResStatus eStatus = ELoadResStatus.NotReady;
                    a_LoadCallbacks.OnFail?.Invoke(a_szBinaryAssetName, eStatus, szErrorMsg, a_oUserData);
                }

                if (!resourceInfo.IsLoadFromBinary)
                {
                    string szErrorMsg = CUtility.Text.Format("Can not load binary  '{0}' which is TypeError", a_szBinaryAssetName);
                    ELoadResStatus eStatus = ELoadResStatus.TypeError;
                    a_LoadCallbacks.OnFail?.Invoke(a_szBinaryAssetName, eStatus, szErrorMsg, a_oUserData);
                }

                if (resourceInfo.UseFileSystem)
                {
                    a_LoadCallbacks.OnSuccess.Invoke(a_szBinaryAssetName, LoadBinaryFromFileSystem(a_szBinaryAssetName), 0f, a_oUserData);
                }
                else
                {
                    string szRootPath = resourceInfo.StorageInReadOnly ? m_refResMgr.m_szReadOnlyPath : m_refResMgr.m_szReadWritePath;
                    string szFullPath = CUtility.Path.GetRegularPath(Path.Combine(szRootPath, resourceInfo.ResourceName.FullName));
                    CLoadBinaryInfo info = CLoadBinaryInfo.Create(a_szBinaryAssetName, resourceInfo, a_LoadCallbacks, a_oUserData);
                    m_refResMgr.m_ResourceHelper.LoadBytes(szFullPath, m_LoadBytesCallbacks, info);
                }
            }

            public byte[] LoadBinaryFromFileSystem(string a_szBinaryAssetName)
            {
                CResourceInfo resourceInfo = _GetResourceInfo(a_szBinaryAssetName);
                if (null == resourceInfo)
                {
                    return null;
                }
                if (!resourceInfo.Ready)
                {
                    return null;
                }

                if (!resourceInfo.IsLoadFromBinary)
                {
                    return null;
                }
                if (!resourceInfo.UseFileSystem)
                {
                    return null;
                }

                IFileSystem fileSystem = m_refResMgr._GetFileSystem(resourceInfo.FileSystemName, resourceInfo.StorageInReadOnly);
                byte[] bytes = fileSystem.ReadFile(resourceInfo.ResourceName.FullName);
                if (null == bytes)
                {
                    return null;
                }

                if (resourceInfo.LoadType == ELoadType.LoadFromBinaryAndQuickDecrypt
                    || resourceInfo.LoadType == ELoadType.LoadFromBinaryAndDecrypt)
                {
                    DecrptResourceCallback callback = m_refResMgr.m_fnDecryptResource ?? _DefaultDecryptResourceCallback;
                    callback(bytes, 0, bytes.Length, resourceInfo.ResourceName.Name, resourceInfo.ResourceName.Variant, resourceInfo.ResourceName.Extentsion, resourceInfo.StorageInReadOnly, resourceInfo.FileSystemName, (byte)resourceInfo.LoadType, resourceInfo.Length, resourceInfo.HashCode);
                }

                return bytes;
            }

            public int LoadBinaryFromFileSystem(string a_szBinaryAssetName, byte[] a_refBuff, int a_nStartIdx, int a_nLen)
            {
                CResourceInfo resourceInfo = _GetResourceInfo(a_szBinaryAssetName);
                if (null == resourceInfo)
                {
                    return -1;
                }

                if (!resourceInfo.Ready)
                {
                    return -1;
                }

                if (!resourceInfo.IsLoadFromBinary)
                {
                    return -1;
                }

                if (!resourceInfo.UseFileSystem)
                {
                    return -1;
                }

                IFileSystem fileSystem = m_refResMgr._GetFileSystem(resourceInfo.FileSystemName, resourceInfo.StorageInReadOnly);
                int nBytesRead = fileSystem.ReadFile(resourceInfo.ResourceName.FullName, a_refBuff, a_nStartIdx, a_nLen);
                if (resourceInfo.LoadType == ELoadType.LoadFromBinaryAndDecrypt
                    || resourceInfo.LoadType == ELoadType.LoadFromBinaryAndQuickDecrypt)
                {
                    DecrptResourceCallback callback = m_refResMgr.m_fnDecryptResource ?? _DefaultDecryptResourceCallback;
                    callback(a_refBuff, a_nStartIdx, nBytesRead, resourceInfo.ResourceName.Name, resourceInfo.ResourceName.Variant, resourceInfo.ResourceName.Extentsion, resourceInfo.StorageInReadOnly, resourceInfo.FileSystemName, (byte)resourceInfo.LoadType, resourceInfo.Length, resourceInfo.HashCode);
                }

                return nBytesRead;
            }

            public byte[] LoadBinarySegmentFromFileSystem(string a_szBinaryAssetName, int a_nOffset, int a_nLen)
            {
                CResourceInfo resourceInfo = _GetResourceInfo(a_szBinaryAssetName);
                if (null == resourceInfo)
                {
                    return null;
                }
                if (!resourceInfo.Ready)
                {
                    return null;
                }

                if (!resourceInfo.IsLoadFromBinary)
                {
                    return null;
                }
                if (!resourceInfo.UseFileSystem)
                {
                    return null;
                }
                IFileSystem fileSystem = m_refResMgr._GetFileSystem(resourceInfo.FileSystemName, resourceInfo.StorageInReadOnly);
                byte[] arrBytes = fileSystem.ReadFileSegement(resourceInfo.ResourceName.FullName, a_nOffset, a_nLen);
                if (arrBytes == null)
                {
                    return null;
                }
                if (resourceInfo.LoadType == ELoadType.LoadFromBinaryAndDecrypt || resourceInfo.LoadType == ELoadType.LoadFromBinaryAndQuickDecrypt)
                {
                    DecrptResourceCallback decrptResourceCallback = m_refResMgr.m_fnDecryptResource ?? _DefaultDecryptResourceCallback;
                    decrptResourceCallback(arrBytes, 0, arrBytes.Length, resourceInfo.ResourceName.Name, resourceInfo.ResourceName.Variant, resourceInfo.ResourceName.Extentsion, resourceInfo.StorageInReadOnly, resourceInfo.FileSystemName, (byte)resourceInfo.LoadType, resourceInfo.Length, resourceInfo.HashCode);
                }

                return arrBytes;
            }

            public int LoadBinarySegmentFromFileSystem(string a_szBinaryAssetName, int a_nOffset, byte[] a_refBuffer, int a_nStartIdx, int a_nLen)
            {
                CResourceInfo resourceInfo = _GetResourceInfo(a_szBinaryAssetName);
                if (null == resourceInfo)
                {
                    return -1;
                }
                if (!resourceInfo.Ready)
                {
                    return -1;
                }

                if (!resourceInfo.IsLoadFromBinary)
                {
                    return -1;
                }
                if (!resourceInfo.UseFileSystem)
                {
                    return -1;
                }
                IFileSystem fileSystem = m_refResMgr._GetFileSystem(resourceInfo.FileSystemName, resourceInfo.StorageInReadOnly);
                int nReadBytes = fileSystem.ReadFileSegement(resourceInfo.ResourceName.FullName, a_refBuffer, a_nStartIdx, a_nOffset, a_nLen);
                if (resourceInfo.LoadType == ELoadType.LoadFromBinaryAndDecrypt || resourceInfo.LoadType == ELoadType.LoadFromBinaryAndQuickDecrypt)
                {
                    DecrptResourceCallback decrptResourceCallback = m_refResMgr.m_fnDecryptResource ?? _DefaultDecryptResourceCallback;
                    decrptResourceCallback(a_refBuffer, a_nStartIdx, nReadBytes, resourceInfo.ResourceName.Name, resourceInfo.ResourceName.Variant, resourceInfo.ResourceName.Extentsion, resourceInfo.StorageInReadOnly, resourceInfo.FileSystemName, (byte)resourceInfo.LoadType, resourceInfo.Length, resourceInfo.HashCode);
                }

                return nReadBytes;
            }

            public string GetBinaryPath(string a_szBinaryAssetName)
            {
                CResourceInfo resourceInfo = _GetResourceInfo(a_szBinaryAssetName);
                if (resourceInfo == null)
                {
                    return null;
                }
                if (!resourceInfo.Ready)
                {
                    return null;
                }

                if (!resourceInfo.IsLoadFromBinary)
                {
                    return null;
                }

                if (resourceInfo.UseFileSystem)
                {
                    return null;
                }
                string szRootPath = resourceInfo.StorageInReadOnly ? m_refResMgr.m_szReadOnlyPath : m_refResMgr.m_szReadWritePath;
                return CUtility.Path.GetRegularPath(Path.Combine(szRootPath, resourceInfo.ResourceName.FullName));
            }

            public bool GetBinaryPath(string a_szBinaryAssetName, out bool a_outBStorageInReadOnly, out bool a_outBInFileSystem, out string a_outszRelativePath, out string a_outszFileName)
            {
                a_outBInFileSystem = false;
                a_outBStorageInReadOnly = false;
                a_outszFileName = null;
                a_outszRelativePath = null;

                CResourceInfo resourceInfo = _GetResourceInfo(a_szBinaryAssetName);
                if (resourceInfo == null)
                {
                    return false;
                }
                if (!resourceInfo.Ready)
                {
                    return false;
                }
                if (!resourceInfo.IsLoadFromBinary)
                {
                    return false;
                }

                a_outBStorageInReadOnly = resourceInfo.StorageInReadOnly;

                if (resourceInfo.UseFileSystem)
                {
                    a_outBInFileSystem = true;
                    a_outszRelativePath = CUtility.Text.Format("{0}.{1}", resourceInfo.FileSystemName, ms_szDefaultExtension);
                    a_outszFileName = resourceInfo.ResourceName.FullName;
                }
                else
                {
                    a_outszRelativePath = resourceInfo.ResourceName.FullName;
                }

                return true;
            }

            public int GetBinaryLen(string a_szBinaryAssetName)
            {
                CResourceInfo resourceInfo = _GetResourceInfo(a_szBinaryAssetName);
                if (resourceInfo == null)
                {
                    return -1;
                }
                if (!resourceInfo.Ready)
                {
                    return -1;
                }

                if (!resourceInfo.IsLoadFromBinary)
                {
                    return -1;
                }
                return resourceInfo.Length;
            }

            private void _DefaultDecryptResourceCallback(byte[] a_arrBytes, int a_nStartIdx, int a_nCount, string a_szName, string a_szVariant, string a_szExtentsion, bool a_bStoreInReadOnly, string a_szFileSystem, byte a_eLoadType, int a_nLen, int a_nHashCode)
            {
                CUtility.Converter.GetBytes(a_nHashCode, m_cacheHashBytes);
                switch ((ELoadType)a_eLoadType)
                {
                    case ELoadType.LoadFromBinaryAndQuickDecrypt:
                    case ELoadType.LoadFromMemoryAndQuickDecrypt:
                        CUtility.Encryption.GetQuickSelfXorBytes(a_arrBytes, m_cacheHashBytes);
                        break;
                    case ELoadType.LoadFromBinaryAndDecrypt:
                    case ELoadType.LoadFromMemoryAndDecrypt:
                        CUtility.Encryption.GetSelfXorBytes(a_arrBytes, m_cacheHashBytes);
                        break;
                    default:
                        throw new GameFrameworkException("");
                }
                Array.Clear(m_cacheHashBytes, 0, mc_nCachedHashBytesLen);
            }

            private bool _LoadDependencyAsset(string a_szAssetName, CLoadResourceTaskBase a_refMainTask, int a_nPriority, object a_oUserData)
            {
                if (a_refMainTask == null)
                {
                    return false;
                }

                CResourceInfo resourceInfo = null;
                string[] arrDependcyAssetName = null;
                if (!_CheckAsset(a_szAssetName, out resourceInfo, out arrDependcyAssetName))
                {
                    return false;
                }

                if (resourceInfo.IsLoadFromBinary)
                {
                    return false;
                }

                CLoadDependencyAssetTask dependencyTask = CLoadDependencyAssetTask.Create(a_szAssetName, resourceInfo, arrDependcyAssetName, a_refMainTask, a_nPriority, a_oUserData);
                foreach (string szDependencyAssetName in arrDependcyAssetName)
                {
                    if (!_LoadDependencyAsset(szDependencyAssetName, dependencyTask, a_nPriority, a_oUserData))
                    {
                        return false;
                    }
                }

                m_taskPool.AddTask(dependencyTask);

                if (!resourceInfo.Ready)
                {
                    m_refResMgr._UpdateResource(resourceInfo.ResourceName);
                }

                return true;
            }

            private bool _CheckAsset(string a_szAssetName, out CResourceInfo a_outResourceInfo, out string[] a_outArrDependencyAssetName)
            {
                a_outResourceInfo = null;
                a_outArrDependencyAssetName = null;
                if (string.IsNullOrEmpty(a_szAssetName))
                {
                    return false;
                }

                CAssetInfo assetInfo = m_refResMgr._GetAssetInfo(a_szAssetName);
                if (null == assetInfo)
                {
                    return false;
                }

                a_outResourceInfo = m_refResMgr._GetResourceInfo(assetInfo.ResourceName);
                if (a_outResourceInfo == null)
                {
                    return false;
                }

                a_outArrDependencyAssetName = assetInfo.GetDependencyAssetNames();
                return m_refResMgr.m_eResourceMode == EResourceMode.UpdatableWhilePlaying ? true : a_outResourceInfo.Ready;
            }

            private CResourceInfo _GetResourceInfo(string a_szAssetName)
            {
                if (string.IsNullOrEmpty(a_szAssetName))
                {
                    return null;
                }
                CAssetInfo assetInfo = m_refResMgr._GetAssetInfo(a_szAssetName);
                if (null == assetInfo)
                {
                    return null;
                }
                return m_refResMgr._GetResourceInfo(assetInfo.ResourceName);
            }

            private void _OnLoadBinarySuccess(string a_szFileUri, byte[] a_arrBytes, float a_fDuration, object a_oUserData)
            {
                CLoadBinaryInfo loadBinaryInfo = (CLoadBinaryInfo)a_oUserData;
                if (null == loadBinaryInfo)
                {
                    return;
                }

                CResourceInfo resourceInfo = loadBinaryInfo.ResourceInfo;
                if (resourceInfo.LoadType == ELoadType.LoadFromBinaryAndDecrypt || resourceInfo.LoadType == ELoadType.LoadFromBinaryAndQuickDecrypt)
                {
                    DecrptResourceCallback callback = m_refResMgr.m_fnDecryptResource ?? _DefaultDecryptResourceCallback;
                    callback(a_arrBytes, 0, a_arrBytes.Length, resourceInfo.ResourceName.Name, resourceInfo.ResourceName.Variant, resourceInfo.ResourceName.Extentsion, resourceInfo.StorageInReadOnly, resourceInfo.FileSystemName, (byte)resourceInfo.LoadType, resourceInfo.Length, resourceInfo.HashCode);
                }

                loadBinaryInfo.LoadBinaryCallbacks.OnSuccess(loadBinaryInfo.BinaryAssetName, a_arrBytes, a_fDuration, loadBinaryInfo.UserData);
                CReferencePool.Release(loadBinaryInfo);
            }

            private void _OnLoadBinaryFail(string a_szFileUri, string a_szErrorMsg, object a_oUserData)
            {
                CLoadBinaryInfo loadBinaryInfo = (CLoadBinaryInfo)a_oUserData;
                if (null == loadBinaryInfo)
                {
                    return;
                }
                loadBinaryInfo.LoadBinaryCallbacks.OnFail(loadBinaryInfo.BinaryAssetName, ELoadResStatus.AssetError, a_szErrorMsg, loadBinaryInfo.UserData);
                CReferencePool.Release(loadBinaryInfo);
            }
        }
    }
}
