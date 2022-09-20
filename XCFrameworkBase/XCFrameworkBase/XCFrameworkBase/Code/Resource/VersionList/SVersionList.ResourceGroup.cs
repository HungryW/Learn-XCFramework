using System;
using System.Runtime.InteropServices;

namespace XCFrameworkBase
{
    public partial struct SVersionList
    {
        /// <summary>
        /// 名字 资源索引S
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        public struct SResourceGroup
        {
            private static readonly int[] ms_EmptyIntArray = new int[] { };

            private readonly string m_szName;
            private readonly int[] m_arrResourceIdxes;

            public SResourceGroup(string a_szName, int[] a_arrResourceIdxes)
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
