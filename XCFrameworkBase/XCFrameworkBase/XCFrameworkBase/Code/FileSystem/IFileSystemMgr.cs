using System;

namespace XCFrameworkBase
{
    public interface IFileSystemMgr
    {
        int Count { get; }

        void SetFileSystemHelper(IFileSystmeHelper a_helper);

        bool HasFileSystem(string a_szFullPath);

        IFileSystem GetFileSystem(string a_szFullPath);

        IFileSystem CreateFileSystem(string a_szFullPath, EFileSystemAccess a_eAccess, int a_nMaxFileCount, int a_nMaxBlockCount);

        IFileSystem LoadFileSystem(string a_szFullPath, EFileSystemAccess a_eAccess);

        void DestroyFileSystem(IFileSystem a_fileSystem, bool a_bDelePhysicalFile);

    }
}
