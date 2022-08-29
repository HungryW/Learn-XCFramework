using System;

namespace XCFrameworkBase
{
    public interface IProcedureMgr
    {
        CProcedureBase CurrentProceduce
        {
            get;
        }

        float CurProceduceTime
        {
            get;
        }

        void Init(IFsmMgr a_fsmMgr, params CProcedureBase[] a_arrProceduces);

        void StartProcedure<T>() where T : CProcedureBase;
        void StartProcedure(Type a_t);

        CProcedureBase GetProcedure<T>() where T : CProcedureBase;
        CProcedureBase GetProcedure(Type a_t);
    }
}
