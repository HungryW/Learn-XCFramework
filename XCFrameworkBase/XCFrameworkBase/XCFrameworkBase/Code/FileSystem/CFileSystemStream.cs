using System;
using System.IO;

namespace XCFrameworkBase
{
    public abstract class CFileSystemStream
    {
        protected const int mc_nCacheByteLen = 0x1000;

        protected static byte[] ms_CacheBytes = new byte[mc_nCacheByteLen];

        public abstract long Position
        {
            get;
            set;
        }

        public abstract long Len
        {
            get;
        }

        public abstract void SetLen(long a_len);

        public abstract void Seek(long a_offset, SeekOrigin a_origin);

        public abstract int ReadByte();

        public abstract int Read(byte[] buffer, int a_nStartIdx, int a_nLen);

        public int Read(Stream a_outStream, int a_nLen)
        {
            int nByteRead = 0;
            int nByteLeft = a_nLen;
            while ((nByteRead = Read(ms_CacheBytes, 0, Math.Min(nByteLeft, mc_nCacheByteLen))) > 0)
            {
                nByteLeft -= nByteRead;
                a_outStream.Write(ms_CacheBytes, 0, nByteRead);
            }
            Array.Clear(ms_CacheBytes, 0, mc_nCacheByteLen);
            return a_nLen - nByteLeft;
        }

        public abstract void WriteByte(byte val);
        public abstract void Write(byte[] a_arrBuff, int a_nStartIdx, int a_nLen);
        public void Write(Stream stream, int a_nLen)
        {
            int nByteRead = 0;
            int nBytesLeft = a_nLen;
            while ((nByteRead = stream.Read(ms_CacheBytes, 0, Math.Min(nBytesLeft, mc_nCacheByteLen))) > 0)
            {
                nBytesLeft -= nByteRead;
                Write(ms_CacheBytes, 0, nByteRead);
            }
            Array.Clear(ms_CacheBytes, 0, mc_nCacheByteLen);
        }

        public abstract void Flush();
        public abstract void Close();
    }
}
