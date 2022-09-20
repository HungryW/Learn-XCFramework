using System;
using System.IO;

namespace XCFrameworkBase
{
    public static partial class CUtility
    {
        public static partial class Compression
        {
            public interface ICompressionHelper
            {
                bool Compress(byte[] a_bytes, int a_nOffset, int a_nLen, Stream a_CompressedStream);

                bool Compress(Stream a_Stream, Stream a_CompressedStream);

                bool Decompress(byte[] a_bytes, int a_nOffset, int a_nLen, Stream a_DecompressedStream);

                bool Decompress(Stream a_Stream, Stream a_DecompressedStream);
            }
        }
    }
}
