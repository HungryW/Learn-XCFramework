using System;

namespace XCFrameworkBase
{
    /// <summary>
    /// 其他知识
    /// event 是对delegate进行封装的语法糖,编译器会生成一个私有的delegate变量,和两个方法(add,remove)
    /// 提供public event可以理解为这个类的返回值(这个功能的返回值)
    /// 会在特定的时间调用,有特定的数据
    /// </summary>
    public interface IDownloadMgr
    {
        event EventHandler<CDownloadStartEventArgs> DownloadStart;
        event EventHandler<CDownloadUpadateEventArgs> DownloadUpdate;
        event EventHandler<CDownloadSuccessEventArgs> DownloadSuccess;
        event EventHandler<CDownloadFailEventArgs> DownloadFail;

        void AddDownloadAgentHelper(IDownloadAgentHelper a_helper);

        int AddDownload(string a_szDownloadPath, string a_szDownloadUri, string a_szTag, int a_nPriority, object a_oUserData);

        bool RemoveDownload(int a_nId);
        int RemoveDownloads(string a_szTag);
        int RemoveAllDownloads();

        bool Paused
        {
            get;
            set;
        }

        int FlushSize
        {
            get;
            set;
        }

        float Timeout
        {
            get;
            set;
        }

        float CurrentSpeed
        {
            get;
        }
    }
}
