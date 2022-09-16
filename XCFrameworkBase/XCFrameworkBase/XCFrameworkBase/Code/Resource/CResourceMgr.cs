using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr : CGameFrameworkModule, IResourceMgr
    {
        private const string RemoteVersionListFileName = "GameFrameworkVersion.dat";
        private const string DefaultExtension = "dat";

        private Dictionary<string, CAssetInfo> m_mapAssetInfo;
        private Dictionary<SResourceName, CResourceInfo> m_mapResourceInfo;

        private string m_szApplicationGameVersion;
        private int m_nInternalResourceVersion;

        private string m_szReadOnlyPath;
        private string m_szReadWritePath;

        private EResourceMode m_eResourceMode;

        private IResourceHelper m_ResourceHelper;

        private DecrptResourceCallback m_fnDecryptResource;

        private CPackageVersionListSerialize m_PackageVersionSerialize;

        public CResourceMgr()
        {
            m_szReadOnlyPath = null;
            m_szReadWritePath = null;
            m_eResourceMode = EResourceMode.Updatable;
            m_fnDecryptResource = null;
            m_ResourceHelper = null;
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
    }
}
