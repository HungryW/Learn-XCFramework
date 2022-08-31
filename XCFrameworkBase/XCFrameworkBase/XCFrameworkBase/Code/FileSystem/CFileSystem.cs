using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace XCFrameworkBase
{
    /// <summary>
    /// 知识点
    /// Marshal类 主要是用来 托管数据和非托管数据的 交互和转换
    /// </summary>
    public sealed partial class CFileSystem : IFileSystem
    {
        private const int mc_nClusterSize = 1024 * 4;
        private const int mc_nCachedBytesLen = 0x1000;

        private static string[] ms_arrEmptyString = new string[] { };
        private static byte[] ms_arrCachedBytes = new byte[mc_nCachedBytesLen];

        private static int mc_nHeaderDataSize = Marshal.SizeOf(typeof(SHeaderData));
        private static int mc_nBlockDataSize = Marshal.SizeOf(typeof(SBlockData));
        private static int mc_nStringDataSize = Marshal.SizeOf(typeof(SStringData));

        private string m_szFullPath;
        private EFileSystemAccess m_eAccess;
        private CFileSystemStream m_stream;
        private Dictionary<String, int> m_mapFileDatas;
        private List<SBlockData> m_listBlockDatas;
        private GameFrameworkMultiDictionary<int, int> m_mapFreeBlockIdxs;
        private SortedDictionary<int, SStringData> m_mapStringData;
        private Queue<int> m_listFreeStringIdxs;
        private Queue<SStringData> m_listFreeStringDatas;

        private SHeaderData m_headerData;
        private int m_nBlockDataOffset;
        private int m_nStringDataOffset;
        private int m_nFileDataOffset;

        private CFileSystem(string a_szFullPath, EFileSystemAccess a_eAccesss, CFileSystemStream a_stream)
        {
            Debug.Assert(!string.IsNullOrEmpty(a_szFullPath));
            Debug.Assert(a_eAccesss != EFileSystemAccess.Unspecified);
            Debug.Assert(a_stream != null);

            m_szFullPath = a_szFullPath;
            m_eAccess = a_eAccesss;
            m_stream = a_stream;
            m_mapFileDatas = new Dictionary<string, int>();
            m_listBlockDatas = new List<SBlockData>();
            m_mapStringData = new SortedDictionary<int, SStringData>();
            m_listFreeStringIdxs = new Queue<int>();
            m_listFreeStringDatas = new Queue<SStringData>();
            m_mapFreeBlockIdxs = new GameFrameworkMultiDictionary<int, int>();

            CUtility.Marshal.EnsureCachedHGlobalSize(mc_nCachedBytesLen);
        }

        public void Shutdown()
        {
            m_stream.Close();
            m_mapFileDatas.Clear();
            m_listBlockDatas.Clear();
            m_mapFreeBlockIdxs.Clear();
            m_mapStringData.Clear();
            m_listFreeStringIdxs.Clear();
            m_listFreeStringDatas.Clear();

            m_nBlockDataOffset = 0;
            m_nStringDataOffset = 0;
            m_nFileDataOffset = 0;
        }

        public static CFileSystem Create(string a_szFullPath, EFileSystemAccess a_eAccess, CFileSystemStream a_stream, int a_nMaxFileCount, int a_nMaxBlockCount)
        {
            Debug.Assert(a_nMaxFileCount > 0);
            Debug.Assert(a_nMaxBlockCount > 0);
            Debug.Assert(a_nMaxFileCount <= a_nMaxBlockCount);

            CFileSystem fileSys = new CFileSystem(a_szFullPath, a_eAccess, a_stream);
            fileSys.m_headerData = new SHeaderData(a_nMaxFileCount, a_nMaxBlockCount);
            CUtility.Marshal.StructureToBytes(fileSys.m_headerData, mc_nHeaderDataSize, ms_arrCachedBytes);

            try
            {
                a_stream.Write(ms_arrCachedBytes, 0, mc_nHeaderDataSize);
                a_stream.SetLen(fileSys.m_nFileDataOffset);
                _CalcOffset(fileSys);
                return fileSys;
            }
            catch
            {
                fileSys.Shutdown();
                return null;
            }
        }

        public static CFileSystem Load(string a_szFullPath, EFileSystemAccess a_eAccess, CFileSystemStream a_stream)
        {
            CFileSystem fileSys = new CFileSystem(a_szFullPath, a_eAccess, a_stream);
            a_stream.Read(ms_arrCachedBytes, 0, mc_nHeaderDataSize);

            fileSys.m_headerData = CUtility.Marshal.BytesToStructure<SHeaderData>(mc_nHeaderDataSize, ms_arrCachedBytes);
            if (!fileSys.m_headerData.IsValid())
            {
                return null;
            }
            _CalcOffset(fileSys);

            if (fileSys.m_listBlockDatas.Capacity < fileSys.m_headerData.BlockCount)
            {
                fileSys.m_listBlockDatas.Capacity = fileSys.m_headerData.BlockCount;
            }
            for (int i = 0; i < fileSys.m_headerData.BlockCount; i++)
            {
                a_stream.Read(ms_arrCachedBytes, 0, mc_nBlockDataSize);
                SBlockData blockData = CUtility.Marshal.BytesToStructure<SBlockData>(mc_nBlockDataSize, ms_arrCachedBytes);
                fileSys.m_listBlockDatas.Add(blockData);
            }

            for (int i = 0; i < fileSys.m_listBlockDatas.Count; i++)
            {
                SBlockData blockData = fileSys.m_listBlockDatas[i];
                if (blockData.Using)
                {
                    SStringData stringData = fileSys._ReadStringData(blockData.StringIdx);
                    fileSys.m_mapStringData.Add(blockData.StringIdx, stringData);
                    fileSys.m_mapFileDatas.Add(stringData.GetString(fileSys.m_headerData.GetEncryptBytes()), i);
                }
                else
                {
                    fileSys.m_mapFreeBlockIdxs.Add(blockData.Len, i);
                }
            }

            int nIdx = 0;
            foreach (var stringData in fileSys.m_mapStringData)
            {
                while (nIdx < stringData.Key)
                {
                    fileSys.m_listFreeStringIdxs.Enqueue(nIdx++);
                }
                nIdx++;
            }
            return fileSys;
        }

        private static void _CalcOffset(CFileSystem a_fileSys)
        {
            a_fileSys.m_nBlockDataOffset = mc_nHeaderDataSize;
            a_fileSys.m_nStringDataOffset = a_fileSys.m_nBlockDataOffset + mc_nBlockDataSize * a_fileSys.m_headerData.MaxBlockCount;
            a_fileSys.m_nFileDataOffset = (int)_GetUpBoundClusterOffset(a_fileSys.m_nStringDataOffset + mc_nStringDataSize * a_fileSys.m_headerData.MaxFileCount);
        }

        private static long _GetUpBoundClusterOffset(long a_offset)
        {
            return (a_offset - 1 + mc_nClusterSize) / mc_nClusterSize * mc_nClusterSize;
        }

        public static int _GetUpBoundClusterCount(long a_len)
        {
            return (int)((a_len - 1 + mc_nClusterSize) / mc_nClusterSize);
        }

        private static long _GetClusterOffest(int a_nClusterIdx)
        {
            return (long)mc_nClusterSize * a_nClusterIdx;
        }

        private SStringData _ReadStringData(int a_nStringIdx)
        {
            m_stream.Position = m_nStringDataOffset + mc_nStringDataSize * a_nStringIdx;
            m_stream.Read(ms_arrCachedBytes, 0, mc_nStringDataSize);
            return CUtility.Marshal.BytesToStructure<SStringData>(mc_nStringDataSize, ms_arrCachedBytes);
        }

        public SFileInfo GetFileInfo(string a_szName)
        {
            Debug.Assert(!string.IsNullOrEmpty(a_szName));

            int nBlockIdx = 0;
            if (!m_mapFileDatas.TryGetValue(a_szName, out nBlockIdx))
            {
                return default(SFileInfo);
            }

            SBlockData blockData = m_listBlockDatas[nBlockIdx];
            return new SFileInfo(a_szName, _GetClusterOffest(blockData.ClusterIdx), blockData.Len);
        }

        public bool HasFile(string a_szName)
        {
            Debug.Assert(!string.IsNullOrEmpty(a_szName));
            return m_mapFileDatas.ContainsKey(a_szName);
        }

        public byte[] ReadFile(string a_szName)
        {
            Debug.Assert(!string.IsNullOrEmpty(a_szName));
            Debug.Assert(m_eAccess == EFileSystemAccess.Read || m_eAccess == EFileSystemAccess.ReadWrite);

            SFileInfo fileInfo = GetFileInfo(a_szName);
            if (!fileInfo.IsValid())
            {
                return null;
            }
            int nLen = fileInfo.Len;
            byte[] buffer = new byte[nLen];
            if (nLen > 0)
            {
                m_stream.Position = fileInfo.Offset;
                m_stream.Read(buffer, 0, nLen);
            }
            return buffer;
        }

        public int ReadFile(string a_szName, byte[] a_outArrBuffer, int a_nStartIdx, int a_nLen)
        {
            Debug.Assert(m_eAccess == EFileSystemAccess.Read || m_eAccess == EFileSystemAccess.ReadWrite);
            Debug.Assert(!string.IsNullOrEmpty(a_szName));
            Debug.Assert(a_outArrBuffer != null);
            Debug.Assert(a_nStartIdx >= 0);
            Debug.Assert(a_nLen >= 0);
            Debug.Assert(a_nStartIdx + a_nLen <= a_outArrBuffer.Length);

            SFileInfo fileInfo = GetFileInfo(a_szName);
            if (!fileInfo.IsValid())
            {
                return 0;
            }

            m_stream.Position = fileInfo.Offset;
            a_nLen = Math.Min(a_nLen, fileInfo.Len);
            if (a_nLen > 0)
            {
                return m_stream.Read(a_outArrBuffer, a_nStartIdx, a_nLen);
            }
            return 0;
        }

        public int Readfile(string a_szName, Stream a_outStream)
        {
            Debug.Assert(m_eAccess == EFileSystemAccess.Read || m_eAccess == EFileSystemAccess.ReadWrite);
            Debug.Assert(!string.IsNullOrEmpty(a_szName));
            Debug.Assert(a_outStream != null);
            Debug.Assert(a_outStream.CanWrite);

            SFileInfo fileInfo = GetFileInfo(a_szName);
            if (!fileInfo.IsValid())
            {
                return 0;
            }
            int nLen = fileInfo.Len;
            m_stream.Position = fileInfo.Offset;
            return m_stream.Read(a_outStream, nLen);
        }

        public byte[] ReadFileSegment(string a_szName, int a_nOffset, int a_nLen)
        {
            Debug.Assert(m_eAccess == EFileSystemAccess.Read || m_eAccess == EFileSystemAccess.ReadWrite);
            Debug.Assert(!string.IsNullOrEmpty(a_szName));
            Debug.Assert(a_nOffset >= 0);
            Debug.Assert(a_nLen >= 0);

            SFileInfo fileInfo = GetFileInfo(a_szName);
            if (!fileInfo.IsValid())
            {
                return null;
            }

            a_nOffset = Math.Min(a_nOffset, fileInfo.Len);
            a_nLen = Math.Min(a_nLen, fileInfo.Len - a_nOffset);
            if (a_nLen > 0)
            {
                byte[] buffer = new byte[a_nLen];
                m_stream.Position = fileInfo.Offset + a_nOffset;
                m_stream.Read(buffer, 0, a_nLen);
                return buffer;
            }
            else
            {
                return null;
            }
        }

        public int ReadFileSegment(string a_szName, int a_nOffset, byte[] a_outBuffer, int a_nStartIdx, int a_nLen)
        {
            Debug.Assert(m_eAccess == EFileSystemAccess.Read || m_eAccess == EFileSystemAccess.ReadWrite);
            Debug.Assert(!string.IsNullOrEmpty(a_szName));
            Debug.Assert(a_outBuffer != null);
            Debug.Assert(a_nOffset >= 0);
            Debug.Assert(a_nStartIdx >= 0);
            Debug.Assert(a_nLen >= 0);


            SFileInfo fileInfo = GetFileInfo(a_szName);
            if (!fileInfo.IsValid())
            {
                return 0;
            }
            a_nOffset = Math.Min(a_nOffset, fileInfo.Len);
            a_nLen = Math.Min(a_nLen, fileInfo.Len - a_nOffset);
            if (a_nLen > 0)
            {
                m_stream.Position = fileInfo.Offset + a_nOffset;
                return m_stream.Read(a_outBuffer, 0, a_nLen);
            }
            return 0;
        }

        public int ReadFileSegment(string a_szName, int a_nOffset, Stream a_outStream, int a_nLen)
        {
            Debug.Assert(m_eAccess == EFileSystemAccess.Read || m_eAccess == EFileSystemAccess.ReadWrite);
            Debug.Assert(!string.IsNullOrEmpty(a_szName));
            Debug.Assert(a_outStream != null);
            Debug.Assert(a_nOffset >= 0);
            Debug.Assert(a_nLen >= 0);

            SFileInfo fileInfo = GetFileInfo(a_szName);
            if (!fileInfo.IsValid())
            {
                return 0;
            }
            a_nOffset = Math.Min(a_nOffset, fileInfo.Len);
            a_nLen = Math.Min(a_nLen, fileInfo.Len - a_nOffset);
            if (a_nLen > 0)
            {
                m_stream.Position = fileInfo.Offset + a_nOffset;
                return m_stream.Read(a_outStream, a_nLen);
            }
            else
            {
                return 0;
            }
        }

        public bool WriteFile(string a_szName, byte[] a_buffer, int a_nStartIdx, int a_nLen)
        {
            Debug.Assert(m_eAccess == EFileSystemAccess.Write || m_eAccess == EFileSystemAccess.ReadWrite);
            Debug.Assert(!string.IsNullOrEmpty(a_szName));
            Debug.Assert(a_szName.Length <= byte.MaxValue);
            Debug.Assert(a_buffer != null);
            Debug.Assert(a_nStartIdx >= 0);
            Debug.Assert(a_nLen >= 0);
            Debug.Assert(a_nStartIdx + a_nLen <= a_buffer.Length);

            int nOldBlockIdx = -1;
            bool bHasFile = m_mapFileDatas.TryGetValue(a_szName, out nOldBlockIdx);

            if (!bHasFile && m_mapFileDatas.Count >= m_headerData.MaxFileCount)
            {
                return false;
            }

            int nNewBlockIdx = _AllocBlock(a_nLen);
            if (nNewBlockIdx < 0)
            {
                return false;
            }

            if (a_nLen > 0)
            {
                m_stream.Position = _GetClusterOffest(m_listBlockDatas[nNewBlockIdx].ClusterIdx);
                m_stream.Write(a_buffer, a_nStartIdx, a_nLen);
            }

            _ProcessWriteFile(a_szName, bHasFile, nOldBlockIdx, nNewBlockIdx, a_nLen);
            m_stream.Flush();
            return true;
        }
        private void _ProcessWriteFile(string a_szName, bool a_bHasFile, int a_nOldBlockIdx, int a_nNewBlockIdx, int a_nLen)
        {
        }

        private int _AllocBlock(int a_nLen)
        {
            if (a_nLen <= 0)
            {
                return _GetEmptyBlockIdx();
            }
            a_nLen = (int)_GetUpBoundClusterOffset(a_nLen);

            int nLenFound = -1;
            GameFrameworkLinkedListRange<int> lenRange = default(GameFrameworkLinkedListRange<int>);
            foreach (var i in m_mapFreeBlockIdxs)
            {
                if (i.Key < a_nLen)
                {
                    continue;
                }
                if (nLenFound >= 0 && nLenFound < i.Key)
                {
                    continue;
                }
                nLenFound = i.Key;
                lenRange = i.Value;
            }

            if (nLenFound >= 0)
            {
                int nBlockIdx = lenRange.First.Value;
                m_mapFreeBlockIdxs.Remove(nLenFound, nBlockIdx);
                SBlockData block = m_listBlockDatas[nBlockIdx];
                m_listBlockDatas[nBlockIdx] = new SBlockData(block.ClusterIdx, a_nLen);
                _WriteBlockData(nBlockIdx);

                int nLeftLen = nLenFound - a_nLen;
                int nNewBlockIdx = _GetEmptyBlockIdx();
                m_listBlockDatas[nNewBlockIdx] = new SBlockData(block.ClusterIdx + _GetUpBoundClusterCount(a_nLen), nLeftLen);
                m_mapFreeBlockIdxs.Add(nLeftLen, nNewBlockIdx);
                _WriteBlockData(nNewBlockIdx);

                return nBlockIdx;
            }
            else
            {
                int nBlockIdx = _GetEmptyBlockIdx();
                if (nBlockIdx < 0)
                {
                    return -1;
                }
                long nFileLen = m_stream.Len;
                try
                {
                    m_stream.SetLen(m_stream.Len + a_nLen);
                }
                catch
                {
                    return -1;
                }
                m_listBlockDatas[nBlockIdx] = new SBlockData(_GetUpBoundClusterCount(nFileLen), a_nLen);
                _WriteBlockData(nBlockIdx);
                return nBlockIdx;
            }
        }

        private bool _TryCombineFreeBlocks(int a_nFreeBlockIdx)
        {
            SBlockData freeBlock = m_listBlockDatas[a_nFreeBlockIdx];
            if (freeBlock.Len <= 0)
            {
                return false;
            }

            int nPreviousFreeBlockIdx = -1;
            int nNextFreeBlockIdx = -1;
            int nNextBlockDataClusterIdx = freeBlock.ClusterIdx + _GetUpBoundClusterCount(freeBlock.Len);
            foreach (var blockIdxs in m_mapFreeBlockIdxs)
            {
                if (blockIdxs.Key <= 0)
                {
                    continue;
                }

                int nClusterCount = _GetUpBoundClusterCount(blockIdxs.Key);
                foreach (int nBlockIdx in blockIdxs.Value)
                {
                    SBlockData blockData = m_listBlockDatas[nBlockIdx];
                    if (blockData.ClusterIdx + nClusterCount == freeBlock.ClusterIdx)
                    {
                        nPreviousFreeBlockIdx = nBlockIdx;
                    }
                    else if (blockData.ClusterIdx == nNextBlockDataClusterIdx)
                    {
                        nNextFreeBlockIdx = nBlockIdx;
                    }
                }
            }

            if (nPreviousFreeBlockIdx < 0 && nNextFreeBlockIdx < 0)
            {
                return false;
            }
            m_mapFreeBlockIdxs.Remove(freeBlock.Len, a_nFreeBlockIdx);
            if (nPreviousFreeBlockIdx >= 0)
            {
                SBlockData previousFreeBlockData = m_listBlockDatas[nPreviousFreeBlockIdx];
                m_mapFreeBlockIdxs.Remove(previousFreeBlockData.Len, nPreviousFreeBlockIdx);
                freeBlock = new SBlockData(previousFreeBlockData.ClusterIdx, previousFreeBlockData.Len + freeBlock.Len);
                m_listBlockDatas[nPreviousFreeBlockIdx] = SBlockData.Empty;
                m_mapFreeBlockIdxs.Add(0, nPreviousFreeBlockIdx);
                _WriteBlockData(nPreviousFreeBlockIdx);
            }

            if (nNextFreeBlockIdx >= 0)
            {
                SBlockData nextFreeBlockData = m_listBlockDatas[nNextFreeBlockIdx];
                m_mapFreeBlockIdxs.Remove(nextFreeBlockData.Len, nNextFreeBlockIdx);
                freeBlock = new SBlockData(nextFreeBlockData.ClusterIdx, nextFreeBlockData.Len + freeBlock.Len);
                m_listBlockDatas[nNextFreeBlockIdx] = SBlockData.Empty;
                m_mapFreeBlockIdxs.Add(0, nNextFreeBlockIdx);
                _WriteBlockData(nNextFreeBlockIdx);
            }
            m_listBlockDatas[a_nFreeBlockIdx] = freeBlock;
            m_mapFreeBlockIdxs.Add(freeBlock.Len, a_nFreeBlockIdx);
            _WriteBlockData(a_nFreeBlockIdx);
            return true;
        }

        private int _GetEmptyBlockIdx()
        {
            GameFrameworkLinkedListRange<int> lenRange = default(GameFrameworkLinkedListRange<int>);
            if (m_mapFreeBlockIdxs.TryGetValue(0, out lenRange))
            {
                int nBlockIdx = lenRange.First.Value;
                m_mapFreeBlockIdxs.Remove(0, nBlockIdx);
                return nBlockIdx;
            }

            if (m_listBlockDatas.Count < m_headerData.MaxBlockCount)
            {
                int nBlockIdx = m_listBlockDatas.Count;
                m_listBlockDatas.Add(SBlockData.Empty);
                _WriteHeaderData();
                return nBlockIdx;
            }
            return -1;
        }

        private int _AllocString(string a_szVal)
        {
            int nStringIdx = -1;
            SStringData stringData = default(SStringData);

            if (m_listFreeStringIdxs.Count > 0)
            {
                nStringIdx = m_listFreeStringIdxs.Dequeue();
            }
            else
            {
                nStringIdx = m_mapStringData.Count;
            }

            if (m_listFreeStringDatas.Count > 0)
            {
                stringData = m_listFreeStringDatas.Dequeue();
            }
            else
            {
                byte[] bytes = new byte[byte.MaxValue];
                CUtility.Random.GetRandomBytes(bytes);
                stringData = new SStringData(0, bytes);
            }

            stringData = stringData.SetString(a_szVal, m_headerData.GetEncryptBytes());
            m_mapStringData.Add(nStringIdx, stringData);
            _WriteStringData(nStringIdx, stringData);
            return nStringIdx;
        }

        private void _WriteHeaderData()
        {
            m_headerData = m_headerData.SetBlockCount(m_listBlockDatas.Count);
            CUtility.Marshal.StructureToBytes(m_headerData, mc_nHeaderDataSize, ms_arrCachedBytes);
            m_stream.Position = 0L;
            m_stream.Write(ms_arrCachedBytes, 0, mc_nHeaderDataSize);
        }

        private void _WriteBlockData(int a_nIdx)
        {
            CUtility.Marshal.StructureToBytes(m_listBlockDatas[a_nIdx], mc_nBlockDataSize, ms_arrCachedBytes);
            m_stream.Position = m_nBlockDataOffset + mc_nBlockDataSize * a_nIdx;
            m_stream.Write(ms_arrCachedBytes, 0, mc_nBlockDataSize);
        }

        private void _WriteStringData(int a_nStringIdx, SStringData a_stringData)
        {
            CUtility.Marshal.StructureToBytes(a_stringData, mc_nStringDataSize, ms_arrCachedBytes);
            m_stream.Position = m_nStringDataOffset + mc_nStringDataSize * a_nStringIdx;
            m_stream.Write(ms_arrCachedBytes, 0, mc_nStringDataSize);
        }

    }
}
