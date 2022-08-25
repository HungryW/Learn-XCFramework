using System;
using System.IO;

namespace XCFrameworkBase
{
    public sealed partial class CDownloadMgr : CGameFrameworkModule, IDownloadMgr
    {
        private sealed class CDownloadAgent : ITaskAgent<CDownloadTask>, IDisposable
        {
            private readonly IDownloadAgentHelper m_helper;
            private CDownloadTask m_task;
            private FileStream m_fileStream;
            private int m_nWaitFlushSize;
            private long m_lStartLen;
            private long m_lDownloadLen;
            private long m_lSaveLen;
            private bool m_bDisposed;
            private float m_fWaitTime;

            private Action<CDownloadAgent> m_fnOnStart;
            private Action<CDownloadAgent, int> m_fnOnUpdate;
            private Action<CDownloadAgent, long> m_fnOnSuccess;
            private Action<CDownloadAgent, string> m_fnOnFail;

            public CDownloadAgent(IDownloadAgentHelper a_helper)
            {
                if (null == a_helper)
                {
                    throw new Exception("Download Agent Helper Invalid");
                }

                m_helper = a_helper;
                m_task = null;
                m_fileStream = null;
                m_nWaitFlushSize = 0;
                m_lStartLen = 0;
                m_lDownloadLen = 0;
                m_lStartLen = 0;
                m_fWaitTime = 0;
                m_bDisposed = false;

                m_fnOnStart = null;
                m_fnOnUpdate = null;
                m_fnOnSuccess = null;
                m_fnOnFail = null;
            }

            public CDownloadTask Task
            {
                get
                {
                    return m_task;
                }
            }

            public void InitCallbacks(Action<CDownloadAgent> a_fnStart, Action<CDownloadAgent, int> a_fnUpdate, Action<CDownloadAgent, long> a_fnSuccess, Action<CDownloadAgent, string> a_fnFail)
            {
                m_fnOnStart = a_fnStart;
                m_fnOnUpdate = a_fnUpdate;
                m_fnOnSuccess = a_fnSuccess;
                m_fnOnFail = a_fnFail;
            }

            public void Init()
            {
                m_helper.EventUpdateBytes += _OnHelperUpdateBytes;
                m_helper.EventUpdateLen += _OnHelperUpdateLen;
                m_helper.EventComplete += _OnHelperComplete;
                m_helper.EventFail += _OnHelperFail;
            }


            public void ShutDown()
            {
                Dispose();
                m_helper.EventUpdateBytes -= _OnHelperUpdateBytes;
                m_helper.EventUpdateLen -= _OnHelperUpdateLen;
                m_helper.EventComplete -= _OnHelperComplete;
                m_helper.EventFail -= _OnHelperFail;
            }

            public void Reset()
            {
                m_helper.Reset();

                if (m_fileStream != null)
                {
                    m_fileStream.Close();
                    m_fileStream = null;
                }

                m_task = null;
                m_nWaitFlushSize = 0;
                m_fWaitTime = 0;

                m_lStartLen = 0;
                m_lDownloadLen = 0;
                m_lSaveLen = 0;
            }

