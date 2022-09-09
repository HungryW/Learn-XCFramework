using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr : CGameFrameworkModule, IResourceMgr
    {
        [StructLayout(LayoutKind.Auto)]
        private struct SResourceName : IComparable, IComparable<SResourceName>, IEquatable<SResourceName>
        {
            private static readonly Dictionary<SResourceName, string> ms_mapResFullName = new Dictionary<SResourceName, string>();

            private readonly string m_szName;
            private readonly string m_szVariant;
            private readonly string m_szExtension;

            public SResourceName(string a_szName, string a_szVariant, string a_szExtension)
            {
                m_szName = a_szName;
                m_szVariant = a_szVariant;
                m_szExtension = a_szExtension;
            }

            public string Name => m_szName;
            public string Variant => m_szVariant;
            public string Extentsion => m_szExtension;

            public string FullName
            {
                get
                {
                    if (ms_mapResFullName.ContainsKey(this))
                    {
                        return ms_mapResFullName[this];
                    }
                    string szFullName = m_szVariant != null
                                        ? CUtility.Text.Format("{0}.{1}.{2}", m_szName, m_szVariant, m_szExtension)
                                        : CUtility.Text.Format("{0}.{1}", m_szName, m_szExtension);
                    ms_mapResFullName.Add(this, szFullName);
                    return szFullName;
                }
            }

            public override string ToString()
            {
                return FullName;
            }

            public override int GetHashCode()
            {
                if (m_szVariant == null)
                {
                    return m_szName.GetHashCode() ^ m_szExtension.GetHashCode();
                }
                else
                {
                    return m_szName.GetHashCode() ^ m_szVariant.GetHashCode() ^ m_szExtension.GetHashCode();
                }
            }

            public override bool Equals(object obj)
            {
                return (obj is SResourceName) && Equals((SResourceName)obj);
            }

            public bool Equals(SResourceName a_otherName)
            {
                return string.Equals(m_szName, a_otherName.m_szName, StringComparison.Ordinal);
            }

            public static bool operator ==(SResourceName a, SResourceName b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(SResourceName a, SResourceName b)
            {
                return !(a == b);
            }

            public int CompareTo(object val)
            {
                if (val == null)
                {
                    return 1;
                }

                if (!(val is SResourceName))
                {
                    return 1;
                }
                return CompareTo((SResourceName)val);
            }

            public int CompareTo(SResourceName a_otherName)
            {
                int r = string.CompareOrdinal(m_szName, a_otherName.m_szName);
                if (r != 0)
                {
                    return r;
                }
                r = string.CompareOrdinal(m_szVariant, a_otherName.m_szVariant);
                if (r != 0)
                {
                    return r;
                }

                return string.CompareOrdinal(m_szExtension, a_otherName.m_szExtension);
            }
        }
    }
}
