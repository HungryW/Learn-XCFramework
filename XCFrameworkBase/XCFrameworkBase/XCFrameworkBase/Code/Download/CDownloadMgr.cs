using System;

namespace XCFrameworkBase
{
    public sealed partial class CDownloadMgr : CGameFrameworkModule, IDownloadMgr
    {
        private const int mc_nOneMegaBytes = 1024 * 1024;

        private readonly CTaskPool<CDownloadTask> m_taskPool;
        private int m_nFlushSize;
        private float m_fTimeout;
        private EventHandler<CDownloadStartEventArgs> m_eventStart;
        private EventHandler<CDownloadUpadateEventArgs> m_eventUpdate;
        private EventHandler<CDownloadSuccessEventArgs> m_eventSuccess;
        private EventHandler<CDownloadFailEventArgs> m_eventFail;

        public CDownloadMgr()
        {
            m_taskPool = new CTaskPool<CDownloadTask>();
            m_nFlushSize = mc_nOneMegaBytes;
            m_fTimeout = 30.0f;
            m_eventStart = null;
            m_eventUpdate = null;
            m_eventSuccess = null;
            m_eventFail = null;
        }

        public override int Priority
        {
            get
            {
                return 5;
            }
        }

        public bool Paused
        {
            get
            {
                return m_taskPool.Pasused;
            }
            set
            {
                m_taskPool.Pasused = value;
            }
        }

        public int FlushSize
        {
            get
            {
                return m_nFlushSize;
            }
            set
            {
                m_nFlushSize = value;
            }
        }

        public float Timeout
        {
            get
            {
                return m_fTimeout;
            }
            set
            {
                m_fTimeout = value;
            }
        }

        public float CurrentSpeed => throw new NotImplementedException();

        public event EventHandler<CDownloadStartEventArgs> DownloadStart
        {
            add
            {
                m_eventStart += value;
            }
            remove
            {
                m_eventStart -= value;
            }
        }

        public event EventHandler<CDownloadUpadateEventArgs> DownloadUpdate
        {
            add
            {
                m_eventUpdate += value;
            }
            remove
            {
                m_eventUpdate -= value;
            }
        }

        public event EventHandler<CDownloadSuccessEventArgs> DownloadSuccess
        {
            add
            {
                m_eventSuccess += value;
            }
            remove
            {
                m_eventSuccess -= value;
            }
        }

        public event EventHandler<CDownloadFailEventArgs> DownloadFail
        {
            add
            {
                m_eventFail += value;
            }
            remove
            {
                m_eventFail -= value;
            }
        }


        public override void Update(float a_fElapseSed, float a_fRealElapseSed)
        {
            m_taskPool.Update(a_fElapseSed, a_fRealElapseSed);
        }

        public override void Shutdown()
        {
            m_taskPool.Shutdown();
        }

        public void AddDownloadAgentHelper(IDownloadAgentHelper a_helper)
        {
            CDownloadAgent agent = new CDownloadAgent(a_helper);
            agent.InitCallbacks(_OnAgentStart, _OnAgentUpdate, _OnAgentSuccess, _OnAgentFail);
            m_taskPool.AddAgent(agent);
        }

        private void _OnAgentStart(CDownloadAgent a_agent)
        {
            if (m_eventStart != null)
            {
                CDownloadStartEventArgs arg = CDownloadStartEventArgs.Create(a_agent.Task.Id, a_agent.Task.DownloadPath, a_agent.Task.DownloadUri, a_agent.CurrentLen, a_agent.Task.UserData);
                m_eventStart.Invoke(this, arg);
                CReferencePool.Release(arg);
            }
        }

        private void _OnAgentUpdate(CDownloadAgent a_agent, int a_nDeltalLen)
        {
            if (m_eventUpdate != null)
            {
                CDownloadUpadateEventArgs arg = CDownloadUpadateEventArgs.Create(a_agent.Task.Id, a_agent.Task.DownloadPath, a_agent.Task.DownloadUri, a_agent.CurrentLen, a_agent.Task.UserData);
                m_eventUpdate.Invoke(this, arg);
                CReferencePool.Release(arg);
            }
        }

        private void _OnAgentSuccess(CDownloadAgent a_agent, long a_lLen)
        {
            if (m_eventSuccess != null)
            {
                CDownloadSuccessEventArgs arg = CDownloadSuccessEventArgs.Create(a_agent.Task.Id, a_agent.Task.DownloadPath, a_agent.Task.DownloadUri, a_agent.CurrentLen, a_agent.Task.UserData);
                m_eventSuccess.Invoke(this, arg);
                CReferencePool.Release(arg);
            }
        }

        private void _OnAgentFail(CDownloadAgent a_agent, string a_szErrorMsg)
        {
            if (m_eventFail != null)
            {
                CDownloadFailEventArgs arg = CDownloadFailEventArgs.Create(a_agent.Task.Id, a_agent.Task.DownloadPath, a_agent.Task.DownloadUri, a_agent.Task.UserData, a_szErrorMsg);
                m_eventFail.Invoke(this, arg);
                CReferencePool.Release(arg);
            }
        }


        public int AddDownload(string a_szDownloadPath, string a_szDownloadUri, string a_szTag, int a_nPriority, object a_oUserData)
        {
            CDownloadTask task = CDownloadTask.Create(a_szDownloadPath, a_szDownloadUri, a_szTag, a_nPriority, FlushSize, m_fTimeout, a_oUserData);
            m_taskPool.AddTask(task);
            return task.Id;
        }

        public bool RemoveDownload(int a_nId)
        {
            return m_taskPool.RemoveTask(a_nId);
        }

        public int RemoveDownloads(string a_szTag)
        {
            return m_taskPool.RemoveTasks(a_szTag);
        }

        public int RemoveAllDownloads()
        {
            return m_taskPool.RemoveAllTasks();
        }
    }
}
