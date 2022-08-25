using System;

namespace XCFrameworkBase
{
    public interface IWebRequestMgr
    {
        float TimeOut
        {
            get;
            set;
        }


        event EventHandler<CWebRequestStartEventArgs> WebRequestStart;
        event EventHandler<CWebRequestSuccessEventArgs> WebRequestSuccess;
        event EventHandler<CWebRequestFailEventArgs> WebRequestFail;

        void AddWebRequestAgentHelper(IWebRequestAgentHelper a_helper);

        int AddWebRequest(string a_szRequestUri);
        int AddWebRequest(string a_szRequestUri, byte[] a_arrPostData);
        int AddWebRequest(string a_szRequestUri, byte[] a_arrPostData, string a_szTag, int a_nPriority, object a_oUserData);

        bool RemoveWebRequest(int a_nId);
        int RemoveWebRequest(string a_szTag);
        int RemoveAllWebRequest();
    }
}
