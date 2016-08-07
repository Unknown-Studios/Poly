using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace NetworkLibrary {
	public class Packet {
		public byte[] bytes;
		public IPEndPoint ipEnd;

		public Packet(IPEndPoint ipend, byte[] Bytes) {
			ipEnd = ipend;
			bytes = Bytes;
		}
	}

	[Serializable]
	public class NetworkMessage {
		public Type[] types;
		public string methodName;
		public object[] parameters;
		public Type scriptType;

		private NetworkMessage(Type st, Type[] Types, string methodN, object[] Parameters) {
			scriptType = st;
			types = Types;
			methodName = methodN;
			parameters = Parameters;
		}

		public static NetworkMessage ToNM(byte[] bytes)
		{
			MemoryStream memStream = new MemoryStream();
			BinaryFormatter binForm = new BinaryFormatter();
			memStream.Write(bytes, 0, bytes.Length);
			memStream.Seek(0, SeekOrigin.Begin);
			NetworkMessage nm = (NetworkMessage) binForm.Deserialize(memStream);
			return nm;
		}

		public byte[] ToByteArray()
		{
			if(this == null)
				return null;
			BinaryFormatter bf = new BinaryFormatter();
			using (MemoryStream ms = new MemoryStream())
			{
				bf.Serialize(ms, this);
				return ms.ToArray();
			}
		}

		public void SendMethod(IPAddress IP, string name) {
			MethodInfo mi = (new StackTrace().GetFrame(1).GetMethod().DeclaringType).GetMethod(name);
			ParameterInfo[] pi = mi.GetParameters();
			Type[] types = new Type[pi.Length];
			for (int i = 0; i < pi.Length; i++) {
				types[i] = pi[i].ParameterType;
			}
			NetworkMessage nm = new NetworkMessage(mi.DeclaringType, types, name, parameters);
			byte[] bytes = nm.ToByteArray();

			Packet packet = new Packet(new IPEndPoint(IP,  NetworkSystem.port), bytes);
			NetworkSystem.queue.Enqueue(packet);
		}

	}
}