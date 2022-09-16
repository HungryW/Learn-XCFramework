using System;
using System.Runtime.InteropServices;

namespace XCFrameworkBase
{
    public partial struct SPackageVersionList
    {
        /// <summary>
        /// 名字 变体名 扩展名 加载方式 长度 hash 包含Asset索引S
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        public struct SResource
        {
            private static readonly int[] ms_EmptyIntArray = new int[] { };

            private readonly string m_szName;
            private readonly string m_szVariant;
            private readonly string m_szExtension;
            private readonly byte m_eLoadType;
            private readonly int m_nLen;
            private readonly int m_nHashCode;
            private readonly int[] m_arrAssetIdxes;

            public SResource(string a_szName, string a_szVariant, string a_szExtension, byte a_eLoadType, int a_nLen, int a_nHashCode, int[] a_arrAssetIdxes)
            {
                m_szName = a_szName;
                m_szVariant = a_szVariant;
                m_szExtension = a_szExtension;
                m_eLoadType = a_eLoadType;
                m_nLen = a_nLen;
                m_nHashCode = a_nHashCode;
                m_arrAssetIdxes = a_arrAssetIdxes ?? ms_EmptyIntArray;
            }

            public string Name => m_szName;
            public string Variant => m_szVariant;
            public string Extension => m_szExtension;
            public byte LoadType => m_eLoadType;
            public int Len => m_nLen;
            public int HashCode => m_nHashCode;

            public int[] GetAssetIdxes()
            {
                return m_arrAssetIdxes;
            }
        }
    }
}
