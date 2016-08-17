﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace OneNetworking
{
    public enum NetworkType { NotConnected = 0, Server = 1, Client = 2 };
	public enum NetworkMessageMode { Server = 0, Clients = 1, All = 2, Private = 3 };
	public enum NetworkMessageType { Connect = 0, Message = 1, Disconnect = 2 };

    public class Packet
    {
        public byte[] bytes;
        public IPEndPoint ipEnd;

        public Packet(IPEndPoint ipend, byte[] Bytes)
        {
            ipEnd = ipend;
            bytes = Bytes;
        }
    }

	[Serializable]
	public class NetworkConnection {
		public IPAddress IP;

		public NetworkConnection(IPAddress ip) {
			IP = ip;
		}

		public static NetworkConnection Parse(string IPAsString) {
			NetworkConnection nc = new NetworkConnection (IPAddress.Parse (IPAsString));
			return nc;
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

		public NetworkMessage(NetworkMessageType type, string clientIP) {
			msgType = type;
			parameters = new object[] { clientIP };
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

        public void Send(IPAddress IP)
        {
            byte[] bytes = ToByteArray();

			Packet packet = new Packet(new IPEndPoint(IP, OneServer.Port), bytes);
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