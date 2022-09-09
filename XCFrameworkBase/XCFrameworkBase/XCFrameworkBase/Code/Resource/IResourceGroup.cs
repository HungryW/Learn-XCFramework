using System.Collections.Generic;

namespace XCFrameworkBase
{
    public interface IResourceGroup
    {
        string Name { get; }

        bool Ready { get; }

        int TotalCount { get; }

        int ReadyCount { get; }

        long TotalLen { get; }

        long TotalCompressLen { get; }

        long ReadyCompressLen { get; }

        float Progress { get; }

        string[] GetResourceNames();

        void GetResourceNames(List<string> results);
    }
}
