using System;

namespace XCFrameworkBase
{
    public sealed class CPackageVersionListSerialize : CFrameworkSerializer<SPackageVersionList>
    {
        private static readonly byte[] Header = new byte[] { (byte)'X', (byte)'F', (byte)'P' };

        public CPackageVersionListSerialize()
        {

        }

        protected override byte[] __GetHeader()
        {
            return Header;
        }
    }
}
