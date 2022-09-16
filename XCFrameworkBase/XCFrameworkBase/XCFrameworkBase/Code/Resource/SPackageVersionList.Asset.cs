using System;
using System.Runtime.InteropServices;

namespace XCFrameworkBase
{
    public partial struct SPackageVersionList
    {
        /// <summary>
        /// 名字 和 依赖Asset索引
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        public struct SAsset
        {
            private static readonly int[] ms_EmptyIntArray = new int[] { };

            private readonly string m_szName;
            private readonly int[] m_arrDependedcyAssetIdxes;

            public SAsset(string a_szName, int[] a_arrDependAssetIdxes)
            {
                m_szName = a_szName;
                m_arrDependedcyAssetIdxes = a_arrDependAssetIdxes ?? ms_EmptyIntArray;
            }

            public string Name => m_szName;

            public int[] GetDependAssetIdxes()
            {
                return m_arrDependedcyAssetIdxes;
            }
        }
    }
}
