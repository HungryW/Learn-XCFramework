using System;
using System.IO;

namespace XCFrameworkBase
{
    public static partial class CUtility
    {
        public static partial class Compression
        {
            private static ICompressionHelper ms_CompressionHelper = null;

            public static void SetCompressionHelper(ICompressionHelper a_compressionHelper)
            {
                ms_CompressionHelper = a_compressionHelper;
            }

            public static byte[] Compress(byte[] a_bytes)
            {
                return Compress(a_bytes, 0, a_bytes.Length);
            }

            public static byte[] Compress(byte[] a_bytes, int a_nOffset, int a_nLen)
            {
                using (MemoryStream compressedStream = new MemoryStream())
                {
                    if (Compress(a_bytes, a_nOffset, a_nLen, compressedStream))
                    {
                        return compressedStream.ToArray();
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            public static bool Compress(byte[] a_bytes, int a_nOffset, int a_nLen, Stream a_CompressedStream)
            {
                if (ms_CompressionHelper == null)
                {
                    return false;
                }
                if (a_bytes == null)
                {
                    return false;
                }

                if (a_nOffset < 0 || a_nLen <= 0 || a_nOffset + a_nLen > a_bytes.Length)
                {
                    return false;
                }

                if (a_CompressedStream == null)
                {
                    return false;
                }

                try
                {
                    return ms_CompressionHelper.Compress(a_bytes, a_nOffset, a_nLen, a_CompressedStream);
                }
                catch (Exception)
                {
                    throw new Exception("Compress Fail");
                }
            }

            public static byte[] Decompress(byte[] a_bytes)
            {
                return Decompress(a_bytes, 0, a_bytes.Length);
            }

            public static byte[] Decompress(byte[] a_bytes, int a_nOffset, int a_nLen)
            {
                using (MemoryStream decompressStream = new MemoryStream())
                {
                    if (Decompress(a_bytes, a_nOffset, a_nLen, decompressStream))
                    {
                        return decompressStream.ToArray();
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            public static bool Decompress(byte[] a_bytes, int a_nOffset, int a_nLen, Stream a_DecompressedStream)
            {
                if (ms_CompressionHelper == null)
                {
                    return false;
                }

                if (a_bytes == null)
                {
                    return false;
                }

                if (a_nOffset < 0 || a_nLen <= 0 || a_nOffset + a_nLen > a_bytes.Length)
                {
                    return false;
                }

                if (a_DecompressedStream == null)
                {
                    return false;
                }

                try
                {
                    return ms_CompressionHelper.Decompress(a_bytes, a_nOffset, a_nLen, a_DecompressedStream);
                }
                catch (Exception)
                {
                    throw new Exception("Decompress Fail");
                }
            }

            public static bool Decompress(Stream stream, Stream decompressedStream)
            {
                if (ms_CompressionHelper == null)
                {
                    return false;
                }
                if (stream == null)
                {
                    return false;
                }

                if (decompressedStream == null)
                {
                    return false;
                }

                try
                {
                    return ms_CompressionHelper.Decompress(stream, decompressedStream);
                }
                catch (Exception)
                {
                    throw new Exception("Decompress Fail");
                }
            }
        }
    }
}
