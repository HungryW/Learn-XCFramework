using System;
using System.Runtime.InteropServices;

namespace XCFrameworkBase
{
    [StructLayout(LayoutKind.Auto)]
    public struct SFileInfo
    {
        private string m_szName;
        private long m_nOffset;
        private int m_nLen;

        public SFileInfo(string a_szName, long a_nOffset, int a_nLen)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(a_szName));
            System.Diagnostics.Debug.Assert(a_nOffset >= 0);
            System.Diagnostics.Debug.Assert(a_nLen >= 0);

            m_szName = a_szName;
            m_nLen = a_nLen;
            m_nOffset = a_nOffset;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(m_szName) && m_nOffset >= 0 && m_nLen >= 0;
        }

        public string Name
        {
            get
            {
                return Name;
            }
        }

        public long Offset
        {
            get
            {
                return m_nOffset;
            }
        }

        public int Len
        {
            get
            {
                return m_nLen;
            }
        }

    }
}
