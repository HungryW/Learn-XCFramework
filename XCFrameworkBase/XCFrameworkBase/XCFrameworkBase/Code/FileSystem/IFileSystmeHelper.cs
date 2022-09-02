using System;

namespace XCFrameworkBase
{
    public interface IFileSystmeHelper
    {
        CFileSystemStream CreateFileSystemStream(string a_szFullPath, EFileSystemAccess a_eAccess, bool a_bCreateNew);
    }
}
