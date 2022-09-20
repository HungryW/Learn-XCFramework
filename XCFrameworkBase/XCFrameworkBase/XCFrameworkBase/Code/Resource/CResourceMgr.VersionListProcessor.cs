using System;
using System.IO;

namespace XCFrameworkBase
{
    public partial class CResourceMgr
    {
        public enum ECheckVersionListResult : byte
        {
            Updated = 0,
            NeedUpdate
        }

        /// <summary>
        /// 作用是保证读写区的远程资源列表文件是最新的,并且解压资源列表文件
        /// 两个 接口
        /// 判断读写区的资源列表文件是不是最新的
        /// 下载最新的资源列表文件 
        /// </summary>
        private sealed class CVersionListProcessor
        {
            private readonly CResourceMgr m_refResMgr;
            private IDownloadMgr m_refDownloadMgr;
            private int m_nVersionListLen;
            private int m_nVersionListHashCode;
            private int m_nVersionListCompressLen;
            private int m_nVersionListCompressHashCode;

            public Action<string, string> m_fnOnVersionListUpdateSuccess;
            public Action<string, string> m_fnOnVersionListUpdateFail;


            public CVersionListProcessor(CResourceMgr a_refResMgr)
            {
                m_refResMgr = a_refResMgr;
                m_refDownloadMgr = null;
                m_nVersionListCompressHashCode = 0;
                m_nVersionListCompressLen = 0;
                m_nVersionListHashCode = 0;
                m_nVersionListLen = 0;

                m_fnOnVersionListUpdateFail = null;
                m_fnOnVersionListUpdateSuccess = null;
            }

            public void SetDownloadMgr(IDownloadMgr a_refDownloadMgr)
            {
                m_refDownloadMgr = a_refDownloadMgr;
            }

            public void Shutdown()
            {
                if (m_refDownloadMgr != null)
                {

                }
            }

            public ECheckVersionListResult CheckVersionList(int a_nLastInternalResVersion)
            {
                if (string.IsNullOrEmpty(m_refResMgr.m_szReadWritePath))
                {
                    throw new Exception("Read Write Path is Invalid");
                }

                string szVersionListFileName = CUtility.Path.GetRegularPath(Path.Combine(m_refResMgr.m_szReadWritePath, ms_szRemoteVersionListFileName));
                if (!File.Exists(szVersionListFileName))
                {
                    return ECheckVersionListResult.NeedUpdate;
                }

                int nIntervalResourceVersion = 0;
                FileStream fileStream = null;
                try
                {
                    fileStream = new FileStream(szVersionListFileName, FileMode.Open, FileAccess.Read);
                    object oInternalResourceVersion = null;
                    if (!m_refResMgr.m_UpdatableVersionSerialize.TryGetVal(fileStream, "InternalResourceVersion", out oInternalResourceVersion))
                    {
                        return ECheckVersionListResult.NeedUpdate;
                    }
                    nIntervalResourceVersion = (int)oInternalResourceVersion;
                }
                catch
                {
                    return ECheckVersionListResult.NeedUpdate;
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Dispose();
                        fileStream = null;
                    }
                }

                if (nIntervalResourceVersion != a_nLastInternalResVersion)
                {
                    return ECheckVersionListResult.NeedUpdate;
                }

                return ECheckVersionListResult.Updated;
            }

