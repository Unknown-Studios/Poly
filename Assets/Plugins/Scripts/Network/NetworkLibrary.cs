using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace NetworkLibrary
{
    public enum NetworkType { Server = 0, Client = 1 };

    public enum NetworkMessageType { Connect = 0, Message = 1, Disconnect = 2 }

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
    public class NetworkMessage
    {
        public string UID;
        public NetworkMessageType msgType = NetworkMessageType.Message;
        public Type[] types;
        public string methodName;
        public object[] parameters;
        public Type scriptType;

        private NetworkMessage(Type st, Type[] Types, string methodN, string uid, object[] Parameters)
        {
            UID = uid;
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
            NetworkMessage nm = (NetworkMessage)binForm.Deserialize(memStream);
            return nm;
        }

        public static void SendMethod(IPAddress IP, string name, params object[] parameters)
        {
            Type ty = (new StackTrace().GetFrame(1).GetMethod().DeclaringType);
            MethodInfo mi = ty.GetMethod(name);
            ParameterInfo[] pi = mi.GetParameters();
            Type[] types = new Type[pi.Length];
            for (int i = 0; i < pi.Length; i++)
            {
                types[i] = pi[i].ParameterType;
            }
            string UID = "";
            foreach (Attribute a in mi.GetCustomAttributes(false))
            {
                if (a.GetType() == typeof(NetworkMethodAttribute))
                {
                    NetworkMethodAttribute na = (NetworkMethodAttribute)a;
                    UID = na.UID;
                }
            }

            NetworkMessage nm = new NetworkMessage(mi.DeclaringType, types, name, UID, parameters);
            byte[] bytes = nm.ToByteArray();

            Packet packet = new Packet(new IPEndPoint(IP, NetworkSystem.port), bytes);
            NetworkSystem.queue.Enqueue(packet);
        }

        public static void SendMethod(IPAddress IP, NetworkMessageType MsgType, string name, params object[] parameters)
        {
            MethodInfo mi = (new StackTrace().GetFrame(1).GetMethod().DeclaringType).GetMethod(name);
            ParameterInfo[] pi = mi.GetParameters();
            Type[] types = new Type[pi.Length];
            for (int i = 0; i < pi.Length; i++)
            {
                types[i] = pi[i].ParameterType;
            }
            string UID = "";
            foreach (Attribute a in mi.GetCustomAttributes(false))
            {
                if (a.GetType() == typeof(NetworkMethodAttribute))
                {
                    NetworkMethodAttribute na = (NetworkMethodAttribute)a;
                    UID = na.UID;
                }
            }

            NetworkMessage nm = new NetworkMessage(mi.DeclaringType, types, name, UID, parameters);
            nm.msgType = MsgType;

            nm.Send(IP);
        }

        public void Send(IPAddress IP)
        {
            byte[] bytes = ToByteArray();

            Packet packet = new Packet(new IPEndPoint(IP, NetworkSystem.port), bytes);
            NetworkSystem.queue.Enqueue(packet);
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

    public sealed class NetworkMethodAttribute : Attribute
    {
        public string UID = "";

        public NetworkMethodAttribute()
        {
        }
    }
}