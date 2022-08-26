using System;

namespace XCFrameworkBase
{
    public sealed partial class CWebRequestMgr : CGameFrameworkModule, IWebRequestMgr
    {
        private readonly CTaskPool<CWebRequestTask> m_taskPool;
        private float m_fTiemout;
        private EventHandler<CWebRequestStartEventArgs> m_eventOnRequestStart;
        private EventHandler<CWebRequestSuccessEventArgs> m_eventOnRequestSuccess;
        private EventHandler<CWebRequestFailEventArgs> m_eventOnRequestFail;

        public CWebRequestMgr()
        {
            m_taskPool = new CTaskPool<CWebRequestTask>();
            m_fTiemout = 30f;
            m_eventOnRequestFail = null;
            m_eventOnRequestStart = null;
            m_eventOnRequestSuccess = null;
        }

        public float TimeOut
        {
            get
            {
                return m_fTiemout;
            }
            set
            {
                m_fTiemout = value;
            }
        }

        public event EventHandler<CWebRequestStartEventArgs> WebRequestStart
        {
            add
            {
                m_eventOnRequestStart += value;
            }
            remove
            {
                m_eventOnRequestStart -= value;
            }
        }

        public event EventHandler<CWebRequestSuccessEventArgs> WebRequestSuccess
        {
            add
            {
                m_eventOnRequestSuccess += value;
            }
            remove
            {
                m_eventOnRequestSuccess -= value;
            }
        }

        public event EventHandler<CWebRequestFailEventArgs> WebRequestFail
        {
            add
            {
                m_eventOnRequestFail += value;
            }
            remove
            {
                m_eventOnRequestFail -= value;
            }
        }

        public void AddWebRequestAgentHelper(IWebRequestAgentHelper a_helper)
        {
            CWebRequestAgent agent = new CWebRequestAgent(a_helper);
            agent.InitCallback(_OnRequestAgentStart, _OnRequestAgentSuccess, _OnRequestAgentFail);
            m_taskPool.AddAgent(agent);
        }

        private void _OnRequestAgentStart(CWebRequestAgent a_agent)
        {
            if (m_eventOnRequestStart != null)
            {
                CWebRequestStartEventArgs arg = CWebRequestStartEventArgs.Create(a_agent.Task.Id, a_agent.Task.WebRequestUri, a_agent.Task.UserData);
                m_eventOnRequestStart.Invoke(this, arg);
                CReferencePool.Release(arg);
            }
        }

        private void _OnRequestAgentSuccess(CWebRequestAgent a_agent, byte[] a_arrRespondData)
        {
            if (m_eventOnRequestSuccess != null)
            {
                CWebRequestSuccessEventArgs arg = CWebRequestSuccessEventArgs.Create(a_agent.Task.Id, a_agent.Task.WebRequestUri, a_agent.Task.UserData, a_arrRespondData);
                m_eventOnRequestSuccess.Invoke(this, arg);
                CReferencePool.Release(arg);
            }
        }

        private void _OnRequestAgentFail(CWebRequestAgent agent, string a_szErrorMsg)
        {
            if (m_eventOnRequestFail != null)
            {
                CWebRequestFailEventArgs arg = CWebRequestFailEventArgs.Create(agent.Task.Id, agent.Task.WebRequestUri, agent.Task.UserData, a_szErrorMsg);
                m_eventOnRequestFail.Invoke(this, arg);
                CReferencePool.Release(arg);
            }
        }

        public int AddWebRequest(string a_szRequestUri)
        {
            return AddWebRequest(a_szRequestUri, null, null, 0, null);
        }

        public int AddWebRequest(string a_szRequestUri, byte[] a_arrPostData)
        {
            return AddWebRequest(a_szRequestUri, a_arrPostData, null, 0, null);
        }

        public int AddWebRequest(string a_szRequestUri, byte[] a_arrPostData, string a_szTag, int a_nPriority, object a_oUserData)
        {
            if (m_taskPool.TotalAgent <= 0)
            {
                throw new GameFrameworkException("You must add web agent first");
            }

            CWebRequestTask task = CWebRequestTask.Create(a_szRequestUri, a_arrPostData, m_fTiemout, a_szTag, a_nPriority, a_oUserData);
            m_taskPool.AddTask(task);
            return task.Id;
        }



        public int RemoveAllWebRequest()
        {
            return m_taskPool.RemoveAllTasks();
        }

        public bool RemoveWebRequest(int a_nId)
        {
            return m_taskPool.RemoveTask(a_nId);
        }

        public int RemoveWebRequest(string a_szTag)
        {
            return m_taskPool.RemoveTasks(a_szTag);
        }

        public override void Shutdown()
        {
            m_taskPool.Shutdown();
        }

        public override void Update(float a_fElapseSed, float a_fRealElapseSed)
        {
            m_taskPool.Update(a_fElapseSed, a_fRealElapseSed);
        }

    }
}
