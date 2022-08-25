using System;

namespace XCFrameworkBase
{
    public sealed partial class CDownloadMgr : CGameFrameworkModule, IDownloadMgr
    {
        private enum EDownloadStatus
        {
            Todo = 0,
            Doing,
            Done,
            Error,
        }

        private sealed class CDownloadTask : CTaskBase
        {
            private static int ms_nIdSeed = 0;

            private EDownloadStatus m_eStatus;
            private string m_szDownloadPath;
            private string m_szDownloadUri;
            private int m_nFlushSize;
            private float m_fTimeout;

            public CDownloadTask()
            {
                m_eStatus = EDownloadStatus.Todo;
                m_szDownloadPath = null;
                m_szDownloadUri = null;
                m_nFlushSize = 0;
                m_fTimeout = 0;
            }

            public override void Clear()
            {
                base.Clear();
                m_eStatus = EDownloadStatus.Todo;
                m_szDownloadPath = null;
                m_szDownloadUri = null;
                m_nFlushSize = 0;
                m_fTimeout = 0;
            }

            public void SetStatus(EDownloadStatus a_eStatus)
            {
                m_eStatus = a_eStatus;
            }

            public EDownloadStatus Status
            {
                get
                {
                    return m_eStatus;
                }
            }
            /// <summary>
            /// 获取下载后存放路径。
            /// </summary>
            public string DownloadPath
            {
                get
                {
                    return m_szDownloadPath;
                }
            }

            /// <summary>
            /// 获取原始下载地址。
            /// </summary>
            public string DownloadUri
            {
                get
                {
                    return m_szDownloadUri;
                }
            }

            /// <summary>
            /// 获取将缓冲区写入磁盘的临界大小。
            /// </summary>
            public int FlushSize
            {
                get
                {
                    return m_nFlushSize;
                }
            }

            /// <summary>
            /// 获取下载超时时长，以秒为单位。
            /// </summary>
            public float Timeout
            {
                get
                {
                    return m_fTimeout;
                }
            }

            /// <summary>
            /// 获取下载任务的描述。
            /// </summary>
            public override string Description
            {
                get
                {
                    return m_szDownloadPath;
                }
            }

            public static CDownloadTask Create(string downloadPath, string downloadUri, string tag, int priority, int flushSize, float timeout, object userData)
            {
                CDownloadTask task = CReferencePool.Acquire<CDownloadTask>();
                task.Initialize(++ms_nIdSeed, tag, priority, userData);
                task.m_fTimeout = timeout;
                task.m_nFlushSize = flushSize;
                task.m_szDownloadPath = downloadPath;
                task.m_szDownloadUri = downloadUri;
                return task;
            }
        }
    }
}
