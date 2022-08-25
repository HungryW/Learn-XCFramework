using System;

namespace XCFrameworkBase
{
    public sealed partial class CWebRequestMgr : CGameFrameworkModule, IWebRequestMgr
    {
        private enum EWebRequestTaskStatus : byte
        {
            Todo = 0,
            Doing,
            Done,
            Error,
        }


        private sealed class CWebRequestTask : CTaskBase
        {
            private static int ms_nIdSeed = 0;

            private EWebRequestTaskStatus m_eStatus;
            private string m_szWebRequestUri;
            private byte[] m_arrPostData;
            private float m_fTimeout;

            public CWebRequestTask()
            {
                m_eStatus = EWebRequestTaskStatus.Todo;
                m_szWebRequestUri = null;
                m_arrPostData = null;
                m_fTimeout = 0;
            }

            public override void Clear()
            {
                base.Clear();
                m_eStatus = EWebRequestTaskStatus.Todo;
                m_szWebRequestUri = null;
                m_arrPostData = null;
                m_fTimeout = 0;
            }

            public static CWebRequestTask Create(string a_szWebRequestUri, byte[] postData, float a_fTimeOut, string a_szTag, int a_nPriority, object a_oUserData)
            {
                CWebRequestTask task = CReferencePool.Acquire<CWebRequestTask>();
                task.Initialize(++ms_nIdSeed, a_szTag, a_nPriority, a_oUserData);
                task.m_szWebRequestUri = a_szWebRequestUri;
                task.m_arrPostData = postData;
                task.m_fTimeout = a_fTimeOut;
                return task;
            }

            public void SetStatus(EWebRequestTaskStatus a_eStatus)
            {
                m_eStatus = a_eStatus;
            }

            public EWebRequestTaskStatus GetStatus()
            {
                return m_eStatus;
            }

            public string WebRequestUri
            {
                get
                {
                    return m_szWebRequestUri;
                }
            }

            public float TimeOut
            {
                get
                {
                    return m_fTimeout;
                }
            }

            public byte[] GetPostData()
            {
                return m_arrPostData;
            }
        }
    }
}
