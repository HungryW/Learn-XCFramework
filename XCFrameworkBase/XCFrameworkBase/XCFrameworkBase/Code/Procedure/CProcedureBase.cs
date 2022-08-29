using System;

namespace XCFrameworkBase
{
    public abstract class CProcedureBase : CFsmState<IProcedureMgr>
    {
        public override void OnInit(IFsm<IProcedureMgr> fsm)
        {
            base.OnInit(fsm);
        }

        public override void OnEnter(IFsm<IProcedureMgr> fsm)
        {
            base.OnEnter(fsm);
        }

        public override void OnUpdate(IFsm<IProcedureMgr> fsm, float a_fElapseSed, float a_fRealElapseSed)
        {
            base.OnUpdate(fsm, a_fElapseSed, a_fRealElapseSed);
        }

        public override void OnLeave(IFsm<IProcedureMgr> fsm, bool isShutdown)
        {
            base.OnLeave(fsm, isShutdown);
        }

        public override void OnDestroy(IFsm<IProcedureMgr> fsm)
        {
            base.OnDestroy(fsm);
        }

    }
}
