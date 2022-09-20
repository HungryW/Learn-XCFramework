using System;

namespace XCFrameworkBase
{
    public sealed class CUpdatableVersionListSerializer : CFrameworkSerializer<SUpdatableVersionList>
    {
        private static readonly byte[] ms_arrHeader = new byte[] { (byte)'X', (byte)'F', (byte)'U' };

        public CUpdatableVersionListSerializer()
        {

        }

        protected override byte[] __GetHeader()
        {
            return ms_arrHeader;
        }
    }
}
