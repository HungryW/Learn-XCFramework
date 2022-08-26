using System;

namespace XCFrameworkBase
{
    public sealed partial class CWebRequestMgr : CGameFrameworkModule, IWebRequestMgr
    {
        private sealed class CWebRequestAgent : ITaskAgent<CWebRequestTask>
        {
            private readonly IWebRequestAgentHelper m_helper;
            private CWebRequestTask m_task;
            private float m_fWaitTime;

            private Action<CWebRequestAgent> m_fnOnRequestAgentStart;
            private Action<CWebRequestAgent, byte[]> m_fnOnRequestAgentSuccess;
            private Action<CWebRequestAgent, string> m_fnOnRequestAgentFail;

            public CWebRequestAgent(IWebRequestAgentHelper a_helper)
            {
                if (a_helper == null)
                {
                    throw new GameFrameworkException("Web Request agent helper invalid");
                }

                m_helper = a_helper;
                m_task = null;
                m_fWaitTime = 0f;

                m_fnOnRequestAgentFail = null;
                m_fnOnRequestAgentStart = null;
                m_fnOnRequestAgentSuccess = null;
            }

            public void InitCallback(Action<CWebRequestAgent> a_fnStart, Action<CWebRequestAgent, byte[]> a_fnSuccess, Action<CWebRequestAgent, string> a_fnFail)
            {
                m_fnOnRequestAgentStart = a_fnStart;
                m_fnOnRequestAgentSuccess = a_fnSuccess;
                m_fnOnRequestAgentFail = a_fnFail;
            }

            public void Init()
            {
                m_helper.OnHelperRequestComplete += _OnAgentHelperComplete;
                m_helper.OnHelperRequestFail += _OnAgentHelperFail;
            }

            public void ShutDown()
            {
                Reset();
                m_helper.OnHelperRequestComplete -= _OnAgentHelperComplete;
                m_helper.OnHelperRequestFail -= _OnAgentHelperFail;
            }

            public EStartTaskStatus Start(CWebRequestTask task)
            {
                if (task == null)
                {
                    throw new Exception("Task is invalid");
                }
                m_task = task;
                m_task.SetStatus(EWebRequestTaskStatus.Doing);
                m_fnOnRequestAgentStart?.Invoke(this);

                byte[] postData = m_task.GetPostData();
                if (postData == null)
                {
                    m_helper.Request(m_task.WebRequestUri, m_task.UserData);
                }
                else
                {
                    m_helper.Request(m_task.WebRequestUri, postData, m_task.UserData);
                }
                m_fWaitTime = 0f;
                return EStartTaskStatus.CanResume;
            }

            public void Update(float a_fElapseSed, float a_fRealElapseSed)
            {
                if (m_task.GetStatus() == EWebRequestTaskStatus.Doing)
                {
                    m_fWaitTime += a_fRealElapseSed;
                    if (m_fWaitTime >= m_task.TimeOut)
                    {
                        CWebRequestAgentHelperErrorEventArgs arg = CWebRequestAgentHelperErrorEventArgs.Create("TimeOut");
                        _OnAgentHelperFail(this, arg);
                        CReferencePool.Release(arg);
                    }
                }
            }

            public void Reset()
            {
                m_helper.Reset();
                m_task = null;
                m_fWaitTime = 0f;
            }

            public CWebRequestTask Task
            {
                get
                {
                    return m_task;
                }
            }

            private void _OnAgentHelperComplete(object a_oSender, CWebRequestAgentHelperCompleteEventArgs a_arg)
            {
                m_helper.Reset();
                m_task.SetStatus(EWebRequestTaskStatus.Done);
                m_fnOnRequestAgentSuccess?.Invoke(this, a_arg.GetWebResponseData());
                m_task.Done = true;
            }

            private void _OnAgentHelperFail(object a_oSender, CWebRequestAgentHelperErrorEventArgs a_arg)
            {
                m_helper.Reset();
                m_task.SetStatus(EWebRequestTaskStatus.Error);
                m_fnOnRequestAgentFail?.Invoke(this, a_arg.ErrorMessage);
                m_task.Done = true;
            }
        }

    }
}
