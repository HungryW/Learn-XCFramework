
using System;
using System.Runtime.InteropServices;

namespace XCFrameworkBase
{
    public sealed partial class CFileSystem : IFileSystem
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct SStringData
        {
            private static readonly byte[] ms_CachedBytes = new byte[byte.MaxValue];

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = byte.MaxValue)]
            private byte[] m_arrBytes;

            private byte m_nLen;

            public SStringData(byte a_nLen, byte[] a_arrBytes)
            {
                m_nLen = a_nLen;
                m_arrBytes = a_arrBytes;
            }

            public string GetString(byte[] a_arrEncryptBytes)
            {
                if (m_nLen <= 0)
                {
                    return null;
                }
                Array.Copy(m_arrBytes, 0, ms_CachedBytes, 0, m_nLen);
                CUtility.Encryption.GetSelfXorBytes(ms_CachedBytes, a_arrEncryptBytes);
                return CUtility.Converter.GetString(ms_CachedBytes, 0, m_nLen);
            }

            public SStringData SetString(string a_szVal, byte[] a_arrEncrypyBytes)
            {
                if (string.IsNullOrEmpty(a_szVal))
                {
                    return Clear();
                }

                int len = CUtility.Converter.GetBytes(a_szVal, ms_CachedBytes);
                if (len > byte.MaxValue)
                {
                    throw new Exception("string is too long");
                }

                CUtility.Encryption.GetSelfXorBytes(ms_CachedBytes, a_arrEncrypyBytes);
                Array.Copy(ms_CachedBytes, 0, m_arrBytes, 0, len);
                return new SStringData((byte)len, m_arrBytes);
            }

            public SStringData Clear()
            {
                return new SStringData(0, m_arrBytes);
            }
        }
    }
}
