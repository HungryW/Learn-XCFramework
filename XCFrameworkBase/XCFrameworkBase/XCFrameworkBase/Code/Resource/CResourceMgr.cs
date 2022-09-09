using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr : CGameFrameworkModule, IResourceMgr
    {
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
