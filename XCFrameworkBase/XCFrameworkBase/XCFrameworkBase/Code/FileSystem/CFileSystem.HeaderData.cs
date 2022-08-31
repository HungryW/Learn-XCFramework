using System;
using System.Runtime.InteropServices;

namespace XCFrameworkBase
{
    public sealed partial class CFileSystem : IFileSystem
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct SHeaderData
        {
            private const int mc_HeaderLen = 3;
            private const int mc_Version = 0;
            private const int mc_EncryptBytesLen = 4;
            private static byte[] mc_arrHeader = new byte[mc_HeaderLen] { (byte)'X', (byte)'C', (byte)'F' };

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = mc_HeaderLen)]
            private byte[] m_arrHeader;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = mc_EncryptBytesLen)]
            private byte[] m_arrEncryptBytes;

            private byte m_nVersion;
            private int m_nMaxFileCount;
            private int m_nMaxBlockCount;
            private int m_nBlockCount;


            public SHeaderData(byte a_version, byte[] a_arrEncryBytes, int a_nMaxFileCount, int a_nMaxBlockCount, int a_nBlockCount)
            {
                m_arrHeader = mc_arrHeader;
                m_nVersion = a_version;
                m_arrEncryptBytes = a_arrEncryBytes;
                m_nMaxFileCount = a_nMaxFileCount;
                m_nMaxBlockCount = a_nMaxBlockCount;
                m_nBlockCount = a_nBlockCount;
            }

            public SHeaderData(int a_nMaxFileCount, int a_nMaxBlockCount)
                    : this(mc_Version, new byte[mc_EncryptBytesLen], a_nMaxFileCount, a_nMaxBlockCount, 0)
            {
                CUtility.Random.GetRandomBytes(m_arrEncryptBytes);
            }

            public bool IsValid()
            {
                return m_nMaxFileCount > 0 && m_nMaxBlockCount > 0 && m_nMaxFileCount <= m_nMaxBlockCount && m_nBlockCount > 0 && m_nBlockCount <= m_nMaxBlockCount;
            }


            public byte Version
            {
                get
                {
                    return m_nVersion;
                }
            }

            public int MaxFileCount
            {
                get
                {
                    return m_nMaxFileCount;
                }
            }

            public int MaxBlockCount
            {
                get
                {
                    return m_nMaxBlockCount;
                }
            }

            public int BlockCount
            {
                get
                {
                    return m_nBlockCount;
                }
            }

            public byte[] GetEncryptBytes()
            {
                return m_arrEncryptBytes;
            }

            public SHeaderData SetBlockCount(int a_nBlockCount)
            {
                return new SHeaderData(m_nVersion, m_arrEncryptBytes, m_nMaxFileCount, m_nBlockCount, a_nBlockCount);
            }


        }
    }
}
