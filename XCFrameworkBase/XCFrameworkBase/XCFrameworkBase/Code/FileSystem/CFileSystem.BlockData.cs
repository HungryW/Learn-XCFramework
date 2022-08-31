using System;
using System.Runtime.InteropServices;

namespace XCFrameworkBase
{
    public sealed partial class CFileSystem : IFileSystem
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct SBlockData
        {
            public static readonly SBlockData Empty = new SBlockData(0, 0);

            private int m_nStringIdx;
            private int m_nClusterIdx;
            private int m_nLen;

            public SBlockData(int a_nStringIdx, int a_nClusterIdx, int a_nLen)
            {
                m_nStringIdx = a_nStringIdx;
                m_nClusterIdx = a_nClusterIdx;
                m_nLen = a_nLen;
            }

            public SBlockData(int a_nClusterIdx, int a_nLen)
                : this(-1, a_nClusterIdx, a_nLen) { }

            public bool Using => m_nStringIdx >= 0;

            public int StringIdx => m_nStringIdx;
            public int ClusterIdx => m_nClusterIdx;
            public int Len => m_nLen;


        }
    }
}