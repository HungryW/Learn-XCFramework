using System;

namespace XCFrameworkBase
{
    public interface IResourceGroupCollection
    {
        bool Ready { get; }

        int TotalCount { get; }

        int ReadyCount { get; }

        long TotalLen { get; }

        long TotalCompressLen { get; }

        long ReadyLen { get; }

        long ReadyCompressLen { get; }

        float Progress { get; }

        IResourceGroup[] GetResourceGroups();

        string[] GetResourceNames();

    }
}
