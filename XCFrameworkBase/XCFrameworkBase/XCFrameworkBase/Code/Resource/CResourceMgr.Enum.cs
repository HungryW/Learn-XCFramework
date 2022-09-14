using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr : CGameFrameworkModule, IResourceMgr
    {
        public enum EHasAssetResult : byte
        {
            NoExist = 0,

            NotReady,
            AssetOnDisk,
            AssetOnFileSystem,
            BinaryOnDisk,
            BinaryOnFileSystem,
        }

        public enum EResourceMode : byte
        {
            UnSpecified = 0,
            Package,
            Updatable,
            UpdatableWhilePlaying,
        }
    }
}
