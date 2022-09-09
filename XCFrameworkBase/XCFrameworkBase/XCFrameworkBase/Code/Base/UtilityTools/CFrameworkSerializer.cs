using System.Collections.Generic;
using System.IO;

namespace XCFrameworkBase
{
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
