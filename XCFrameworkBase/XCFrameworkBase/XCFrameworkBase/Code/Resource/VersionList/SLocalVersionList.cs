using System;
using System.Runtime.InteropServices;
using static XCFrameworkBase.SVersionList;

namespace XCFrameworkBase
{

    [StructLayout(LayoutKind.Auto)]
    public struct SLocalVersionList
    {
        private static readonly SResource[] EmptyResourceArray = new SResource[] { };
        private static readonly SFileSystem[] EmptyFileSystemArray = new SFileSystem[] { };

        private readonly bool m_IsValid;
        private readonly SResource[] m_Resources;
        private readonly SFileSystem[] m_FileSystems;

        /// <param name="resources">包含的资源集合。</param>
        /// <param name="fileSystems">包含的文件系统集合。</param>
        public SLocalVersionList(SResource[] resources, SFileSystem[] fileSystems)
        {
            m_IsValid = true;
            m_Resources = resources ?? EmptyResourceArray;
            m_FileSystems = fileSystems ?? EmptyFileSystemArray;
        }

        public bool IsValid
        {
            get
            {
                return m_IsValid;
            }
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

    }
}
