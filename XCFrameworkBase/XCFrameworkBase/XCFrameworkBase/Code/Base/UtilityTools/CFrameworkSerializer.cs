using System.Collections.Generic;
using System.IO;

namespace XCFrameworkBase
{
    /// <summary>
    /// 序列化和反序列化的基类
    /// 包含了公用的头部数据 头标识字节 和 版本号
    /// 提供了校验头标识 和 管理不同版本序列化和反序列化回调
    /// 序列化接口 参数是 数据结构实例,要写入的二进制流,版本号   将数据结构转化为二进制流
    /// 反序列化接口 参数是 要读取的二进制流, 返回数据结构实例, 流中有版本信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CFrameworkSerializer<T>
    {
        public delegate bool SerializeCallback(Stream stream, T data);
        public delegate T DeserializeCallback(Stream stream);
        public delegate bool TryGetValCallback(Stream stream, string a_szKey, out object val);

        private readonly Dictionary<byte, SerializeCallback> m_mapSerializeCallbacks;
        private readonly Dictionary<byte, DeserializeCallback> m_mapDeserializeCallbacks;
        private readonly Dictionary<byte, TryGetValCallback> m_mapTryGetValCallbacks;
        private byte m_nLastSerializeCallbackVersion;

        public CFrameworkSerializer()
        {
            m_mapSerializeCallbacks = new Dictionary<byte, SerializeCallback>();
            m_mapDeserializeCallbacks = new Dictionary<byte, DeserializeCallback>();
            m_mapTryGetValCallbacks = new Dictionary<byte, TryGetValCallback>();
            m_nLastSerializeCallbackVersion = 0;
        }

        public void RegisterSerializeCallback(byte a_nVersion, SerializeCallback a_fn)
        {
            m_mapSerializeCallbacks[a_nVersion] = a_fn ?? throw new GameFrameworkException("Serialize callback is invalid");
            if (a_nVersion > m_nLastSerializeCallbackVersion)
            {
                m_nLastSerializeCallbackVersion = a_nVersion;
            }
        }

        public void RegisterDeserializeCallback(byte a_nVersion, DeserializeCallback a_fn)
        {
            m_mapDeserializeCallbacks[a_nVersion] = a_fn ?? throw new GameFrameworkException("Deserialize callback is invalid");
        }

        public void RegisterTryGetValCallbacks(byte a_nVersion, TryGetValCallback a_fn)
        {
            m_mapTryGetValCallbacks[a_nVersion] = a_fn ?? throw new GameFrameworkException("TryGetVal callback is invalid");
        }

        public bool Serialize(Stream stream, T data)
        {
            if (m_mapSerializeCallbacks.Count <= 0)
            {
                throw new GameFrameworkException("No serialize callback registered");
            }

            return Serialize(stream, data, m_nLastSerializeCallbackVersion);
        }

        public bool Serialize(Stream stream, T data, byte a_nVersion)
        {
            byte[] headerBuffer = __GetHeader();
            stream.WriteByte(headerBuffer[0]);
            stream.WriteByte(headerBuffer[1]);
            stream.WriteByte(headerBuffer[2]);
            stream.WriteByte(a_nVersion);
            SerializeCallback callback = null;
            if (!m_mapSerializeCallbacks.TryGetValue(a_nVersion, out callback))
            {
                throw new GameFrameworkException(CUtility.Text.Format("Serialize callback '{0}' is invalid", a_nVersion));
            }

            return callback(stream, data);
        }

        public T Deserialize(Stream stream)
        {
            byte[] headerStandard = __GetHeader();
            byte[] header = new byte[3];
            for (int i = 0; i < header.Length; i++)
            {
                header[i] = (byte)stream.ReadByte();
            }
            for (int i = 0; i < header.Length; i++)
            {
                if (header[i] != headerStandard[i])
                {
                    throw new GameFrameworkException("Header is invalid");
                }
            }
            byte a_nVersion = (byte)stream.ReadByte();
            DeserializeCallback callback = null;
            if (!m_mapDeserializeCallbacks.TryGetValue(a_nVersion, out callback))
            {
                throw new GameFrameworkException("Deserialize callback is null");
            }
            return callback(stream);
        }

        public bool TryGetVal(Stream stream, string a_szKey, out object val)
        {
            val = null;
            byte[] headerStandard = __GetHeader();
            byte[] header = new byte[3];
            for (int i = 0; i < header.Length; i++)
            {
                header[i] = (byte)stream.ReadByte();
            }
            for (int i = 0; i < header.Length; i++)
            {
                if (header[i] != headerStandard[i])
                {
                    throw new GameFrameworkException("Header is invalid");
                }
            }
            byte a_nVersion = (byte)stream.ReadByte();
            TryGetValCallback callback = null;
            if (!m_mapTryGetValCallbacks.TryGetValue(a_nVersion, out callback))
            {
                return false;
            }

            return callback(stream, a_szKey, out val);
        }

        protected abstract byte[] __GetHeader();
    }
}
