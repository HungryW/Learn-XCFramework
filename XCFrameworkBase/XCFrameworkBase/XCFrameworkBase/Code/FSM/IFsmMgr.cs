using System;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public interface IFsmMgr
    {
        int Count { get; }

        bool HasFsm<T>() where T : class;
        bool HasFsm(Type ownerType);
        bool HasFsm<T>(string a_szName) where T : class;
        bool HasFsm(Type ownerType, string a_szName);

        IFsm<T> GetFsm<T>() where T : class;
        CFsmBase GetFsm(Type ownerType);
        IFsm<T> GetFsm<T>(string a_szName) where T : class;
        CFsmBase GetFsm(Type ownerType, string a_szName);

        IFsm<T> CreateFsm<T>(T owner, params CFsmState<T>[] states) where T : class;
        IFsm<T> CreateFsm<T>(string a_szName, T owner, params CFsmState<T>[] states) where T : class;
        IFsm<T> CreateFsm<T>(T owner, List<CFsmState<T>> states) where T : class;
        IFsm<T> CreateFsm<T>(string a_szName, T owner, List<CFsmState<T>> states) where T : class;

        bool DestroyFsm<T>() where T : class;
        bool DestroyFsm(Type ownerType);
        bool DestrotFsm<T>(string a_szName) where T : class;
        bool DestoryFsm(Type ownerType, string a_szName);
        bool DestoryFsm<T>(IFsm<T> fsm) where T : class;
        bool DestoryFsm(CFsmBase fsm);

        void GetAllFsms(List<CFsmBase> results);
    }
}
