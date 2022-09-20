using System;

namespace XCFrameworkBase
{
    public sealed class CReadWriteVersionListSerializer : CFrameworkSerializer<SLocalVersionList>
    {
        private static readonly byte[] ms_arrHeader = new byte[] { (byte)'X', (byte)'F', (byte)'W' };

        public CReadWriteVersionListSerializer()
        {

        }

        protected override byte[] __GetHeader()
        {
            return ms_arrHeader;
        }
    }
}
