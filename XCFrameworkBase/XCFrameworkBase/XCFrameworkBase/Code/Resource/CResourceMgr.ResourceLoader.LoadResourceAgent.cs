using System;
using System.Collections.Generic;
using System.IO;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr : CGameFrameworkModule, IResourceMgr
    {
        public delegate void DecrptResourceCallback(byte[] bytes, int a_nStartIdx, int a_nCount
                                                    , string a_szName, string a_szVariant, string a_szExtension
                                                    , bool a_bStroageInReadOnly, string a_szFileSystemName, byte a_eLoadType, int a_nLen, int a_nHashCode);

        private sealed partial class CResourceLoader
        {
            private abstract class CLoadResourceAgent : ITaskAgent<CLoadResourceTaskBase>
            {

                private static readonly HashSet<string> ms_setLoadingAssetNames = new HashSet<string>(StringComparer.Ordinal);
                private static readonly HashSet<string> ms_setLoadingResourceNames = new HashSet<string>(StringComparer.Ordinal);
                private static readonly Dictionary<string, string> ms_mapCachedResourceNames = new Dictionary<string, string>();

                private readonly ILoadResourceAgentHelper m_refAgentHelper;
                private readonly IResourceHelper m_refResourceHelper;
                private readonly CResourceLoader m_refResourceLoader;
                private readonly string m_szReadonlyPath;
                private readonly string m_szReadWritePath;
                private readonly DecrptResourceCallback m_fnDecryptResourceCallback;
                private CLoadResourceTaskBase m_refTask;

                public CLoadResourceAgent(ILoadResourceAgentHelper a_refAgentHelper, IResourceHelper a_refResHelper, CResourceLoader a_refResLoader, string a_szReadonlyPath, string a_sReadWritePath, DecrptResourceCallback a_fnOnDecryptRes)
                {
                    m_refAgentHelper = a_refAgentHelper;
                    m_refResourceHelper = a_refResHelper;
                    m_refResourceLoader = a_refResLoader;
                    m_szReadonlyPath = a_szReadonlyPath;
                    m_szReadWritePath = a_sReadWritePath;
                    m_fnDecryptResourceCallback = a_fnOnDecryptRes;
                    m_refTask = null;
                }

                public void Init()
                {
                    m_refAgentHelper.OnUpdate += _OnLoadResourceAgentHelperUpdate;
                    m_refAgentHelper.OnReadFileComplete += _OnLoadResoureAgentHelperReadFileComplete;
                    m_refAgentHelper.OnReadBytesComplete += _OnLoadResoureAgentHelperReadBytesComplete;
                    m_refAgentHelper.OnParseBytesComplete += _OnLoadResourceAgentHelperParseBytesComplete;
                    m_refAgentHelper.OnLoadComplete += _OnLoadResourceAgentHelperLoadComplete;
                    m_refAgentHelper.OnLoadFail += _OnLoadResourceAgentHelperError;
                }

                public void ShutDown()
                {
                    Reset();
                    m_refAgentHelper.OnUpdate -= _OnLoadResourceAgentHelperUpdate;
                    m_refAgentHelper.OnReadFileComplete -= _OnLoadResoureAgentHelperReadFileComplete;
                    m_refAgentHelper.OnReadBytesComplete -= _OnLoadResoureAgentHelperReadBytesComplete;
                    m_refAgentHelper.OnParseBytesComplete -= _OnLoadResourceAgentHelperParseBytesComplete;
                    m_refAgentHelper.OnLoadComplete -= _OnLoadResourceAgentHelperLoadComplete;
                    m_refAgentHelper.OnLoadFail -= _OnLoadResourceAgentHelperError;
                }

                public void Reset()
                {
                    m_refAgentHelper.Reset();
                    m_refTask = null;
                }

                public void Update(float elapseSeconds, float realElapseSeconds)
                {

                }

                public CLoadResourceTaskBase Task => m_refTask;

                public ILoadResourceAgentHelper Helper
                {
                    get
                    {
                        return m_refAgentHelper;
                    }
                }

                public EStartTaskStatus Start(CLoadResourceTaskBase a_task)
                {
                    if (a_task == null)
                    {
                        throw new GameFrameworkException(" Task is invalid");
                    }

                    m_refTask = a_task;
                    m_refTask.StartTime = DateTime.UtcNow;
                    CResourceInfo resourceInfo = m_refTask.ResourceInfo;

                    if (!resourceInfo.Ready)
                    {
                        m_refTask.StartTime = default(DateTime);
                        return EStartTaskStatus.HasToWait;
                    }

                    if (_IsAssetLoading(m_refTask.AssetName))
                    {
                        m_refTask.StartTime = default(DateTime);
                        return EStartTaskStatus.HasToWait;
                    }

                    if (!m_refTask.IsScene)
                    {
                        CAssetObject assetObj = m_refResourceLoader.m_AssetPool.Spawn(m_refTask.AssetName);
                        if (null != assetObj)
                        {
                            _OnAssetReady(assetObj);
                            return EStartTaskStatus.Done;
                        }
                    }

                    foreach (string szDependencyAssetName in m_refTask.GetDependencyAssetNames())
                    {
                        if (!m_refResourceLoader.m_AssetPool.CanSpawn(szDependencyAssetName))
                        {
                            m_refTask.StartTime = default(DateTime);
                            return EStartTaskStatus.HasToWait;
                        }
                    }

                    string szResourceName = resourceInfo.ResourceName.Name;
                    if (_IsResourceLoading(szResourceName))
                    {
                        m_refTask.StartTime = default(DateTime);
                        return EStartTaskStatus.HasToWait;
                    }

                    ms_setLoadingAssetNames.Add(m_refTask.AssetName);

                    CResourceObj resourceObj = m_refResourceLoader.m_ResourcePool.Spawn(szResourceName);
                    if (null != resourceObj)
                    {
                        _OnResourceObjReady(resourceObj);
                        return EStartTaskStatus.CanResume;
                    }

                    ms_setLoadingResourceNames.Add(szResourceName);
                    string szResFullPath = null;
                    if (!ms_mapCachedResourceNames.TryGetValue(szResourceName, out szResFullPath))
                    {
                        string szRootPath = resourceInfo.StorageInReadOnly ? m_szReadonlyPath : m_szReadWritePath;
                        string szFileName = resourceInfo.UseFileSystem ? resourceInfo.FileSystemName : resourceInfo.ResourceName.FullName;
                        szResFullPath = CUtility.Path.GetRegularPath(Path.Combine(szRootPath, szFileName));
                        ms_mapCachedResourceNames.Add(szResourceName, szResFullPath);
                    }

                    if (resourceInfo.LoadType == ELoadType.LoadFormFile)
                    {
                        if (resourceInfo.UseFileSystem)
                        {
                            IFileSystem fileSystem = m_refResourceLoader.m_refResMgr._GetFileSystem(resourceInfo.FileSystemName, resourceInfo.StorageInReadOnly);
                            m_refAgentHelper.ReadFile(fileSystem, resourceInfo.ResourceName.FullName);
                        }
                        else
                        {
                            m_refAgentHelper.ReadFile(szResFullPath);
                        }
                    }
                    else if (resourceInfo.LoadType == ELoadType.LoadFromMemory
                        || resourceInfo.LoadType == ELoadType.LoadFromMemoryAndDecrypt
                        || resourceInfo.LoadType == ELoadType.LoadFromMemoryAndQuickDecrypt)
                    {
                        if (resourceInfo.UseFileSystem)
                        {
                            IFileSystem fileSystem = m_refResourceLoader.m_refResMgr._GetFileSystem(resourceInfo.FileSystemName, resourceInfo.StorageInReadOnly);
                            m_refAgentHelper.ReadBytes(fileSystem, resourceInfo.ResourceName.FullName);
                        }
                        else
                        {
                            m_refAgentHelper.ReadBytes(szResFullPath);
                        }
                    }
                    else
                    {
                        throw new GameFrameworkException(CUtility.Text.Format("Resource Load type {0} is invalid", resourceInfo.LoadType));
                    }

                    return EStartTaskStatus.CanResume;
                }

                private void _OnAssetReady(CAssetObject a_assetObj)
                {
                    m_refAgentHelper.Reset();

                    object asset = a_assetObj.Target;
                    if (m_refTask.IsScene)
                    {
                        m_refResourceLoader.m_mapSceneToAsset.Add(m_refTask.AssetName, asset);
                    }

                    m_refTask.OnLoadAssetSuccess(this, asset, (float)(DateTime.UtcNow - m_refTask.StartTime).TotalSeconds);
                    m_refTask.Done = true;
                }

                private void _OnResourceObjReady(CResourceObj a_resObj)
                {
                    m_refTask.LoadMain(this, a_resObj);
                }

                private void _OnError(ELoadResStatus a_eStatus, string a_szErrorMsg)
                {
                    m_refAgentHelper.Reset();
                    m_refTask.OnLoadAssetFailure(this, a_eStatus, a_szErrorMsg);
                    ms_setLoadingAssetNames.Remove(m_refTask.AssetName);
                    ms_setLoadingResourceNames.Remove(m_refTask.ResourceInfo.ResourceName.Name);
                    m_refTask.Done = true;
                }

                private void _OnLoadResourceAgentHelperUpdate(object sender, CLoadResAgentHelperUpdateEventArgs arg)
                {
                    m_refTask.OnLoadAssetUpdate(this, arg.Type, arg.Progress);
                }

                private void _OnLoadResoureAgentHelperReadFileComplete(object sender, CLoadResAgentHelperReadFileCompleteEventArgs arg)
                {
                    _OnResourceLoadEnd(arg.Resource);
                }

                private void _OnLoadResoureAgentHelperReadBytesComplete(object sender, CLoadResAgentHelperReadBytesCompleteEventArgs arg)
                {
                    byte[] bytes = arg.GetByts();
                    CResourceInfo resInfo = m_refTask.ResourceInfo;
                    if (resInfo.LoadType == ELoadType.LoadFromMemoryAndQuickDecrypt
                        || resInfo.LoadType == ELoadType.LoadFromBinaryAndQuickDecrypt)
                    {
                        m_fnDecryptResourceCallback(bytes, 0, bytes.Length
                            , resInfo.ResourceName.Name, resInfo.ResourceName.Variant, resInfo.ResourceName.Extentsion
                            , resInfo.StorageInReadOnly, resInfo.FileSystemName, (byte)resInfo.LoadType, resInfo.Length, resInfo.HashCode);
                    }

                    m_refAgentHelper.ParseBytes(bytes);
                }

                private void _OnLoadResourceAgentHelperParseBytesComplete(object sender, CLoadResAgentHelperParseBytesCompleteEventArgs arg)
                {
                    _OnResourceLoadEnd(arg.Resource);
                }

                private void _OnResourceLoadEnd(object a_oResource)
                {
                    CResourceObj resObj = CResourceObj.Create(m_refTask.ResourceInfo.ResourceName.Name, a_oResource, m_refResourceLoader, m_refResourceHelper);
                    m_refResourceLoader.m_ResourcePool.Register(resObj, true);
                    ms_setLoadingResourceNames.Remove(m_refTask.ResourceInfo.ResourceName.Name);
                    _OnResourceObjReady(resObj);
                }

                private void _OnLoadResourceAgentHelperLoadComplete(object sender, CLoadResAgentHelperLoadCompleteEventArgs arg)
                {
                    CAssetObject assetObj = null;
                    if (m_refTask.IsScene)
                    {
                        assetObj = m_refResourceLoader.m_AssetPool.Spawn(m_refTask.AssetName);
                    }

                    if (assetObj == null)
                    {
                        List<object> listDependcyAssets = m_refTask.GetDependencyAssets();
                        assetObj = CAssetObject.Create(m_refTask.AssetName, arg.Asset, listDependcyAssets, m_refTask.ResourceObject.Target, m_refResourceHelper, m_refResourceLoader);
                        m_refResourceLoader.m_AssetPool.Register(assetObj, true);
                        m_refResourceLoader.m_mapAssetToRessouce.Add(arg.Asset, m_refTask.ResourceObject.Target);
                        foreach (object dependencyAsset in listDependcyAssets)
                        {
                            object dependencyRes = null;
                            if (m_refResourceLoader.m_mapAssetToRessouce.TryGetValue(dependencyAsset, out dependencyRes))
                            {
                                m_refTask.ResourceObject.AddDependencyRes(dependencyRes);
                            }
                            else
                            {
                                throw new GameFrameworkException("Can not find dependency resource.");
                            }
                        }
                    }
                    ms_setLoadingAssetNames.Remove(m_refTask.AssetName);
                    _OnAssetReady(assetObj);
                }

                private void _OnLoadResourceAgentHelperError(object a_sender, CLoadResAgentHelperErrorEventArgs arg)
                {
                    _OnError(arg.Status, arg.ErrorMsg);
                }

                public static void Clear()
                {
                    ms_mapCachedResourceNames.Clear();
                    ms_setLoadingAssetNames.Clear();
                    ms_setLoadingResourceNames.Clear();
                }

                private static bool _IsAssetLoading(string a_szAssetName)
                {
                    return ms_setLoadingAssetNames.Contains(a_szAssetName);
                }

                private static bool _IsResourceLoading(string a_szResourceName)
                {
                    return ms_setLoadingResourceNames.Contains(a_szResourceName);
                }

            }
        }
    }
}
