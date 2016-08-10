using NetworkLibrary;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetworkSystem : MonoBehaviour
{
    public static NetworkType networkType;
    public static Queue<Packet> queue;
    public static int tickrate = 30;
    public static int port = 12055;
    public int tick = 0;
    private static NetworkSystem _instance;

    public static NetworkSystem instance
    {
        get
        {
            if (_instance == null)
            {
                NetworkSystem[] networkSystems = FindObjectsOfType<NetworkSystem>();
                if (networkSystems.Length > 1)
                {
                    _instance = networkSystems[0];
                    Debug.LogError("Multiple Instances of NetworkSystem found.");
                    for (int i = 1; i < networkSystems.Length; i++)
                    {
                        networkSystems[i].enabled = false;
                    }
                }
                else if (networkSystems.Length == 1)
                {
                    _instance = networkSystems[0];
                }
            }
            return _instance;
        }
    }

    public static void InitServer()
    {
        networkType = NetworkType.Server;
        instance.Init();
    }

    public static void InitClient(IPAddress ip)
    {
        networkType = NetworkType.Client;
        ConnectTo(ip);
        instance.Init();
    }

    public static void ConnectTo(IPAddress ip)
    {
    }

    public NetworkObject GetNetobjByID(string UID)
    {
        NetworkObject[] netobj = FindObjectsOfType<NetworkObject>();
        for (int i = 0; i < netobj.Length; i++)
        {
            if (netobj[i].UID == UID)
            {
                return netobj[i];
            }
        }
        return null;
    }

    private void Init()
    {
        queue = new Queue<Packet>();

        //Send Objects

        UdpClient receiver = new UdpClient(port);
        receiver.BeginReceive(DataReceived, receiver);
        InvokeRepeating("SendPackages", 0.0f, (1.0f / tickrate));
    }

    private void DataReceived(IAsyncResult ar)
    {
        UdpClient c = (UdpClient)ar.AsyncState;
        IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] receivedBytes = c.EndReceive(ar, ref receivedIpEndPoint);
        NetworkMessage nm = NetworkMessage.ToNM(receivedBytes);
        switch (nm.msgType)
        {
            case NetworkMessageType.Connect:
                if (networkType == NetworkType.Server)
                {
                    NetworkObject[] netobjs = FindObjectsOfType<NetworkObject>();
                    GameObject[] objs = new GameObject[netobjs.Length];
                    for (int i = 0; i < netobjs.Length; i++)
                    {
                        objs[i] = netobjs[i].gameObject;
                    }
                    NetworkMessage.SendMethod(receivedIpEndPoint.Address, "SendObjects", objs);
                }
                break;

            case NetworkMessageType.Message:
                RunMethod(nm);
                break;
        }
        c.BeginReceive(DataReceived, c);
    }

    private void RunMethod(NetworkMessage nm)
    {
        nm.scriptType.GetMethod(nm.methodName).Invoke(GetNetobjByID(nm.UID).GetComponent(nm.scriptType), nm.parameters);
    }

    private void SendPackages()
    {
        tick++;
        if (queue == null)
        {
            queue = new Queue<Packet>();
        }
        using (UdpClient sender = new UdpClient(port))
        {
            for (int i = 0; i < queue.Count; i++)
            {
                Packet packet = queue.Dequeue();
                sender.Send(packet.bytes, packet.bytes.Length, packet.ipEnd);
            }
        }
    }
}