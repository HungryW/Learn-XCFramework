using System;
using System.Collections.Generic;
using System.IO;

namespace XCFrameworkBase
{
    public sealed class CFileSystemMgr : CGameFrameworkModule, IFileSystemMgr
    {
        private Dictionary<string, CFileSystem> m_mapFileSys;

        private IFileSystmeHelper m_refFileSysHelper;

        public CFileSystemMgr()
        {
            m_mapFileSys = new Dictionary<string, CFileSystem>();
            m_refFileSysHelper = null;
        }

        public override int Priority => 4;
        public int Count => m_mapFileSys.Count;

        public void SetFileSystemHelper(IFileSystmeHelper a_refHelper)
        {
            System.Diagnostics.Debug.Assert(a_refHelper != null);

            m_refFileSysHelper = a_refHelper;
        }

        public bool HasFileSystem(string a_szFullPath)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(a_szFullPath));
            return m_mapFileSys.ContainsKey(CUtility.Path.GetRegularPath(a_szFullPath));
        }

        public IFileSystem GetFileSystem(string a_szFullPath)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(a_szFullPath));

            CFileSystem fileSys = null;
            m_mapFileSys.TryGetValue(CUtility.Path.GetRegularPath(a_szFullPath), out fileSys);
            return fileSys;
        }

        public IFileSystem CreateFileSystem(string a_szFullPath, EFileSystemAccess a_eAccess, int a_nMaxFile, int a_nMaxBlock)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(a_szFullPath));
            System.Diagnostics.Debug.Assert(a_eAccess == EFileSystemAccess.ReadWrite || a_eAccess == EFileSystemAccess.Write);

            string szFulPath = CUtility.Path.GetRegularPath(a_szFullPath);
            System.Diagnostics.Debug.Assert(!m_mapFileSys.ContainsKey(szFulPath));

            CFileSystemStream stream = m_refFileSysHelper.CreateFileSystemStream(szFulPath, a_eAccess, true);
            System.Diagnostics.Debug.Assert(stream != null);

            CFileSystem fileSys = CFileSystem.Create(szFulPath, a_eAccess, stream, a_nMaxFile, a_nMaxBlock);
            m_mapFileSys.Add(szFulPath, fileSys);
            return fileSys;
        }


        public IFileSystem LoadFileSystem(string a_szFullPath, EFileSystemAccess a_eAccess)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(a_szFullPath));

            string szFullPath = CUtility.Path.GetRegularPath(a_szFullPath);
            System.Diagnostics.Debug.Assert(!m_mapFileSys.ContainsKey(szFullPath));

            CFileSystemStream stream = m_refFileSysHelper.CreateFileSystemStream(a_szFullPath, a_eAccess, false);
            if (null == stream)
            {
                return null;
            }

            CFileSystem fileSystem = CFileSystem.Load(szFullPath, a_eAccess, stream);
            if (fileSystem == null)
            {
                stream.Close();
                return null;
            }

            m_mapFileSys.Add(szFullPath, fileSystem);
            return fileSystem;
        }

        public void DestroyFileSystem(IFileSystem a_fileSys, bool a_bDelPhysicalFile)
        {
            if (a_fileSys == null)
            {
                return;
            }

            string szFullPath = a_fileSys.FullPath;
            ((CFileSystem)a_fileSys).Shutdown();
            m_mapFileSys.Remove(szFullPath);

            if (a_bDelPhysicalFile && File.Exists(szFullPath))
            {
                File.Delete(szFullPath);
            }
        }

        public override void Update(float a_fElapseSed, float a_fRealElapseSed)
        {
        }

        public override void Shutdown()
        {
            foreach (var sys in m_mapFileSys)
            {
                ((CFileSystem)sys.Value).Shutdown();
            }

            m_mapFileSys.Clear();
        }
    }
}
