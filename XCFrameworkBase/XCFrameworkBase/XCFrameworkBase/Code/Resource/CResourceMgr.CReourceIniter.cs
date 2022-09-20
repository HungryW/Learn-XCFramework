using System;
using System.Collections.Generic;
using System.IO;

namespace XCFrameworkBase
{
    public partial class CResourceMgr
    {
        private sealed class CReourceIniter
        {
            private readonly CResourceMgr m_refResMgr;
            private readonly Dictionary<SResourceName, string> m_mapCacheFileSysNames;
            private string m_szCurVarint;

            public Action m_fnOnComplete;


            public CReourceIniter(CResourceMgr a_refResMgr)
            {
                m_refResMgr = a_refResMgr;
                m_mapCacheFileSysNames = new Dictionary<SResourceName, string>();
                m_szCurVarint = null;
                m_fnOnComplete = null;
            }

            public void Shutdown()
            {

            }

            public void InitResource(string a_szCurrentVariant)
            {
                m_szCurVarint = a_szCurrentVariant;
                string szFileUri = CUtility.Path.GetRemotePath(Path.Combine(m_refResMgr.m_szReadOnlyPath, ms_szRemoteVersionListFileName));
                CLoadBytesCallbacks callback = new CLoadBytesCallbacks(_OnLoadPackageVersionSuccess, _OnLoadPackageVersionFial);
                m_refResMgr.m_ResourceHelper.LoadBytes(szFileUri, callback, null);
            }

            private void _OnLoadPackageVersionSuccess(string a_szFileUri, byte[] a_arrBytes, float a_fnDuration, object a_oUserdata)
            {
                MemoryStream mStream = null;
                try
                {
                    mStream = new MemoryStream(a_arrBytes, false);
                    SPackageVersionList versionList = m_refResMgr.m_PackageVersionSerialize.Deserialize(mStream);
                    if (!versionList.IsValid)
                    {
                        return;
                    }
                    SVersionList.SAsset[] arrAsset = versionList.GetAssets();
                    SVersionList.SResource[] arrResource = versionList.GetResources();
                    SVersionList.SFileSystem[] arrFileSystems = versionList.GetFileSystems();
                    SVersionList.SResourceGroup[] arrResourceGroups = versionList.GetResourceGroups();

                    m_refResMgr.m_nInternalResourceVersion = versionList.InternalResourceVersion;
                    m_refResMgr.m_szApplicationGameVersion = versionList.ApplicableGameVersion;
                    m_refResMgr.m_mapAssetInfo = new Dictionary<string, CAssetInfo>();
                    m_refResMgr.m_mapResourceInfo = new Dictionary<SResourceName, CResourceInfo>();

                    CResourceGroup defaultResGroup = m_refResMgr._GetOrAddResourceGroup(string.Empty);

                    foreach (var fileSys in arrFileSystems)
                    {
                        int[] arrResIdx = fileSys.GetResourceIdxes();
                        foreach (int nResIdx in arrResIdx)
                        {
                            SVersionList.SResource resource = arrResource[nResIdx];
                            if (resource.Variant != null && resource.Variant != m_szCurVarint)
                            {
                                continue;
                            }
                            SResourceName resName = new SResourceName(resource.Name, resource.Variant, resource.Extension);
                            m_mapCacheFileSysNames.Add(resName, fileSys.Name);
                        }
                    }

                    foreach (var res in arrResource)
                    {
                        if (res.Variant != null && res.Variant != m_szCurVarint)
                        {
                            continue;
                        }
                        SResourceName resName = new SResourceName(res.Name, res.Variant, res.Extension);

                        int[] arrAssetIdxs = res.GetAssetIdxes();
                        foreach (int nIdx in arrAssetIdxs)
                        {
                            SVersionList.SAsset asset = arrAsset[nIdx];
                            int[] arrDependAssetIdx = asset.GetDependAssetIdxes();
                            string[] arrDependAssetName = new string[arrDependAssetIdx.Length];
                            for (int i = 0; i < arrDependAssetIdx.Length; i++)
                            {
                                SVersionList.SAsset dependAsset = arrAsset[nIdx];
                                arrDependAssetName[i] = dependAsset.Name;
                            }

                            CAssetInfo assetInfo = new CAssetInfo(asset.Name, resName, arrDependAssetName);
                            m_refResMgr.m_mapAssetInfo.Add(asset.Name, assetInfo);
                        }

                        string szFileSysName = null;
                        m_mapCacheFileSysNames.TryGetValue(resName, out szFileSysName);

                        CResourceInfo resInfo = new CResourceInfo(resName, szFileSysName, (ELoadType)res.LoadType, res.Len, res.HashCode, res.Len, true, true);
                        m_refResMgr.m_mapResourceInfo.Add(resName, resInfo);
                        defaultResGroup.AddResource(resName, res.Len, res.Len);
                    }

                    foreach (var resGroupinfo in arrResourceGroups)
                    {
                        CResourceGroup group = m_refResMgr._GetOrAddResourceGroup(resGroupinfo.Name);
                        int[] arrRerIdxs = resGroupinfo.GetResourceIdxes();
                        foreach (int nIdx in arrRerIdxs)
                        {
                            SVersionList.SResource resource = arrResource[nIdx];
                            if (resource.Variant != null && resource.Variant != m_szCurVarint)
                            {
                                continue;
                            }

                            SResourceName resName = new SResourceName(resource.Name, resource.Variant, resource.Extension);
                            group.AddResource(resName, resource.Len, resource.Len);
                        }
                    }

                    m_fnOnComplete();
                }
                catch
                {

                }
                finally
                {
                    m_mapCacheFileSysNames.Clear();
                    if (mStream != null)
                    {
                        mStream.Dispose();
                        mStream = null;
                    }
                }
            }

            private void _OnLoadPackageVersionFial(string a_szFileUri, string a_szErrorMsg, object a_oUserData)
            {

            }
        }
    }

}