            public void Dispose()
            {
                _Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void _Dispose(bool a_bDisposing)
            {
                if (m_bDisposed)
                {
                    return;
                }
                if (a_bDisposing)
                {
                    if (m_fileStream != null)
                    {
                        m_fileStream.Dispose();
                        m_fileStream = null;
                    }
                }
                m_bDisposed = true;
            }

            public void Update(float a_fElapseSed, float a_fRealElapseSed)
            {
                if (m_task.Status == EDownloadStatus.Doing)
                {
                    m_fWaitTime += a_fRealElapseSed;
                    if (m_fWaitTime >= m_task.Timeout)
                    {
                        m_fWaitTime = 0;
                        _ToDoHelperFail("Timeout");
                    }
                }
            }


            public EStartTaskStatus Start(CDownloadTask a_task)
            {
                if (null == a_task)
                {
                    throw new Exception("Task is invalid");
                }
                m_task = a_task;
                m_task.SetStatus(EDownloadStatus.Doing);
                string szDownloadFile = CUtility.Text.Format("{0}.download", m_task.DownloadPath);

                try
                {
                    if (File.Exists(szDownloadFile))
                    {
                        m_fileStream = File.OpenWrite(szDownloadFile);
                        m_fileStream.Seek(0, SeekOrigin.End);
                        m_lStartLen = m_lSaveLen = m_fileStream.Length;
                        m_lDownloadLen = 0L;
                    }
                    else
                    {
                        string szDic = Path.GetDirectoryName(m_task.DownloadPath);
                        if (!Directory.Exists(szDic))
                        {
                            Directory.CreateDirectory(szDic);
                        }
                        m_fileStream = new FileStream(szDownloadFile, FileMode.Create, FileAccess.Write);
                        m_lStartLen = m_lSaveLen = m_lDownloadLen = 0;
                    }

                    m_fnOnStart?.Invoke(this);

                    if (m_lStartLen > 0)
                    {
                        m_helper.Download(m_task.DownloadUri, m_lStartLen, m_task.UserData);
                    }
                    else
                    {
                        m_helper.Download(m_task.DownloadUri, m_task.UserData);
                    }
                    return EStartTaskStatus.CanResume;
                }
                catch (Exception exception)
                {
                    _ToDoHelperFail(exception.ToString());
                    return EStartTaskStatus.UnknownEror;
                }
            }

            private void _OnHelperUpdateBytes(object a_oSender, CDownloadAgentHelperUpdateBytesEventArgs a_arg)
            {
                m_fWaitTime = 0f;
                try
                {
                    m_fileStream.Write(a_arg.GetBytes(), a_arg.Offset, a_arg.Len);
                    m_nWaitFlushSize += a_arg.Len;
                    m_lSaveLen += a_arg.Len;
                    if (m_nWaitFlushSize >= m_task.FlushSize)
                    {
                        m_fileStream.Flush();
                        m_nWaitFlushSize = 0;
                    }
                }
                catch (Exception exception)
                {
                    _ToDoHelperFail(exception.ToString());
                }
            }

            private void _OnHelperUpdateLen(object a_oSender, CDownloadAgentHelperUpdateLenEventArgs a_arg)
            {
                m_fWaitTime = 0f;
                m_lDownloadLen += a_arg.DeltaLen;
                m_fnOnUpdate?.Invoke(this, a_arg.DeltaLen);
            }

            private void _OnHelperComplete(object a_oSender, CDownloadAgentHelperCompleteEventArgs a_args)
            {
                m_fWaitTime = 0f;
                m_lDownloadLen = a_args.Len;
                if (m_lSaveLen != (m_lStartLen + m_lDownloadLen))
                {
                    throw new Exception("internal download error");
                }
                m_helper.Reset();
                m_fileStream.Close();
                m_fileStream = null;

                if (File.Exists(m_task.DownloadPath))
                {
                    File.Delete(m_task.DownloadPath);
                }

                File.Move(CUtility.Text.Format("{0}.download", m_task.DownloadPath), m_task.DownloadPath);
                m_task.SetStatus(EDownloadStatus.Done);
                m_fnOnSuccess?.Invoke(this, a_args.Len);
                m_task.Done = true;
            }

            private void _ToDoHelperFail(string a_szErrorMsg)
            {
                CDownloadAgentHelperErrorEventArgs arg = CDownloadAgentHelperErrorEventArgs.Create(false, a_szErrorMsg);
                _OnHelperFail(this, arg);
                CReferencePool.Release(arg);
            }

            private void _OnHelperFail(object a_oSender, CDownloadAgentHelperErrorEventArgs a_arg)
            {
                m_helper.Reset();
                if (m_fileStream != null)
                {
                    m_fileStream.Close();
                    m_fileStream = null;
                }

                if (a_arg.DeleteDownloading)
                {
                    File.Delete(CUtility.Text.Format("{0}.download", m_task.DownloadPath));
                }

                m_task.SetStatus(EDownloadStatus.Error);

                m_fnOnFail?.Invoke(this, a_arg.ErrorMessage);
                m_task.Done = true;
            }


        }
    }
}
