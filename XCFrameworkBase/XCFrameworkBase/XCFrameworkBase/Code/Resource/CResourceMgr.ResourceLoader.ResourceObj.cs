using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr : CGameFrameworkModule, IResourceMgr
    {
        private sealed partial class CResourceLoader
        {
            /// <summary>
            /// 记录了Res引用的其他Res
            /// 实际对象object作为Key值
            /// 问题是
            /// 为什么和AssetObj 不一样没有 unspawn, 只要Release时引用-1;
            /// 依赖的Res也不是创建时传递的
            /// </summary>
            private sealed class CResourceObj : CObjectBase
            {
                private List<object> m_listDependencyReses;
                private IResourceHelper m_refHelper;
                private CResourceLoader m_refLoader;

                public CResourceObj()
                {
                    m_listDependencyReses = new List<object>();
                    m_refHelper = null;
                    m_refLoader = null;
                }

                public override bool CustomCanReleaseFlag()
                {
                    int nTargetReferenceCount = 0;
                    m_refLoader.m_mapResourceDependencyCount.TryGetValue(Target, out nTargetReferenceCount);
                    return base.CustomCanReleaseFlag() && nTargetReferenceCount <= 0;
                }

                public override void Clear()
                {
                    base.Clear();
                    m_listDependencyReses.Clear();
                    m_refHelper = null;
                    m_refLoader = null;
                }

                public void AddDependencyRes(object a_refRes)
                {
                    if (Target == a_refRes)
                    {
                        return;
                    }
                    if (m_listDependencyReses.Contains(a_refRes))
                    {
                        return;
                    }

                    m_listDependencyReses.Add(a_refRes);

                    if (!m_refLoader.m_mapResourceDependencyCount.ContainsKey(a_refRes))
                    {
                        m_refLoader.m_mapResourceDependencyCount.Add(a_refRes, 0);
                    }
                    m_refLoader.m_mapResourceDependencyCount[a_refRes] += 1;
                }

                protected override void _Release(bool a_isShutDown)
                {
                    if (!a_isShutDown)
                    {
                        int nTargetReferenceCount = 0;
                        if (m_refLoader.m_mapResourceDependencyCount.TryGetValue(Target, out nTargetReferenceCount) && nTargetReferenceCount > 0)
                        {
                            throw new GameFrameworkException(CUtility.Text.Format("Resource target '{0}' reference count is {1} >0", Name, nTargetReferenceCount));
                        }

                        foreach (object dependencyRes in m_listDependencyReses)
                        {
                            if (m_refLoader.m_mapAssetDependencyCount.ContainsKey(dependencyRes))
                            {
                                m_refLoader.m_mapAssetDependencyCount[dependencyRes] -= 1;
                            }
                            else
                            {
                                throw new GameFrameworkException(CUtility.Text.Format("Resource target '{0}' dependency asset reference count is invalid.", Name));
                            }
                        }
                    }

                    m_refLoader.m_mapResourceDependencyCount.Remove(Target);
                    m_refHelper.Release(Target);
                }

                public static CResourceObj Create(string a_szName, object a_oTarget, CResourceLoader a_refLoader, IResourceHelper a_refHelper)
                {
                    CResourceObj res = CReferencePool.Acquire<CResourceObj>();
                    res.Init(a_szName, a_oTarget);
                    res.m_refHelper = a_refHelper;
                    res.m_refLoader = a_refLoader;
                    return res;
                }
            }
        }
    }
}
