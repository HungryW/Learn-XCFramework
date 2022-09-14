using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr : CGameFrameworkModule, IResourceMgr
    {
        private sealed partial class CResourceLoader
        {
            private sealed class CLoadAssetTask : CLoadResourceTaskBase
            {
                private CLoadAssetCallbacks m_LoadCallbacks;

                public CLoadAssetTask()
                {
                    m_LoadCallbacks = null;
                }

                public override bool IsScene => false;

                public override void Clear()
                {
                    base.Clear();
                    m_LoadCallbacks = null;
                }

                public override void OnLoadAssetSuccess(CLoadResourceAgent a_refAgent, object a_oAsset, float a_fDuration)
                {
                    base.OnLoadAssetSuccess(a_refAgent, a_oAsset, a_fDuration);
                    m_LoadCallbacks.OnSuccess?.Invoke(AssetName, a_oAsset, a_fDuration, UserData);
                }

                public override void OnLoadAssetFailure(CLoadResourceAgent a_refAgent, ELoadResStatus a_eStatus, string a_szErrorMsg)
                {
                    base.OnLoadAssetFailure(a_refAgent, a_eStatus, a_szErrorMsg);
                    m_LoadCallbacks.OnFail?.Invoke(AssetName, a_eStatus, a_szErrorMsg, UserData);
                }

                public override void OnLoadAssetUpdate(CLoadResourceAgent a_refAgent, ELoadResProgress a_eProgress, float a_fProgress)
                {
                    base.OnLoadAssetUpdate(a_refAgent, a_eProgress, a_fProgress);
                    m_LoadCallbacks.OnUpdate?.Invoke(AssetName, a_fProgress, UserData);
                }

                public override void OnLoadDependencyAsset(CLoadResourceAgent a_refAgent, string a_szDependencyAssetName, object a_oDependencyObj)
                {
                    base.OnLoadDependencyAsset(a_refAgent, a_szDependencyAssetName, a_oDependencyObj);
                    m_LoadCallbacks.OnDependencyLoad?.Invoke(AssetName, a_szDependencyAssetName, LoadedDependencyAssetCount, TotalDependencyAssetCount, UserData);
                }

                public static CLoadAssetTask Create(string a_szAssetName, Type a_tAssetType, CResourceInfo a_refResourceInfo, string[] a_arrDependencyAssetNames, CLoadAssetCallbacks a_fnLoadAssetCallbacks, object a_oUserData, int a_nPriority)
                {
                    CLoadAssetTask task = CReferencePool.Acquire<CLoadAssetTask>();
                    task.Init(a_szAssetName, a_tAssetType, a_refResourceInfo, a_arrDependencyAssetNames, a_nPriority, a_oUserData);
                    task.m_LoadCallbacks = a_fnLoadAssetCallbacks;
                    return task;
                }
            }
        }
    }
}
