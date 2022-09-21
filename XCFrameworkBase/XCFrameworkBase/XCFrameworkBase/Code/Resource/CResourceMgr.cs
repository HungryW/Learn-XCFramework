using System;
using System.Collections.Generic;
using System.IO;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr : CGameFrameworkModule, IResourceMgr
    {
        private const string ms_szRemoteVersionListFileName = "GameFrameworkVersion.dat";
        private const string ms_szLocalVersionListFileName = "GameFrameworkList.dat";
        private const string ms_szDefaultExtension = "dat";
        private const string ms_szTempExtension = "tmp";

        private Dictionary<string, CAssetInfo> m_mapAssetInfo;
        private Dictionary<SResourceName, CResourceInfo> m_mapResourceInfo;
        private SortedDictionary<SResourceName, SReadWriteResourceInfo> m_mapReadWriteResInfo;

        //private Dictionary<string, IFileSystem> m_mapReadOnlyFileSystem;
        private Dictionary<string, IFileSystem> m_mapReadWriteFileSystem;

        private string m_szApplicationGameVersion;
        private int m_nInternalResourceVersion;

        private string m_szReadOnlyPath;
        private string m_szReadWritePath;
        private string m_szUpdatePrefixUri;

        private MemoryStream m_streamCached;

        private EResourceMode m_eResourceMode;

        private IFileSystemMgr m_refFileSysMgr;
        private IResourceHelper m_ResourceHelper;

        private DecrptResourceCallback m_fnDecryptResource;

        private CPackageVersionListSerialize m_PackageVersionSerialize;
        private CUpdatableVersionListSerializer m_UpdatableVersionSerialize;
        private CReadWriteVersionListSerializer m_ReadWriteVersionSerialize;
        private CReadOnlyVersionListSerializer m_ReadOnlyVersionSerialize;

        public CResourceMgr()
        {
            m_szReadOnlyPath = null;
            m_szReadWritePath = null;
            m_eResourceMode = EResourceMode.Updatable;
            m_fnDecryptResource = null;
            m_ResourceHelper = null;
            m_PackageVersionSerialize = null;
            m_szUpdatePrefixUri = null;
            m_UpdatableVersionSerialize = null;
            m_ReadWriteVersionSerialize = null;
            //m_mapReadOnlyFileSystem = null;
            m_mapReadWriteFileSystem = null;
            m_refFileSysMgr = null;
            m_ReadOnlyVersionSerialize = null;
        }

        private CResourceGroup _GetOrAddResourceGroup(string a_szResourceGroupName)
        {
            return null;
        }

        private void _UpdateResource(SResourceName a_ResourceName)
        {

        }

        private CAssetInfo _GetAssetInfo(string a_szAssetName)
        {
            return null;
        }

        private CResourceInfo _GetResourceInfo(SResourceName a_szResourceName)
        {
            return null;
        }

        public override int Priority => base.Priority;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void Shutdown()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override void Update(float a_fElapseSed, float a_fRealElapseSed)
        {
            throw new NotImplementedException();
        }

        private IFileSystem _GetFileSystem(string a_szFileSystemName, bool a_bStorageInReadOnly)
        {
            return null;
        }

        private void _PrepareCacheStream()
        {
            if (m_streamCached == null)
            {
                m_streamCached = new MemoryStream();
            }
            m_streamCached.Position = 0L;
            m_streamCached.SetLength(0L);
        }

        private void _FreeChachStream()
        {
            if (m_streamCached != null)
            {
                m_streamCached.Dispose();
                m_streamCached = null;
            }
        }
    }
}
