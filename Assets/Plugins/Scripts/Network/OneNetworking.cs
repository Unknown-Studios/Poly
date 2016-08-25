using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace OneNetworking
{

	[Serializable]
	public class EncryptedMessage
	{
		public byte[] IV;
		public byte[] bytes;

		public EncryptedMessage(byte[] iv, byte[] Bytes) {
			IV = iv;
			bytes = Bytes;
		}

		public byte[] ToByteArray() {
			if (this == null)
				return null;
			BinaryFormatter bf = new BinaryFormatter();
			using (MemoryStream ms = new MemoryStream())
			{
				bf.Serialize(ms, this);
				return ms.ToArray();
			}
		}

		public static EncryptedMessage ToEM(byte[] bytes)
		{
			MemoryStream memStream = new MemoryStream();
			BinaryFormatter binForm = new BinaryFormatter();
			memStream.Write(bytes, 0, bytes.Length);
			memStream.Seek(0, SeekOrigin.Begin);
			EncryptedMessage em = (EncryptedMessage)binForm.Deserialize(memStream);
			return em;
		}
	}

	public class Encryption {
		public static EncryptedMessage EncryptBytes(string Key, byte[] message)
		{
			if (Key == null)
			{
				throw new ArgumentNullException("key");
			}
			byte[] key = System.Text.Encoding.ASCII.GetBytes (Key);
			byte[] IV;
			using (Aes aes = Aes.Create()) {
				aes.Key = key;
				aes.GenerateIV ();
				IV = aes.IV;
				using (var stream = new MemoryStream ()) {
					using (var encryptor = aes.CreateEncryptor ()) {
						using (var encrypt = new CryptoStream (stream, encryptor, CryptoStreamMode.Write)) {
							encrypt.Write (message, 0, message.Length);
							encrypt.FlushFinalBlock ();
							return new EncryptedMessage(IV, stream.ToArray ());
						}
					}
				}
			}
		}

		public static byte[] DecryptBytes(string Key, EncryptedMessage encMsg)
		{
			if (Key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (encMsg == null) {
				throw new ArgumentNullException("encMsg");
			}

			byte[] key = System.Text.Encoding.ASCII.GetBytes (Key);

			using (Aes aes = Aes.Create()) {
				aes.Key = key;
				aes.IV = encMsg.IV;
				using (var stream = new MemoryStream ()) {
					using (var decryptor = aes.CreateDecryptor ()) {
						using (var encrypt = new CryptoStream (stream, decryptor, CryptoStreamMode.Write)) {
							encrypt.Write (encMsg.bytes, 0, encMsg.bytes.Length);
							encrypt.FlushFinalBlock ();
							return stream.ToArray();
						}
					}
				}
			}
		}
	}

    public enum NetworkType { NotConnected = 0, Server = 1, Client = 2 };
	public enum NetworkMessageMode { Server = 0, Clients = 1, All = 2, Private = 3 };
	public enum NetworkMessageType { Connect = 0, Message = 1, Disconnect = 2 };
	public enum NetworkError { NotSpaceEnough = 0 };

    public class Packet
    {
        public byte[] bytes;
		public NetworkConnection connection;
		public bool Encrypt = true;

		public Packet(NetworkConnection Connection, byte[] Bytes, bool encrypt = true)
        {
			connection = Connection;
            bytes = Bytes;
			Encrypt = encrypt;
        }
    }


	public class EncryptKeyPair
	{
		public string privateKey;
		public string publicKey;

		public EncryptKeyPair(string PublicKey, string PrivateKey) {
			privateKey = PrivateKey;
			publicKey = PublicKey;
		}

		static public explicit operator string (EncryptKeyPair keys)
		{
			return "(public: "+keys.publicKey+", private: "+keys.privateKey+")";
		}
	}

	[Serializable]
	public class NetworkConnection {
		public IPAddress IP;
		public int Port;
		public string encryptionKey;

		public NetworkConnection(IPAddress ip, int port) {
			IP = ip;
			Port = port;
		}

		public NetworkConnection(IPAddress ip, int port, string encryption) {
			IP = ip;
			Port = port;
			encryptionKey = encryption;
		}

		public NetworkConnection(string ip, int port) {
			IP = IPAddress.Parse(ip);
			Port = port;
		}

		static public explicit operator IPEndPoint(NetworkConnection connection) {
			return new IPEndPoint (connection.IP, connection.Port);
		}

		static public explicit operator string (NetworkConnection connection)
		{
			return connection.IP+":"+connection.Port;
		}
	}

    [Serializable]
    public class NetworkMessage
    {
        public string UID;
		public NetworkMessageMode msgMode = NetworkMessageMode.Private;
        public NetworkMessageType msgType = NetworkMessageType.Message;
        public string methodName;
        public object[] parameters;
        public Type scriptType;
		public int Ping;

		public NetworkMessage(NetworkMessageType type, NetworkConnection connection) {
			msgType = type;
			parameters = new object[] { connection };
		}

		public NetworkMessage(Type st, string methodN, string uid, object[] Parameters)
        {
            UID = uid;
            scriptType = st;
            methodName = methodN;
            parameters = Parameters;
        }

		public NetworkMessage(Type st, NetworkMessageMode mode, string methodN, string uid, object[] Parameters)
		{
			msgMode = mode;
			UID = uid;
			scriptType = st;
			methodName = methodN;
			parameters = Parameters;
		}

        public static NetworkMessage ToNM(byte[] bytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(bytes, 0, bytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            NetworkMessage nm = (NetworkMessage)binForm.Deserialize(memStream);
            return nm;
        }

		public void Send(IPAddress IP, int port)
        {
            byte[] bytes = ToByteArray();

			Packet packet = new Packet(new NetworkConnection(IP, port), bytes);
			OneServer.queue.Enqueue(packet);
        }

        public byte[] ToByteArray()
        {
            if (this == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, this);
                return ms.ToArray();
            }
        }
    }
}