using System;

namespace XCFrameworkBase
{
    public enum EStartTaskStatus : byte
    {
        Done = 0,
        CanResume,
        HasToWait,
        UnknownEror
    }

    /// <summary>
    /// TaskAgent的数据主要是 Task引用
    /// 定义的行为  是 开始一个Task,更新代理,重置代理
    /// </summary>

    public interface ITaskAgent<T> where T : CTaskBase
    {
        T Task
        {
            get;
        }

        void Init();

        EStartTaskStatus Start(T task);

        void Update(float a_fElapseSed, float a_fRealElapseSed);

        void ShutDown();

        void Reset();

    }
}
