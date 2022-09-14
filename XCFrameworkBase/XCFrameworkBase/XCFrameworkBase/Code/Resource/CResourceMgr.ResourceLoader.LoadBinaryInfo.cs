using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr : CGameFrameworkModule, IResourceMgr
    {
        private sealed partial class CResourceLoader
        {
            private sealed class CLoadBinaryInfo : IReference
            {
                private string m_szBinaryAssetName;
                private CResourceInfo m_refResourceInfo;
                private CLoadBinaryCallbacks m_LoadBinaryCallbacks;
                private object m_oUserData;


                public CLoadBinaryInfo()
                {
                    Clear();
                }

                public void Clear()
                {
                    m_szBinaryAssetName = null;
                    m_refResourceInfo = null;
                    m_LoadBinaryCallbacks = null;
                    m_oUserData = null;
                }

                public string BinaryAssetName => m_szBinaryAssetName;
                public CResourceInfo ResourceInfo => m_refResourceInfo;
                public object UserData => m_oUserData;
                public CLoadBinaryCallbacks LoadBinaryCallbacks => m_LoadBinaryCallbacks;

                public static CLoadBinaryInfo Create(string a_szBinaryAssetName, CResourceInfo a_refResourceInfo, CLoadBinaryCallbacks a_LoadBinaryCallbacks, object a_oUserData)
                {
                    CLoadBinaryInfo info = CReferencePool.Acquire<CLoadBinaryInfo>();
                    info.m_szBinaryAssetName = a_szBinaryAssetName;
                    info.m_refResourceInfo = a_refResourceInfo;
                    info.m_LoadBinaryCallbacks = a_LoadBinaryCallbacks;
                    info.m_oUserData = a_oUserData;
                    return info;
                }

            }
        }

    }
}
