using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr : CGameFrameworkModule, IResourceMgr
    {
        private sealed partial class CResourceLoader
        {
            private abstract class CLoadResourceTaskBase : CTaskBase
            {
                private static int ms_nIdSeed = 0;

                private string m_szAssetName;
                private Type m_tAssetType;
                private CResourceInfo m_ResourceInfo;
                private string[] m_arrDependencyAssetNames;
                private readonly List<object> m_listDependencyAssets;
                private CResourceObj m_ResourceObj;
                private DateTime m_timeStart;
                private int m_nTotalDependencyAssetCount;

                public CLoadResourceTaskBase()
                {
                    m_szAssetName = null;
                    m_tAssetType = null;
                    m_ResourceInfo = null;
                    m_arrDependencyAssetNames = null;
                    m_listDependencyAssets = new List<object>();
                    m_ResourceObj = null;
                    m_timeStart = default(DateTime);
                    m_nTotalDependencyAssetCount = 0;
                }

                public override void Clear()
                {
                    base.Clear();
                    m_szAssetName = null;
                    m_tAssetType = null;
                    m_ResourceInfo = null;
                    m_arrDependencyAssetNames = null;
                    m_listDependencyAssets.Clear();
                    m_ResourceObj = null;
                    m_timeStart = default(DateTime);
                    m_nTotalDependencyAssetCount = 0;
                }

                protected void Init(string a_szAssetName, Type a_tAsset, CResourceInfo a_refResourceInfo, string[] a_arrDependcyAssetNames, int a_nPriority, object a_userData)
                {
                    Initialize(++ms_nIdSeed, null, a_nPriority, a_userData);
                    m_szAssetName = a_szAssetName;
                    m_tAssetType = a_tAsset;
                    m_ResourceInfo = a_refResourceInfo;
                    m_arrDependencyAssetNames = a_arrDependcyAssetNames;
                }

                public abstract bool IsScene { get; }

                public void LoadMain(CLoadResourceAgent a_refAgent, CResourceObj a_refResourceObj)
                {
                    m_ResourceObj = a_refResourceObj;
                    a_refAgent.Helper.LoadAsset(a_refResourceObj.Target, m_szAssetName, m_tAssetType, IsScene);
                }

                public virtual void OnLoadAssetSuccess(CLoadResourceAgent a_refAgent, object a_oAsset, float a_fDuration)
                {

                }

                public virtual void OnLoadAssetFailure(CLoadResourceAgent a_refAgent, ELoadResStatus a_eStatus, string a_szErrorMsg)
                {

                }

                public virtual void OnLoadAssetUpdate(CLoadResourceAgent a_refAgent, ELoadResProgress a_eProgress, float a_fProgress)
                {

                }

                public virtual void OnLoadDependencyAsset(CLoadResourceAgent a_refAgent, string a_szDependencyAssetName, object a_oDependencyObj)
                {
                    m_listDependencyAssets.Add(a_oDependencyObj);
                }

                public string AssetName
                {
                    get
                    {
                        return m_szAssetName;
                    }
                }

                public Type AssetType
                {
                    get
                    {
                        return m_tAssetType;
                    }
                }

                public CResourceInfo ResourceInfo
                {
                    get
                    {
                        return m_ResourceInfo;
                    }
                }

                public CResourceObj ResourceObject
                {
                    get
                    {
                        return m_ResourceObj;
                    }
                }

                public int LoadedDependencyAssetCount
                {
                    get
                    {
                        return m_listDependencyAssets.Count;
                    }
                }

                public int TotalDependencyAssetCount
                {
                    get
                    {
                        return m_nTotalDependencyAssetCount;
                    }
                    set
                    {
                        m_nTotalDependencyAssetCount = value;
                    }
                }


                public DateTime StartTime
                {
                    get
                    {
                        return m_timeStart;
                    }
                    set
                    {
                        m_timeStart = value;
                    }
                }

                public string[] GetDependencyAssetNames()
                {
                    return m_arrDependencyAssetNames;
                }

                public List<object> GetDependencyAssets()
                {
                    return m_listDependencyAssets;
                }


            }
        }
    }
}
