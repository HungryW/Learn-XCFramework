using System;

namespace XCFrameworkBase
{
    public sealed class CProcedureMgr : CGameFrameworkModule, IProcedureMgr
    {
        private IFsmMgr m_refFsmMgr;
        private IFsm<IProcedureMgr> m_fsmProcedure;

        public CProcedureMgr()
        {
            m_refFsmMgr = null;
            m_fsmProcedure = null;
        }

        public void Init(IFsmMgr a_fsmMgr, params CProcedureBase[] a_arrProceduces)
        {
            System.Diagnostics.Debug.Assert(a_fsmMgr != null);
            m_refFsmMgr = a_fsmMgr;
            m_fsmProcedure = m_refFsmMgr.CreateFsm(this, a_arrProceduces);
        }


        public void StartProcedure<T>() where T : CProcedureBase
        {
            System.Diagnostics.Debug.Assert(m_fsmProcedure != null);
            m_fsmProcedure.Start<T>();
        }

        public void StartProcedure(Type a_t)
        {
            System.Diagnostics.Debug.Assert(m_fsmProcedure != null);
            m_fsmProcedure.Start(a_t);
        }


        public override void Update(float a_fElapseSed, float a_fRealElapseSed)
        {

        }

        public override void Shutdown()
        {
            if (m_refFsmMgr != null)
            {
                if (m_fsmProcedure != null)
                {
                    m_refFsmMgr.DestoryFsm(m_fsmProcedure);
                    m_fsmProcedure = null;
                }
                m_refFsmMgr = null;
            }
        }

        public CProcedureBase GetProcedure<T>() where T : CProcedureBase
        {
            System.Diagnostics.Debug.Assert(m_fsmProcedure != null);
            return m_fsmProcedure.GetState<T>();
        }

        public CProcedureBase GetProcedure(Type a_t)
        {
            System.Diagnostics.Debug.Assert(m_fsmProcedure != null);
            return (CProcedureBase)m_fsmProcedure.GetState(a_t);
        }


        public CProcedureBase CurrentProceduce
        {
            get
            {
                if (m_fsmProcedure == null)
                {
                    return null;
                }
                return (CProcedureBase)m_fsmProcedure.CurState;
            }
        }


        public float CurProceduceTime
        {
            get
            {
                if (m_fsmProcedure == null)
                {
                    return 0f;
                }
                return m_fsmProcedure.CurrentStateTime;
            }
        }

    }
}
