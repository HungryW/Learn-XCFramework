using System;
using System.Runtime.InteropServices;

namespace XCFrameworkBase
{
    public partial struct SPackageVersionList
    {
        /// <summary>
        /// 名字 和 资源索引s
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        public struct SFileSystem
        {
            private static readonly int[] ms_EmptyIntArray = new int[] { };

            private readonly string m_szName;
            private readonly int[] m_arrResourceIdxes;

            public SFileSystem(string a_szName, int[] a_arrResourceIdxes)
            {
                m_szName = a_szName;
                m_arrResourceIdxes = a_arrResourceIdxes ?? ms_EmptyIntArray;
            }

            public string Name => m_szName;

            public int[] GetResourceIdxes()
            {
                return m_arrResourceIdxes;
            }
        }
    }
}