            public void UpdateVersionList(int a_nLen, int a_nHashCode, int a_nCompressLen, int a_nCompressHashCode)
            {
                if (m_refDownloadMgr == null)
                {
                    return;
                }

                m_nVersionListLen = a_nLen;
                m_nVersionListHashCode = a_nHashCode;
                m_nVersionListCompressLen = a_nCompressLen;
                m_nVersionListCompressHashCode = a_nCompressHashCode;


                int nDotPos = ms_szRemoteVersionListFileName.LastIndexOf('.');
                string szFileName = ms_szRemoteVersionListFileName.Substring(0, nDotPos);
                string szFileExtension = ms_szRemoteVersionListFileName.Substring(nDotPos + 1);
                string szLastVersionListFullNameWithCrc32 = CUtility.Text.Format("{0}.{1:x8}.{2}", szFileName, m_nVersionListHashCode, szFileExtension);
                string szDownLoadUri = CUtility.Path.GetRemotePath(Path.Combine(m_refResMgr.m_szUpdatePrefixUri, szLastVersionListFullNameWithCrc32));
                string szReadWriteVersionListFilePath = CUtility.Path.GetRegularPath(Path.Combine(m_refResMgr.m_szReadWritePath, ms_szRemoteVersionListFileName));

                m_refDownloadMgr.AddDownload(szReadWriteVersionListFilePath, szDownLoadUri, null, 0, this);
            }

            private void _OnDownLoadSuccess(object a_oSender, CDownloadSuccessEventArgs arg)
            {
                CVersionListProcessor processor = arg.UserData as CVersionListProcessor;
                if (processor == null || processor != this)
                {
                    return;
                }

                try
                {
                    using (FileStream fileStream = new FileStream(arg.DownloadPath, FileMode.Open, FileAccess.Read))
                    {
                        int nLen = (int)fileStream.Length;
                        if (nLen != m_nVersionListCompressLen)
                        {
                            fileStream.Close();
                            _OnDownloadFileCheckError(CUtility.Text.Format("VersionList Compressed Len Error Need Len={0},  DownloadLen ={1}", m_nVersionListCompressLen, nLen), arg);
                            return;
                        }
                        fileStream.Position = 0L;
                        int nHashCode = CUtility.Verifier.GetCrc32(fileStream);
                        if (nHashCode != m_nVersionListCompressHashCode)
                        {
                            fileStream.Close();
                            _OnDownloadFileCheckError("Compress Hash Code Invalid", arg);
                            return;
                        }

                        fileStream.Position = 0L;
                        m_refResMgr._PrepareCacheStream();
                        if (!CUtility.Compression.Decompress(fileStream, m_refResMgr.m_streamCached))
                        {
                            fileStream.Close();
                            _OnDownloadFileCheckError("Decompress Fail", arg);
                            return;
                        }

                        int nUncompressLen = (int)m_refResMgr.m_streamCached.Length;
                        if (nUncompressLen != m_nVersionListLen)
                        {
                            fileStream.Close();
                            _OnDownloadFileCheckError("Len Invalid", arg);
                            return;
                        }

                        fileStream.Position = 0L;
                        fileStream.SetLength(0L);
                        fileStream.Write(m_refResMgr.m_streamCached.GetBuffer(), 0, nUncompressLen);

                        m_fnOnVersionListUpdateSuccess?.Invoke(arg.DownloadPath, arg.DownloadUri);
                    }
                }
                catch (Exception ex)
                {
                    _OnDownloadFileCheckError(ex.ToString(), arg);
                }
            }

            private void _OnDownloadFileCheckError(string a_szErrorMsg, CDownloadSuccessEventArgs arg)
            {
                CDownloadFailEventArgs args = CDownloadFailEventArgs.Create(arg.nId, arg.DownloadPath, arg.DownloadUri, arg.UserData, a_szErrorMsg);
                _OnDownloadFail(this, args);
                CReferencePool.Release(args);
            }

            private void _OnDownloadFail(object a_oSender, CDownloadFailEventArgs a_arg)
            {
                CVersionListProcessor processor = a_arg.UserData as CVersionListProcessor;
                if (processor == null || processor != this)
                {
                    return;
                }
                if (File.Exists(a_arg.DownloadPath))
                {
                    File.Delete(a_arg.DownloadPath);
                }
                m_fnOnVersionListUpdateFail?.Invoke(a_arg.DownloadPath, a_arg.DownloadUri);
            }
        }
    }
}
