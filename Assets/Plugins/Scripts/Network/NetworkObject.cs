using OneNetworking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class NetworkObject : MonoBehaviour
{
    [HideInInspector]
    public string UID;

    [HideInInspector]
    public bool isMine = false;

    public void SendMethod(NetworkMessageMode mode, string name, params object[] parameters)
    {
        if (mode == NetworkMessageMode.Private)
        {
            Debug.LogError("Please use SendMethod(NetworkConnection, string, params object[]) for private messages!");
            return;
        }
        Type ty = (new StackTrace().GetFrame(1).GetMethod().DeclaringType);
        MethodInfo mi = ty.GetMethod(name);

        List<object> param = new List<object>();
        param.Add(new NetworkConnection(OneServer.publicIP, OneServer.Port));
        foreach (object o in parameters)
        {
            param.Add(o);
        }

        NetworkMessage nm = new NetworkMessage(mi.DeclaringType, mode, name, UID, param.ToArray());
        byte[] bytes = nm.ToByteArray();

        if (OneServer.networkType == NetworkType.Server)
        {
            Packet packet = new Packet(new IPEndPoint(IPAddress.Parse("127.0.0.1"), OneServer.sendPort), bytes);
            OneServer.queue.Enqueue(packet);
        }
        else
        {
            Packet packet = new Packet(new IPEndPoint(OneServer.connections[0].IP, OneServer.sendPort), bytes);
            OneServer.queue.Enqueue(packet);
        }
    }

    public void SendMethod(NetworkConnection connection, string name, params object[] parameters)
    {
        Type ty = (new StackTrace().GetFrame(1).GetMethod().DeclaringType);
        MethodInfo mi = ty.GetMethod(name);

        List<object> param = new List<object>();
        param.Add(new NetworkConnection(OneServer.publicIP, OneServer.Port));
        foreach (object o in parameters)
        {
            param.Add(o);
        }

        NetworkMessage nm = new NetworkMessage(mi.DeclaringType, name, UID, param.ToArray());
        byte[] bytes = nm.ToByteArray();

        Packet packet = new Packet(new IPEndPoint(connection.GetIP(), OneServer.sendPort), bytes);
        OneServer.queue.Enqueue(packet);
    }

    public void Update()
    {
        NetworkMessage[] netmsg = OneServer.GetPersonalPackages("Server");
        if (netmsg.Length != 0)
        {
            for (int i = 0; i < netmsg.Length; i++)
            {
                NetworkMessage msg = netmsg[i];
                //msg.scriptType.GetMethod (msg.methodName).Invoke (GetComponent (msg.scriptType), msg.parameters);
                MethodInfo[] methods = OneServer.GetMethods(msg.scriptType, msg.methodName);
                bool found = false;
                for (int k = 0; k < methods.Length && !found; k++)
                {
                    try
                    {
                        methods[k].Invoke(GetComponent(msg.scriptType), msg.parameters);
                        found = true; //Stop if matching format found
                    }
                    catch (TargetParameterCountException)
                    {
                    }
                    if (!found && k == methods.Length - 1)
                    {
                        Debug.LogError("Parameters doesn't match format for any of the methods found!");
                    }
                }
            }
        }
    }

    private void Awake()
    {
        UID = new Guid().ToString();
    }
}