using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr : CGameFrameworkModule, IResourceMgr
    {
        private sealed partial class CResourceLoader
        {
            /// <summary>
            /// AssetObj 主要操作引用计数的数据
            /// 通过父管理器Loader,管理所有引用计数的数据
            /// 创建的时候 所有引用Asset的引用计数+1
            /// 回收的时候 所有引用Asset的引用计数-1
            /// 释放的时候 自身的引用计数要为0, 所有引用Asset的引用计数-1
            /// 通过helper 来做实际的释放
            /// </summary>
            private sealed class CAssetObject : CObjectBase
            {
                private List<object> m_listDependencyAssets;
                private object m_refResource;
                private IResourceHelper m_refResourceHelper;
                private CResourceLoader m_refResourceLoader;

                public CAssetObject()
                {
                    m_listDependencyAssets = new List<object>();
                    m_refResource = null;
                    m_refResourceHelper = null;
                    m_refResourceLoader = null;
                }

                public override bool CustomCanReleaseFlag()
                {
                    int nTargetReferenceCount = 0;
                    m_refResourceLoader.m_mapAssetDependencyCount.TryGetValue(Target, out nTargetReferenceCount);
                    return base.CustomCanReleaseFlag() && nTargetReferenceCount <= 0;
                }

                public override void Clear()
                {
                    base.Clear();
                    m_listDependencyAssets.Clear();
                    m_refResource = null;
                    m_refResourceHelper = null;
                    m_refResourceLoader = null;
                }

                public override void OnUnSpawn()
                {
                    base.OnUnSpawn();
                    foreach (object dependenctyAsset in m_listDependencyAssets)
                    {
                        m_refResourceLoader.m_AssetPool.UnSpawn(dependenctyAsset);
                    }
                }

                protected override void _Release(bool a_isShutDown)
                {
                    if (!a_isShutDown)
                    {
                        int nTargetReferenceCount = 0;
                        if (m_refResourceLoader.m_mapAssetDependencyCount.TryGetValue(Target, out nTargetReferenceCount))
                        {
                            throw new GameFrameworkException(CUtility.Text.Format("Asset target '{0}' reference count is '{1}' > 0", Name, nTargetReferenceCount));
                        }

                        foreach (object dependencyAsset in m_listDependencyAssets)
                        {
                            if (m_refResourceLoader.m_mapAssetDependencyCount.ContainsKey(dependencyAsset))
                            {
                                m_refResourceLoader.m_mapAssetDependencyCount[dependencyAsset] -= 1;
                            }
                            else
                            {
                                throw new GameFrameworkException(CUtility.Text.Format("Asset target '{0}' dependency asset reference count is invalid.", Name));
                            }
                        }

                        m_refResourceLoader.m_ResourcePool.UnSpawn(m_refResource);
                    }

                    m_refResourceLoader.m_mapAssetDependencyCount.Remove(Target);
                    m_refResourceLoader.m_mapAssetToRessouce.Remove(Target);
                    m_refResourceHelper.Release(Target);
                }


                public static CAssetObject Create(string a_szName, object a_oTarget, List<object> a_listDependencyAsset, object a_refRes, IResourceHelper a_helper, CResourceLoader a_refLoader)
                {
                    CAssetObject assetObj = CReferencePool.Acquire<CAssetObject>();
                    assetObj.Init(a_szName, a_oTarget);
                    assetObj.m_listDependencyAssets.AddRange(a_listDependencyAsset);
                    assetObj.m_refResource = a_refRes;
                    assetObj.m_refResourceHelper = a_helper;
                    assetObj.m_refResourceLoader = a_refLoader;

                    foreach (object dependAssets in a_listDependencyAsset)
                    {
                        if (!a_refLoader.m_mapAssetDependencyCount.ContainsKey(dependAssets))
                        {
                            a_refLoader.m_mapAssetDependencyCount.Add(dependAssets, 0);
                        }
                        a_refLoader.m_mapAssetDependencyCount[dependAssets] += 1;
                    }

                    return assetObj;
                }
            }

        }
    }
}
