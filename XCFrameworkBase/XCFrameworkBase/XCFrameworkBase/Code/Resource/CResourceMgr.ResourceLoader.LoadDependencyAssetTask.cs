using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr : CGameFrameworkModule, IResourceMgr
    {
        private sealed partial class CResourceLoader
        {
            private sealed class CLoadDependencyAssetTask : CLoadResourceTaskBase
            {
                private CLoadResourceTaskBase m_refMainTask;

                public CLoadDependencyAssetTask()
                {
                    m_refMainTask = null;
                }

                public override bool IsScene => false;

                public override void Clear()
                {
                    base.Clear();
                    m_refMainTask = null;
                }

                public override void OnLoadAssetSuccess(CLoadResourceAgent a_refAgent, object a_oAsset, float a_fDuration)
                {
                    base.OnLoadAssetSuccess(a_refAgent, a_oAsset, a_fDuration);
                    m_refMainTask.OnLoadDependencyAsset(a_refAgent, AssetName, a_oAsset);
                }

                public override void OnLoadAssetFailure(CLoadResourceAgent a_refAgent, ELoadResStatus a_eStatus, string a_szErrorMsg)
                {
                    base.OnLoadAssetFailure(a_refAgent, a_eStatus, a_szErrorMsg);
                    m_refMainTask.OnLoadAssetFailure(a_refAgent, ELoadResStatus.DependencyError, CUtility.Text.Format("Can not load dependency asset '{0}', internal status '{1}', internal error message '{2}'.", AssetName, a_eStatus, a_szErrorMsg));
                }

                public static CLoadDependencyAssetTask Create(string a_szAssetName, CResourceInfo a_refResourceInfo, string[] a_arrDependencyAssetNames, CLoadResourceTaskBase a_refMainTask, int a_nPriority, object a_oUserData)
                {
                    CLoadDependencyAssetTask task = CReferencePool.Acquire<CLoadDependencyAssetTask>();
                    task.Init(a_szAssetName, null, a_refResourceInfo, a_arrDependencyAssetNames, a_nPriority, a_oUserData);
                    task.m_refMainTask = a_refMainTask;
                    return task;
                }
            }
        }
    }
}
