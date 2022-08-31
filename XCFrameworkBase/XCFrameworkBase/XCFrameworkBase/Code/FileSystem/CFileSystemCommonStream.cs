using System;
using System.IO;

namespace XCFrameworkBase
{
    public sealed class CFileSystemCommonStream : CFileSystemStream, IDisposable
    {
        private FileStream m_fileStream;

        public CFileSystemCommonStream(string a_szFullPath, EFileSystemAccess a_eAccess, bool a_bCreateNew)
        {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(a_szFullPath));
            switch (a_eAccess)
            {
                case EFileSystemAccess.Read:
                    m_fileStream = new FileStream(a_szFullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    break;
                case EFileSystemAccess.Write:
                    m_fileStream = new FileStream(a_szFullPath, a_bCreateNew ? FileMode.Create : FileMode.Open, FileAccess.Write, FileShare.Read);
                    break;
                case EFileSystemAccess.ReadWrite:
                    m_fileStream = new FileStream(a_szFullPath, a_bCreateNew ? FileMode.Create : FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                    break;
                default:
                    throw new Exception("access is invalid");
            }
        }

        public override long Position
        {
            get => m_fileStream.Position;
            set => m_fileStream.Position = value;
        }

        public override long Len => m_fileStream.Length;

        public override void SetLen(long a_len)
        {
            m_fileStream.SetLength(a_len);
        }

        public override void Seek(long a_offset, SeekOrigin a_origin)
        {
            m_fileStream.Seek(a_offset, a_origin);
        }

        public override int ReadByte()
        {
            return m_fileStream.ReadByte();
        }

        public override int Read(byte[] buffer, int a_nStartIdx, int a_nLen)
        {
            return m_fileStream.Read(buffer, a_nStartIdx, a_nLen);
        }

        public override void WriteByte(byte val)
        {
            m_fileStream.WriteByte(val);
        }

        public override void Write(byte[] a_arrBuff, int a_nStartIdx, int a_nLen)
        {
            m_fileStream.Write(a_arrBuff, a_nStartIdx, a_nLen);
        }

        public override void Flush()
        {
            m_fileStream.Flush();
        }

        public override void Close()
        {
            m_fileStream.Close();
        }

        public void Dispose()
        {
            m_fileStream.Dispose();
        }
    }
}
