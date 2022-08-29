using System;

namespace XCFrameworkBase
{
    public abstract class CFsmState<T> where T : class
    {
        public CFsmState()
        {

        }

        public virtual void OnInit(IFsm<T> fsm)
        {

        }

        public virtual void OnEnter(IFsm<T> fsm)
        {

        }

        public virtual void OnUpdate(IFsm<T> fsm, float a_fElapseSed, float a_fRealElapseSed)
        {

        }

        public virtual void OnLeave(IFsm<T> fsm, bool isShutdown)
        {

        }

        public virtual void OnDestroy(IFsm<T> fsm)
        {

        }

        protected void ChangeState<TState>(IFsm<T> a_fsm) where TState : CFsmState<T>
        {
            CFsm<T> fsm = (CFsm<T>)a_fsm;
            System.Diagnostics.Debug.Assert(fsm != null);
            fsm.ChangeState<TState>();
        }

        protected void ChangeState(IFsm<T> a_fsm, Type stateType)
        {
            CFsm<T> fsm = (CFsm<T>)a_fsm;
            System.Diagnostics.Debug.Assert(fsm != null);
            System.Diagnostics.Debug.Assert(stateType != null);
            System.Diagnostics.Debug.Assert(!typeof(CFsmState<T>).IsAssignableFrom(stateType));
            fsm.ChangeState(stateType);
        }
    }
}
