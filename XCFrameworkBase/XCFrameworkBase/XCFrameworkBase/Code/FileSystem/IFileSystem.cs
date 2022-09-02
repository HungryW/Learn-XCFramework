using System.IO;
using System.Collections.Generic;

namespace XCFrameworkBase
{
    public enum EFileSystemAccess
    { /// <summary>
      /// 未指定。
      /// </summary>
        Unspecified = 0,

        /// <summary>
        /// 只可读。
        /// </summary>
        Read = 1,

        /// <summary>
        /// 只可写。
        /// </summary>
        Write = 2,

        /// <summary>
        /// 可读写。
        /// </summary>
        ReadWrite = 3
    }


    public interface IFileSystem
    {
        string FullPath
        {
            get;
        }

        EFileSystemAccess Access
        {
            get;
        }

        int FileCount
        {
            get;
        }

        int MaxFileCount
        {
            get;
        }

        bool HasFile(string a_szName);

        int ReadFile(string a_szName, byte[] a_outArrBuffer, int a_nStartIdx, int a_nLen);
        int ReadFile(string a_szName, Stream a_outStream);

        int ReadFileSegement(string a_szName, byte[] a_outArrBuffer, int a_nStartIdx, int a_nOffset, int a_nLen);
        int ReadFileSegement(string a_szName, Stream a_outStream, int a_nOffset, int a_nLen);

        bool WriteFile(string a_szName, byte[] a_arrBuffer, int a_nStartIdx, int a_nLen);
        bool WriteFile(string a_szName, Stream a_stream);
        bool WriteFile(string a_szName, string a_szFilePath);

        bool SaveAsFile(string a_szName, string a_szFilePath);

        bool RenameFile(string a_szOldName, string a_szNewName);
        bool DelFile(string a_szName);

    }
}
