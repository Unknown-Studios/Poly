using UnityEngine;
using System.Collections;
using OneNetworking;
using System.Collections.Generic;
using System.Net;
using System;
using System.Net.Sockets;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class OneServer : MonoBehaviour {
	//General settings
	private static OneServer _instance;

	public static OneServer instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType<OneServer>();
			}
			return _instance;
		}
	}

	//Send settings
	public static Queue<Packet> queue;
	public static int tickrate = 30;
	public static List<NetworkConnection> connections;
	public static IPAddress publicIP;
	public static int MaxPlayers = 20;

	public IPAddress connectIP;
	[HideInInspector]
	public int connectPort;

	//Connection settings
	public static NetworkType networkType = NetworkType.NotConnected;
	/// <summary>
	/// The port used to send/receive messages, customizable in the future.
	/// Ranges between 1024 and 65535 because ports under 1024 are reserved to system services 
	/// and 65535 is the maximum port number
	/// </summary>
	[Range(1024,65535)]
	public static int Port = 12000;
	[Range(1024,65535)]
	public static int sendPort = 12000;

	//Incoming settings
	private static List<NetworkMessage> packetQueue;

	//Test/Debug variables
	public int tick = 0;
	public int BytesReceived = 0;
	public int BytesSent = 0;
		
	/// <summary>
	/// Get packages waiting for a UID
	/// </summary>
	/// <returns>Packages meant for this UID</returns>
	/// <param name="UID">Unique ID</param>
	public static NetworkMessage[] GetPersonalPackages(string UID) {
		if (packetQueue == null) {
			packetQueue = new List<NetworkMessage> ();
		}
		List<NetworkMessage> netList = new List<NetworkMessage>();
		for (int i = 0; i < packetQueue.Count; i++) {
			if (packetQueue [i].UID == UID) {
				netList.Add (packetQueue [i]);
				packetQueue.RemoveAt (i);
			}
		}
		return netList.ToArray ();
	}

	/// <summary>
	/// You can only send NetworkMessages after this has been instantiated
	/// </summary>
	private void Init()
	{
		//Get public IP
		string externalip = new WebClient().DownloadString("http://icanhazip.com");
		Regex rgx = new Regex("[^0-9 .]");
		externalip = rgx.Replace(externalip, "");
		if (!IPAddress.TryParse (externalip, out publicIP)) {
			Debug.LogError ("Couldn't retrieve public IP");
			return;
		}
		packetQueue = new List<NetworkMessage> ();
		connections = new List<NetworkConnection> ();
		queue = new Queue<Packet>();

		//Client.Client.Bind ((new IPEndPoint (IPAddress.Any, Port)));

		Client.BeginReceive(new AsyncCallback(Receiver), null);
		InvokeRepeating("SendPackages", 0.0f, (1.0f / tickrate));
	}

	void CreateUdpClient() {
		int attempts = 0;
		//If the next 100 ports isn't available throw an exception
		while (Client == null && attempts < 100) {
			try
			{
				attempts++;
				Client = new UdpClient(Port);
			}
			catch (SocketException) 
			{
				Client = null;
				Port++;
			}
		}
		if (attempts >= 100) {
			throw new Exception ("Couldn't find a port, that is available, try defining another startport with '-StartPort #' start argument.");
		}
	}

	private void SendPackages()
	{
		tick++;
		if (queue == null)
		{
			queue = new Queue<Packet>();
		}
		using (UdpClient sender = new UdpClient())
		{
			for (int i = 0; i < queue.Count; i++)
			{
				Packet packet = queue.Dequeue();
				sender.Send(packet.bytes, packet.bytes.Length, packet.ipEnd);
				BytesSent += packet.bytes.Length;
			}
			sender.Close ();
		}
	}

	public static void Instantiate(string pathToAsset, Vector3 position, Quaternion rotation) {
		
	}

	public void InitServer(int port)
	{
		if (networkType == NetworkType.Client) {
			Debug.LogError ("You can't host a server because you are already a client");
			return;
		} else if (networkType == NetworkType.Server) {
			Debug.LogError ("You can't host a server because you are already hosting one");
			return;
		}
		Port = port;
		sendPort = port;
		Client = new UdpClient(Port);
		Init ();
		Checked = new List<NetworkConnection> ();
		connections.Add(new NetworkConnection(IPAddress.Parse("127.0.0.1"), port));
		networkType = NetworkType.Server;
	}

	public void Connect(NetworkConnection con) {
		Debug.Log ("Connected to "+connectIP+":"+connectPort);
		networkType = NetworkType.Client;
	}

	public void InitClient(IPAddress ip, int port)
	{
		if (networkType == NetworkType.Server) {
			Debug.LogError ("You cannot connect as a client because you are already a server");
			return;
		} else if (networkType == NetworkType.Client) {
			Debug.LogError ("You are already connected as a client");
			return;
		}
		Port = port;
		sendPort = port;

		CreateUdpClient ();

		Init ();
		connectIP = ip;
		connectPort = port;

		connections.Clear ();
		connections.Add (new NetworkConnection(ip,port));
		//Get my own ip, so server can connect to me
		if (ip.ToString () == "127.0.0.1") { //Server is located locally
			NetworkMessage nm = new NetworkMessage(NetworkMessageType.Connect, new NetworkConnection(IPAddress.Parse("127.0.0.1"),Port)); //Send local credentials to the server
			nm.UID = "Server";
			nm.Send (connectIP, connectPort);
		} else {
			NetworkMessage nm = new NetworkMessage(NetworkMessageType.Connect, new NetworkConnection(publicIP,Port)); //Send credentials to the server
			nm.UID = "Server";
			nm.Send (connectIP, connectPort);
		}
	}

	public void ConnectFailed(NetworkConnection connection, NetworkError error) {
		if (error == NetworkError.NotSpaceEnough) {
			Debug.LogError ("Couldn't connect to server, because there wasn't space enough");
		}
	}

	public void UpdateClientList(NetworkConnection[] allCon) {
		Debug.Log ("Updated client list");
		connections.Clear();
		foreach (NetworkConnection con in allCon) {
			connections.Add(con);
		}
		if (networkType == NetworkType.NotConnected) {
			networkType = NetworkType.Client;
		}
	}

	public void SendMethod(NetworkMessageMode mode, string name, params object[] parameters)
	{
		if (mode == NetworkMessageMode.Private) {
			Debug.LogError ("Please use SendMethod(NetworkConnection, string, params object[]) for private messages!");
			return;
		}
		Type ty = typeof(OneServer);
		MethodInfo mi = ty.GetMethod(name);

		List<object> param = new List<object> ();
		param.Add (new NetworkConnection (OneServer.publicIP, OneServer.Port));
		foreach (object o in parameters) {
			param.Add (o);
		}

		NetworkMessage nm = new NetworkMessage(mi.DeclaringType, mode, name, "Server", param.ToArray());
		byte[] bytes = nm.ToByteArray();

		if (networkType == NetworkType.Server) {
			Packet packet = new Packet (new IPEndPoint (IPAddress.Parse("127.0.0.1"), OneServer.sendPort), bytes);
			OneServer.queue.Enqueue (packet);
		} else {
			Packet packet = new Packet (new IPEndPoint (connectIP, connectPort), bytes);
			OneServer.queue.Enqueue (packet);
		}
	}

	public void SendMethod(NetworkConnection connection, string name, params object[] parameters)
	{
		Type ty = typeof(OneServer);
		MethodInfo mi = ty.GetMethod(name);

		List<object> param = new List<object> ();
		param.Add (new NetworkConnection (publicIP, OneServer.Port));
		foreach (object o in parameters) {
			param.Add (o);
		}

		NetworkMessage nm = new NetworkMessage(mi.DeclaringType, name, "Server", param.ToArray());
		byte[] bytes = nm.ToByteArray();

		Packet packet = new Packet(new IPEndPoint(connection.GetIP(), OneServer.sendPort), bytes);
		OneServer.queue.Enqueue(packet);
	}

	UdpClient Client;

	public static MethodInfo[] GetMethods(Type type, string name) {
		List<MethodInfo> methods = new List<MethodInfo> ();
		foreach (MethodInfo m in type.GetMethods()) {
			if (m.Name == name) {
				methods.Add (m);
			}
		}
		return methods.ToArray ();
	}

	void Update() {
		NetworkMessage[] netmsg = OneServer.GetPersonalPackages ("Server");
		if (netmsg.Length != 0) {
			for (int i = 0; i < netmsg.Length; i++) {
				NetworkMessage msg = netmsg [i];
				//msg.scriptType.GetMethod (msg.methodName).Invoke (GetComponent (msg.scriptType), msg.parameters);
				MethodInfo[] methods = OneServer.GetMethods (msg.scriptType, msg.methodName);
				bool found = false;
				for (int k = 0; k < methods.Length && !found; k++) {
					try {
						methods[k].Invoke(GetComponent(msg.scriptType), msg.parameters);
						found = true; //Stop if matching format found
					}
					catch (TargetParameterCountException) {
					}
					if (!found && k == methods.Length-1) {
						Debug.LogError ("Parameters doesn't match format for any of the methods found!");
					}
				}
			}
		}
	}

	void Disconnect(NetworkConnection con) {
		connections.Remove (con);
		Debug.Log ("User disconnected: " + con.IP.ToString ());
	}

	List<NetworkConnection> Checked;
		
	void Receiver (IAsyncResult res)
	{
		IPEndPoint sender = new IPEndPoint (IPAddress.Any, 0);
		byte[] received = Client.EndReceive(res, ref sender);
		BytesReceived += received.Length;
		NetworkMessage d = NetworkMessage.ToNM (received); //Convert to NetworkMessage

		if (d.msgType == NetworkMessageType.Connect) {
			if (networkType == NetworkType.Server) {
				NetworkConnection con = new NetworkConnection(sender.Address,sender.Port);
				Debug.Log ("Connecting: "+con.IP+":"+con.Port);
				if (connections.Count + 1 < MaxPlayers) {
					Debug.Log (con.Port);
					connections.Add (con);
					SendMethod (con, "Connect");
				} else {
					SendMethod (con, "ConnectFailed", NetworkError.NotSpaceEnough);
				}
			}
		} else if (d.msgType == NetworkMessageType.Disconnect) { 
			Disconnect ((NetworkConnection)d.parameters [0]);
		} else if (d.msgType == NetworkMessageType.Message) {
			if (networkType == NetworkType.Server) {
				if (d.msgMode == NetworkMessageMode.All) {
					packetQueue.Add (d); //Wait for another script to pick it up
					d.msgMode = NetworkMessageMode.Private;
					for (int i = 1; i < connections.Count; i++) {
						Debug.Log ((string)connections [i]);
						Packet packet = new Packet (new IPEndPoint (connections[i].GetIP(), connections[i].Port), d.ToByteArray ());
						queue.Enqueue (packet);
					}
				} else if (d.msgMode == NetworkMessageMode.Clients) {
					Debug.Log ("Meant for clients: "+connections.Count);
					//If server and meant for players
					d.msgMode = NetworkMessageMode.Private;
					for (int i = 1; i < connections.Count; i++) {
						Packet packet = new Packet (new IPEndPoint (connections[i].GetIP(), connections[i].Port), d.ToByteArray ());
						queue.Enqueue (packet);
					}
				} else if (d.msgMode == NetworkMessageMode.Server) {
					packetQueue.Add (d); //Wait for another script to pick it up
				}
			} else if (d.msgMode == NetworkMessageMode.Private) {
				packetQueue.Add (d); //Wait for another script to pick it up
			}
		}
		Client.BeginReceive(new AsyncCallback(Receiver), null);
	}
}
