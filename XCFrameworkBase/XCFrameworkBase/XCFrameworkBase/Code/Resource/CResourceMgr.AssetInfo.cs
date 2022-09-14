﻿using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public sealed partial class CResourceMgr : CGameFrameworkModule, IResourceMgr
    {
        private sealed class CAssetInfo
        {
            private readonly string m_szAssetName;
            private readonly SResourceName m_ResourceName;
            private readonly string[] m_arrDependencyAssetName;

            public CAssetInfo(string a_szAssetName, SResourceName a_refResourceName, string[] a_refArrDependencyAssetName)
            {
                m_szAssetName = a_szAssetName;
                m_ResourceName = a_refResourceName;
                m_arrDependencyAssetName = a_refArrDependencyAssetName;
            }

            public string AssetName => m_szAssetName;

            public SResourceName ResourceName => m_ResourceName;

            public string[] GetDependencyAssetNames()
            {
                return m_arrDependencyAssetName;
            }
        }
    }
}
