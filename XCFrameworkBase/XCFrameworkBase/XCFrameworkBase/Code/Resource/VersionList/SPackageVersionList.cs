using System;
using System.Runtime.InteropServices;
using static XCFrameworkBase.SVersionList;

namespace XCFrameworkBase
{
    [StructLayout(LayoutKind.Auto)]
    public struct SPackageVersionList
    {
        private static readonly SAsset[] EmptyAssetArray = new SAsset[] { };
        private static readonly SResource[] EmptyResourceArray = new SResource[] { };
        private static readonly SFileSystem[] EmptyFileSystemArray = new SFileSystem[] { };
        private static readonly SResourceGroup[] EmptyResourceGroupArray = new SResourceGroup[] { };

        private readonly bool m_IsValid;
        private readonly string m_ApplicableGameVersion;
        private readonly int m_InternalResourceVersion;
        private readonly SAsset[] m_Assets;
        private readonly SResource[] m_Resources;
        private readonly SFileSystem[] m_FileSystems;
        private readonly SResourceGroup[] m_ResourceGroups;

        /// <summary>
        /// 初始化单机模式版本资源列表的新实例。
        /// </summary>
        /// <param name="applicableGameVersion">适配的游戏版本号。</param>
        /// <param name="internalResourceVersion">内部资源版本号。</param>
        /// <param name="assets">包含的资源集合。</param>
        /// <param name="resources">包含的资源集合。</param>
        /// <param name="fileSystems">包含的文件系统集合。</param>
        /// <param name="resourceGroups">包含的资源组集合。</param>
        public SPackageVersionList(string applicableGameVersion, int internalResourceVersion, SAsset[] assets, SResource[] resources, SFileSystem[] fileSystems, SResourceGroup[] resourceGroups)
        {
            m_IsValid = true;
            m_ApplicableGameVersion = applicableGameVersion;
            m_InternalResourceVersion = internalResourceVersion;
            m_Assets = assets ?? EmptyAssetArray;
            m_Resources = resources ?? EmptyResourceArray;
            m_FileSystems = fileSystems ?? EmptyFileSystemArray;
            m_ResourceGroups = resourceGroups ?? EmptyResourceGroupArray;
        }

        public bool IsValid
        {
            get
            {
                return m_IsValid;
            }
        }

        public string ApplicableGameVersion
        {
            get
            {
                if (!m_IsValid)
                {
                    throw new GameFrameworkException("Data is invalid.");
                }

                return m_ApplicableGameVersion;
            }
        }

        public int InternalResourceVersion
        {
            get
            {
                if (!m_IsValid)
                {
                    throw new GameFrameworkException("Data is invalid.");
                }

                return m_InternalResourceVersion;
            }
        }

        public SAsset[] GetAssets()
        {
            if (!m_IsValid)
            {
                throw new GameFrameworkException("Data is invalid.");
            }

            return m_Assets;
        }

        public SResource[] GetResources()
        {
            if (!m_IsValid)
            {
                throw new GameFrameworkException("Data is invalid.");
            }

            return m_Resources;
        }

        public SFileSystem[] GetFileSystems()
        {
            if (!m_IsValid)
            {
                throw new GameFrameworkException("Data is invalid.");
            }

            return m_FileSystems;
        }

        public SResourceGroup[] GetResourceGroups()
        {
            if (!m_IsValid)
            {
                throw new GameFrameworkException("Data is invalid.");
            }

            return m_ResourceGroups;
        }
    }

}
