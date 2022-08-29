using System;

namespace XCFrameworkBase
{
    public interface IFsm<T> where T : class
    {
        string Name { get; }
        string FullName { get; }
        T Owner { get; }
        int StateCount { get; }


        bool IsRunning { get; }
        bool IsDestoryed { get; }
        float CurrentStateTime { get; }

        CFsmState<T> CurState { get; }
        void Start<TState>() where TState : CFsmState<T>;
        void Start(Type stateType);

        bool HasState<TState>() where TState : CFsmState<T>;
        bool HasState(Type stateType);

        TState GetState<TState>() where TState : CFsmState<T>;
        CFsmState<T> GetState(Type stateType);

        CFsmState<T>[] GetAllStates();

        TData GetData<TData>(string a_szName) where TData : CVariable;
        CVariable GetData(string a_szName);

        void SetData<TData>(string a_szName, TData data) where TData : CVariable;
        void SetData(string a_szName, CVariable data);

        bool RemoveData(string a_szName);
    }
}
