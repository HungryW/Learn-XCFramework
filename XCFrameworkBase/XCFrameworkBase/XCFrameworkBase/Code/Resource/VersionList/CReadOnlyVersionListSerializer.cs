using System;

namespace XCFrameworkBase
{
    public sealed class CReadOnlyVersionListSerializer : CFrameworkSerializer<SLocalVersionList>
    {
        private static readonly byte[] ms_arrHeader = new byte[] { (byte)'X', (byte)'F', (byte)'R' };

        public CReadOnlyVersionListSerializer()
        {

        }

        protected override byte[] __GetHeader()
        {
            return ms_arrHeader;
        }
    }
}
