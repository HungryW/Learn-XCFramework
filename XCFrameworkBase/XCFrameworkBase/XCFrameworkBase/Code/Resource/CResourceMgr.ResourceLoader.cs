using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr : CGameFrameworkModule, IResourceMgr
    {
        private sealed partial class CResourceLoader
        {
            private readonly CResourceMgr m_refResMgr;
            private readonly Dictionary<object, int> m_mapAssetDependencyCount;
            private readonly Dictionary<object, int> m_mapResourceDependencyCount;
            private readonly Dictionary<object, object> m_mapAssetToRessouce;
            private readonly Dictionary<string, object> m_mapSceneToAsset;

            private readonly CLoadBytesCallbacks m_LoadBytesCallbacks;
            private readonly byte[] m_cacheHashBytes;

            private IObjectPool<CAssetObject> m_AssetPool;
            private IObjectPool<CResourceObj> m_ResourcePool;

            public CResourceLoader()
            {
                m_refResMgr = null;
                m_mapAssetDependencyCount = null;
                m_mapResourceDependencyCount = null;
                m_mapAssetToRessouce = null;
                m_mapSceneToAsset = null;
                m_LoadBytesCallbacks = new CLoadBytesCallbacks(null);
                m_cacheHashBytes = new byte[1];
                m_AssetPool = null;
                m_ResourcePool = null;
            }
        }
    }
}
