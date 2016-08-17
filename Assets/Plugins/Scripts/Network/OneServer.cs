using UnityEngine;
using System.Collections;
using OneNetworking;
using System.Collections.Generic;
using System.Net;
using System;
using System.Net.Sockets;
using System.Diagnostics;
using System.Reflection;
using Debug = UnityEngine.Debug;
using System.Text.RegularExpressions;

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

	//Connection settings
	public static NetworkType networkType = NetworkType.NotConnected;
	/// <summary>
	/// The port used to send/receive messages, customizable in the future.
	/// Ranges between 1024 and 65535 because ports under 1024 are reserved to system services 
	/// and 65535 is the maximum port number
	/// </summary>
	[Range(1024,65535)]
	public static int Port = 12000;

	//Incoming settings
	private static List<NetworkMessage> packetQueue;

	//Test/Debug variables
	public int tick = 0;
	public int BytesReceived = 0;
	public int BytesSent = 0;

	//Get packages waiting for a specific ID
	public static NetworkMessage[] GetPersonalPackages(string UID) {
		List<NetworkMessage> netList = new List<NetworkMessage>();
		for (int i = 0; i < packetQueue.Count; i++) {
			if (packetQueue [i].UID == UID) {
				netList.Add (packetQueue [i]);
				packetQueue.RemoveAt (i);
			}
		}
		return netList.ToArray ();
	}

	void Start () {
		InitServer ();
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
		Client = new UdpClient (Port);
		Client.BeginReceive(new AsyncCallback(Receiver), null);
		queue = new Queue<Packet>();
		InvokeRepeating("SendPackages", 0.0f, (1.0f / tickrate));
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
		}
	}

	public static void Instantiate(string pathToAsset, Vector3 position, Quaternion rotation) {
		
	}

	public void Spawn(NetworkConnection con, int test, string te) {
		Debug.Log (con.IP + ": " + test);
	}

	void Send() {
		SendMethod (NetworkMessageMode.Server, "Spawn", UnityEngine.Random.Range(1,5), "Test: ");
	}


	public void ConnectTo(IPAddress ip)
	{
		InitClient (ip);
	}

	public void InitServer()
	{
		if (networkType == NetworkType.Client) {
			Debug.LogError ("You can't host a server because you are already a client");
			return;
		} else if (networkType == NetworkType.Server) {
			Debug.LogError ("You can't host a server because you are already hosting one");
			return;
		}
		Init ();
		Checked = new List<NetworkConnection> ();
		connections.Add(NetworkConnection.Parse(publicIP.ToString()));
		InvokeRepeating ("PingAllNow", 0.0f, 10.0f);
		networkType = NetworkType.Server;
	}

	private void InitClient(IPAddress ip)
	{
		Init ();

		if (networkType == NetworkType.Server) {
			Debug.LogError ("You cannot connect as a client because you are already a server");
			return;
		} else if (networkType == NetworkType.Client) {
			Debug.LogError ("You are already connected as a client");
			return;
		}
		//Get my own ip, so server can connect to me
		NetworkMessage nm = new NetworkMessage(NetworkMessageType.Connect, publicIP.Address.ToString());
		nm.msgType = NetworkMessageType.Connect;
		nm.UID = "Server";

		connections.Add(NetworkConnection.Parse(publicIP.ToString()));
		networkType = NetworkType.Client;
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
		param.Add (new NetworkConnection (OneServer.publicIP));
		foreach (object o in parameters) {
			param.Add (o);
		}

		NetworkMessage nm = new NetworkMessage(mi.DeclaringType, mode, name, "Server", param.ToArray());
		byte[] bytes = nm.ToByteArray();

		if (networkType == NetworkType.Server) {
			Packet packet = new Packet (new IPEndPoint (IPAddress.Parse("127.0.0.1"), OneServer.Port), bytes);
			OneServer.queue.Enqueue (packet);
		} else {
			Packet packet = new Packet (new IPEndPoint (connections [0].IP, OneServer.Port), bytes);
			OneServer.queue.Enqueue (packet);
		}
	}

	/// <summary>
	/// Used to send a method to another client
	/// </summary>
	/// <param name="connection">Connection.</param>
	/// <param name="name">Name.</param>
	/// <param name="parameters">Parameters.</param>
	public void SendMethod(NetworkConnection connection, string name, params object[] parameters)
	{
		Type ty = typeof(OneServer);
		MethodInfo mi = ty.GetMethod(name);

		List<object> param = new List<object> ();
		param.Add (new NetworkConnection (publicIP));
		foreach (object o in parameters) {
			param.Add (o);
		}

		NetworkMessage nm = new NetworkMessage(mi.DeclaringType, name, "Server", param.ToArray());
		byte[] bytes = nm.ToByteArray();

		Packet packet = new Packet(new IPEndPoint(connection.IP, OneServer.Port), bytes);
		OneServer.queue.Enqueue(packet);
	}

	void OnGUI() {
		if (GUILayout.Button ("Test")) {
			Send ();
		}
	}

	UdpClient Client;

	private void RunMethod(NetworkMessage nm)
	{
		nm.scriptType.GetMethod(nm.methodName).Invoke(this, nm.parameters);
	}

	void PingAllNow() {
		StartCoroutine (PingAll ());
	}

	IEnumerator PingAll() {
		Checked.Clear ();
		for (int i = 0; i < connections.Count; i++) {
			SendMethod(connections[i], "Ping");
		}
		yield return new WaitForSeconds(5.0f);
		for (int i = 0; i < connections.Count; i++) {
			if (!Checked.Contains (connections [i])) {
				Disconnect (connections[i]);
			}
		}
	}

	void Disconnect(NetworkConnection con) {
		//connections.Remove (con);
		//Debug.Log ("User disconnected: " + con.IP.ToString ());
	}

	List<NetworkConnection> Checked;

	public void SaveFromDisconnection(NetworkConnection con) {
		Debug.Log ("Pong");
		Checked.Add (con);
	}

	public void Ping(NetworkConnection con) {
		Debug.Log ("Ping");
		//Send a message back to not disconnect the object this time.
		SendMethod (con, "SaveFromDisconnection");
	}
		
	void Receiver (IAsyncResult res)
	{
		IPEndPoint RemoteIpEndPoint = new IPEndPoint (IPAddress.Any, Port);
		byte[] received = Client.EndReceive (res, ref RemoteIpEndPoint);
		BytesReceived += received.Length;

		NetworkMessage d = NetworkMessage.ToNM (received);
		if (d.msgType == NetworkMessageType.Message) {
			if (d.msgMode == NetworkMessageMode.Private) {
				if (d.UID == "Server") { //Meant for the OneServer object, so just run it
					RunMethod (d);
				} else {
					packetQueue.Add (d); //Wait for another script to pick it up
				}
			} else if (networkType == NetworkType.Server) {
				if (d.msgMode == NetworkMessageMode.All) {
					//If server and meant for all
					if (d.UID == "Server") { //Meant for the OneServer object, so just run it
						RunMethod (d);
					} else {
						packetQueue.Add (d); //Wait for another script to pick it up
					}
					d.msgMode = NetworkMessageMode.Private;
					for (int i = 1; i < connections.Count; i++) {
						Packet packet = new Packet (new IPEndPoint (connections[i].IP, Port), d.ToByteArray ());
						queue.Enqueue (packet);
					}
				} else if (d.msgMode == NetworkMessageMode.Clients) {
					//If server and meant for players
					d.msgMode = NetworkMessageMode.Private;
					for (int i = 1; i < connections.Count; i++) {
						Packet packet = new Packet (new IPEndPoint (connections[i].IP, Port), d.ToByteArray ());
						queue.Enqueue (packet);
					}
				} else if (d.msgMode == NetworkMessageMode.Server) {
					if (d.UID == "Server") { //Meant for the OneServer object, so just run it
						RunMethod (d);
					} else {
						packetQueue.Add (d); //Wait for another script to pick it up
					}
				}
			}
		}

		Client.BeginReceive(new AsyncCallback(Receiver), null);
	}
}
