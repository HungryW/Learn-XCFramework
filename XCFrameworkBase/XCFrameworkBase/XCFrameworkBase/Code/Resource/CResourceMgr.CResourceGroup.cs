using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr : CGameFrameworkModule, IResourceMgr
    {
        /// <summary>
        /// 名字 resourceInfo的引用 所有资源的SResourceName 总长度 压缩后的总长度
        /// </summary>
        private sealed class CResourceGroup : IResourceGroup
        {
            private readonly string m_szName;
            private readonly Dictionary<SResourceName, CResourceInfo> m_mapResourceInfo;
            private readonly HashSet<SResourceName> m_setResourceNames;
            private long m_nTotalLen;
            private long m_nTotalCompressLen;

            public CResourceGroup(string a_szName, Dictionary<SResourceName, CResourceInfo> a_refMapResourceInfos)
            {
                m_szName = a_szName;
                m_mapResourceInfo = a_refMapResourceInfos;
                m_setResourceNames = new HashSet<SResourceName>();
            }

            public void AddResource(SResourceName a_resourceName, int a_nLen, int a_nCompressLen)
            {
                m_setResourceNames.Add(a_resourceName);
                m_nTotalLen += a_nLen;
                m_nTotalCompressLen += a_nCompressLen;
            }

            public bool HasResource(SResourceName a_name)
            {
                return m_setResourceNames.Contains(a_name);
            }

            public string Name => m_szName;


            public int TotalCount => m_setResourceNames.Count;

            public int ReadyCount
            {
                get
                {
                    int nReadyCount = 0;
                    foreach (SResourceName resName in m_setResourceNames)
                    {
                        CResourceInfo resInfo = null;
                        if (m_mapResourceInfo.TryGetValue(resName, out resInfo) && resInfo.Ready)
                        {
                            nReadyCount++;
                        }
                    }
                    return nReadyCount;
                }
            }

            public bool Ready => ReadyCount >= TotalCount;

            public long TotalLen => m_nTotalLen;

            public long ReadyLen
            {
                get
                {
                    long nReadyLen = 0;
                    foreach (SResourceName resName in m_setResourceNames)
                    {
                        CResourceInfo resInfo = null;
                        if (m_mapResourceInfo.TryGetValue(resName, out resInfo) && resInfo.Ready)
                        {
                            nReadyLen += resInfo.Length;
                        }
                    }
                    return nReadyLen;
                }
            }

            public long TotalCompressLen => m_nTotalCompressLen;

            public long ReadyCompressLen
            {
                get
                {
                    long nReadyCompressLen = 0;
                    foreach (SResourceName resName in m_setResourceNames)
                    {
                        CResourceInfo resInfo = null;
                        if (m_mapResourceInfo.TryGetValue(resName, out resInfo) && resInfo.Ready)
                        {
                            nReadyCompressLen += resInfo.CompressedLength;
                        }
                    }
                    return nReadyCompressLen;
                }
            }

            public float Progress
            {
                get
                {
                    return m_nTotalLen > 0 ? (float)ReadyLen / m_nTotalLen : 1L;
                }
            }

            public void GetResourceNames(List<string> a_outListNames)
            {
                a_outListNames.Clear();
                foreach (var name in m_setResourceNames)
                {
                    a_outListNames.Add(name.FullName);
                }
            }

            public void GetResourceInternalNames(List<SResourceName> a_outListName)
            {
                a_outListName.Clear();
                foreach (var name in m_setResourceNames)
                {
                    a_outListName.Add(name);
                }
            }
        }
    }
}
