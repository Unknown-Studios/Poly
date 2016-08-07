using System.Net;
using System.Net.Sockets;
using System;
using NetworkLibrary;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class NetworkSystem : MonoBehaviour {
	public static NetworkType networkType;
	public static Queue<Packet> queue;
	public static int tickrate;
	public static int port = 12055;
	private static NetworkSystem _instance;

	public enum NetworkType { Server = 0, Client = 1 };

	public static NetworkSystem instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType<NetworkSystem>();
				if (FindObjectsOfType<NetworkSystem> ().Length > 1) {
					Debug.LogError ("Multiple Instances of NetworkSystem found.");
				}
			}
			return _instance;
		}
	}

	public static void InitServer() {
		networkType = NetworkType.Server;
		Init();
	}

	public static void InitClient() {
		networkType = NetworkType.Client;
		Init();
	}

	private static void Init() {
		queue = new Queue<Packet>();
		NetworkSystem.instance.InvokeRepeating("SendPackages", 0.0f, 1.0f / tickrate);
	}

	private static void SendPackages() {
		using (UdpClient sender = new UdpClient(port)) {
			for (int i = 0; i < queue.Count; i++) {
				Packet packet = queue.Dequeue();
				sender.Send(packet.bytes, packet.bytes.Length, packet.ipEnd);
			}
		}
	}
}