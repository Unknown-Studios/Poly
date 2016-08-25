using OneNetworking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using System.Security.Cryptography;
using System.Text;

public class OneServer : MonoBehaviour
{
    //Send settings
    public static Queue<Packet> queue;

    public static int tickrate = 30;

    public static List<NetworkConnection> connections;

    public static IPAddress publicIP;

    public static int MaxPlayers = 20;

    //Connection settings
    public static NetworkType networkType = NetworkType.NotConnected;

    /// <summary>
    /// The port used to send/receive messages, customizable in the future.
    /// Ranges between 1024 and 65535 because ports under 1024 are reserved to system services
    /// and 65535 is the maximum port number
    /// </summary>
    [Range(1024, 65535)]
    public static int Port = 12000;

    [Range(1024, 65535)]
    public static int sendPort = 12000;

    public IPAddress connectIP;

    [HideInInspector]
    public int connectPort;

    //Test/Debug variables
    public int tick = 0;

    public int BytesReceived = 0;

    public int BytesSent = 0;

    //General settings
    private static OneServer _instance;

    //Incoming settings
    private static List<NetworkMessage> packetQueue;

    private UdpClient Client;

	public static EncryptKeyPair CreateKeyPair()
	{
		CspParameters cspParams = new CspParameters { ProviderType = 1 };

		RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(2048, cspParams);

		string publicKey = rsaProvider.ToXmlString(false);
		string privateKey = rsaProvider.ToXmlString(true);

		return new EncryptKeyPair(publicKey, privateKey);
	}

	private string RSAEncrypt(string publickey, string value)
	{
		byte[] plaintext = Encoding.Unicode.GetBytes(value);

		CspParameters cspParams = new CspParameters();
		using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(2048,cspParams))
		{
			RSA.FromXmlString (publickey);
			byte[] encryptedData = RSA.Encrypt(plaintext, false);
			return Convert.ToBase64String(encryptedData);
		}
	}

	private string RSADecrypt(string privatekey, string value)
	{
		byte[] encryptedData = Convert.FromBase64String(value);

		CspParameters cspParams = new CspParameters();
		cspParams.KeyContainerName = privatekey;
		using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(2048,cspParams))
		{ 
			RSA.FromXmlString (privatekey);
			byte[] decryptedData = RSA.Decrypt(encryptedData,false);
			return Encoding.Unicode.GetString(decryptedData);
		}
	}

	private EncryptKeyPair encryptionKeys;

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

    /// <summary>
    /// Get packages waiting for a UID
    /// </summary>
    /// <returns>Packages meant for this UID</returns>
    /// <param name="UID">Unique ID</param>
    public static NetworkMessage[] GetPersonalPackages(string UID)
    {
        if (packetQueue == null)
        {
            packetQueue = new List<NetworkMessage>();
        }
        List<NetworkMessage> netList = new List<NetworkMessage>();
        for (int i = 0; i < packetQueue.Count; i++)
        {
            if (packetQueue[i].UID == UID)
            {
                netList.Add(packetQueue[i]);
                packetQueue.RemoveAt(i);
            }
        }
        return netList.ToArray();
    }

    public static void Instantiate(string pathToAsset, Vector3 position, Quaternion rotation)
    {
    }

    public static MethodInfo[] GetMethods(Type type, string name)
    {
        List<MethodInfo> methods = new List<MethodInfo>();
        foreach (MethodInfo m in type.GetMethods())
        {
            if (m.Name == name)
            {
                methods.Add(m);
            }
        }
        return methods.ToArray();
    }

    public void InitServer(int port)
    {
        if (networkType == NetworkType.Client)
        {
            Debug.LogError("You can't host a server because you are already a client");
            return;
        }
        else if (networkType == NetworkType.Server)
        {
            Debug.LogError("You can't host a server because you are already hosting one");
            return;
        }
        Port = port;
        sendPort = port;
        Client = new UdpClient(Port);
        Init();
		myConnection = new NetworkConnection (IPAddress.Parse ("127.0.0.1"), port, encryptionKeys.publicKey);
		connections.Add(myConnection);
		Keys.Add (myConnection, GenerateKey ());
		networkType = NetworkType.Server;
    }

	public void Connect(NetworkConnection con, string RSAKey, string serverAESKey, string clientAESKey, string ClientRSA)
	{
		networkType = NetworkType.Client;
		string decryptKey = "";
		if (ClientRSA == encryptionKeys.publicKey) {
			decryptKey = encryptionKeys.privateKey;
		}

		string serverAES = RSADecrypt (decryptKey, serverAESKey);
		string clientAES = RSADecrypt (decryptKey, clientAESKey);

		swatch.Stop ();
		Debug.Log (swatch.Elapsed.Seconds);

		connections [0].encryptionKey = RSAKey;
		Keys.Add (con, serverAES);
		Keys.Add (myConnection, clientAES);
    }

    public void InitClient(IPAddress ip, int port)
    {
        if (networkType == NetworkType.Server)
        {
            Debug.LogError("You cannot connect as a client because you are already a server");
            return;
        }
        else if (networkType == NetworkType.Client)
        {
            Debug.LogError("You are already connected as a client");
            return;
        }
        Port = port;
        sendPort = port;

		CreateUdpClient();

        Init();
        connectIP = ip;
        connectPort = port;

        connections.Clear();
        connections.Add(new NetworkConnection(ip, port));
        //Get my own ip, so server can connect to me
        if (ip.ToString() == "127.0.0.1")
        { //Server is located locally
			myConnection = new NetworkConnection(IPAddress.Parse("127.0.0.1"), Port, encryptionKeys.publicKey);
			NetworkMessage nm = new NetworkMessage(NetworkMessageType.Connect, myConnection); //Send local credentials to the server
            nm.UID = "Server";
			Packet packet = new Packet (new NetworkConnection (connectIP, connectPort), nm.ToByteArray (), false);
			queue.Enqueue (packet);
        }
        else
		{
			myConnection = new NetworkConnection (publicIP, Port, encryptionKeys.publicKey);
			NetworkMessage nm = new NetworkMessage(NetworkMessageType.Connect, myConnection); //Send credentials to the server
			nm.UID = "Server";
			Packet packet = new Packet (new NetworkConnection (connectIP, connectPort), nm.ToByteArray (), false);
			queue.Enqueue (packet);
        }
		swatch = new Stopwatch ();
		swatch.Start ();
    }

	Stopwatch swatch;

	NetworkConnection myConnection;

    public void ConnectFailed(NetworkConnection connection, NetworkError error)
    {
        if (error == NetworkError.NotSpaceEnough)
        {
            Debug.LogError("Couldn't connect to server, because there wasn't space enough");
        }
    }

	private void SendMethod(NetworkMessageMode mode, string name, params object[] parameters)
    {
        if (mode == NetworkMessageMode.Private)
        {
            Debug.LogError("Please use SendMethod(NetworkConnection, string, params object[]) for private messages!");
            return;
        }
        Type ty = typeof(OneServer);
        MethodInfo mi = ty.GetMethod(name);

        List<object> param = new List<object>();
        param.Add(new NetworkConnection(OneServer.publicIP, OneServer.Port));
        foreach (object o in parameters)
        {
            param.Add(o);
        }

        NetworkMessage nm = new NetworkMessage(mi.DeclaringType, mode, name, "Server", param.ToArray());
        byte[] bytes = nm.ToByteArray();

        if (networkType == NetworkType.Server)
        {
			Packet packet = new Packet(new NetworkConnection(IPAddress.Parse("127.0.0.1"), OneServer.sendPort), bytes);
            OneServer.queue.Enqueue(packet);
        }
        else
        {
			Packet packet = new Packet(new NetworkConnection(connectIP, connectPort), bytes);
            OneServer.queue.Enqueue(packet);
        }
    }

	private void SendMethod(NetworkConnection connection, string name, params object[] parameters)
    {
        Type ty = typeof(OneServer);
        MethodInfo mi = ty.GetMethod(name);

        List<object> param = new List<object>();
        param.Add(new NetworkConnection(publicIP, OneServer.Port));
        foreach (object o in parameters)
        {
            param.Add(o);
        }

        NetworkMessage nm = new NetworkMessage(mi.DeclaringType, name, "Server", param.ToArray());
        byte[] bytes = nm.ToByteArray();

		Packet packet = new Packet(connection, bytes);
        OneServer.queue.Enqueue(packet);
    }

	private void SendMethod(NetworkConnection connection, bool Encrypt, string name, params object[] parameters)
	{
		Type ty = typeof(OneServer);
		MethodInfo mi = ty.GetMethod(name);

		List<object> param = new List<object>();
		param.Add(new NetworkConnection(publicIP, OneServer.Port));
		foreach (object o in parameters)
		{
			param.Add(o);
		}

		NetworkMessage nm = new NetworkMessage(mi.DeclaringType, name, "Server", param.ToArray());
		byte[] bytes = nm.ToByteArray();

		Packet packet = new Packet(connection, bytes, Encrypt);
		OneServer.queue.Enqueue(packet);
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
        if (!IPAddress.TryParse(externalip, out publicIP))
        {
            Debug.LogError("Couldn't retrieve public IP");
            return;
        }
		encryptionKeys = CreateKeyPair ();

		Keys = new Dictionary<NetworkConnection, string> ();
        packetQueue = new List<NetworkMessage>();
        connections = new List<NetworkConnection>();
        queue = new Queue<Packet>();

        Client.BeginReceive(new AsyncCallback(Receiver), null);
        InvokeRepeating("SendPackages", 0.0f, (1.0f / tickrate));
    }

	string GetKey(NetworkConnection con) {
		string data = null;
		if (!Keys.TryGetValue (con, out data)) {
			throw new Exception ("Key for that NetworkConnection couldn't be found");
		}
		return data;
	}

    private void CreateUdpClient()
    {
        int attempts = 0;
        //If the next 100 ports isn't available throw an exception
        while (Client == null && attempts < 100)
        {
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
        if (attempts >= 100)
        {
            throw new Exception("Couldn't find a port, that is available, try defining another startport with '-StartPort #' start argument.");
        }
    }

	public string GenerateKey() {
		return Guid.NewGuid().ToString();
	}

	Dictionary<NetworkConnection, string> Keys;

    private void SendPackages()
    {
        tick++;
        if (queue == null)
        {
            queue = new Queue<Packet>();
        }
        for (int i = 0; i < queue.Count; i++)
        {
            Packet packet = queue.Dequeue();
			if (packet.Encrypt) {
				EncryptedMessage encryptedMsg = Encryption.EncryptBytes(GetKey(packet.connection), packet.bytes);
				byte[] encryptedBytes = encryptedMsg.ToByteArray ();

				Client.Send (encryptedBytes, encryptedBytes.Length, (IPEndPoint)packet.connection);
				BytesSent += encryptedBytes.Length;
			} else {
				Client.Send (packet.bytes, packet.bytes.Length, (IPEndPoint)packet.connection);
				BytesSent += packet.bytes.Length;
			}
        }
    }

    private void Update()
    {
        NetworkMessage[] netmsg = OneServer.GetPersonalPackages("Server");
        if (netmsg.Length != 0)
        {
            for (int i = 0; i < netmsg.Length; i++)
            {
                NetworkMessage msg = netmsg[i];
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

    private void Disconnect(NetworkConnection con)
    {
        connections.Remove(con);
        Debug.Log("User disconnected: " + con.IP.ToString());
    }

    private void Receiver(IAsyncResult res)
    {
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
		byte[] received = Client.EndReceive(res, ref sender);
		NetworkMessage d = null;
		try {
			d = NetworkMessage.ToNM(received); //Convert to NetworkMessage
		}
		catch (Exception) {
			EncryptedMessage e = EncryptedMessage.ToEM(received);
			string key = GetKey ((NetworkConnection)d.parameters [0]);
			received = Encryption.DecryptBytes(key, e);
			d = NetworkMessage.ToNM(received); //Convert to NetworkMessage
		}

        BytesReceived += received.Length;

        if (d.msgType == NetworkMessageType.Connect)
        {
            if (networkType == NetworkType.Server)
            {
                if (connections.Count + 1 < MaxPlayers)
                {
					NetworkConnection con = (NetworkConnection)d.parameters [0]; 
					connections.Add(con);
					//Get both keys as strings
					string serverKey = GetKey (myConnection);
					string clientKey = GenerateKey ();
					//Add clients key to list
					Keys.Add (con, clientKey);

					//Encrypt both keys
					string encryptServerKey = RSAEncrypt (con.encryptionKey, serverKey);
					string encryptClientKey = RSAEncrypt (con.encryptionKey, clientKey);
					SendMethod(con, false, "Connect", encryptionKeys.publicKey, encryptServerKey, encryptClientKey, con.encryptionKey);
                }
                else
				{
					NetworkConnection con = (NetworkConnection)d.parameters [0];
					SendMethod(con, "ConnectFailed", NetworkError.NotSpaceEnough);
                }
            }
        }
        else if (d.msgType == NetworkMessageType.Disconnect)
        {
            Disconnect((NetworkConnection)d.parameters[0]);
        }
        else if (d.msgType == NetworkMessageType.Message)
        {
            if (networkType == NetworkType.Server)
            {
                if (d.msgMode == NetworkMessageMode.All)
                {
                    packetQueue.Add(d); //Wait for another script to pick it up
                    d.msgMode = NetworkMessageMode.Private;
                    for (int i = 1; i < connections.Count; i++)
                    {
                        Debug.Log((string)connections[i]);
						Packet packet = new Packet(new NetworkConnection(connections[i].IP, connections[i].Port), d.ToByteArray());
                        queue.Enqueue(packet);
                    }
                }
                else if (d.msgMode == NetworkMessageMode.Clients)
                {
                    //If server and meant for players
                    d.msgMode = NetworkMessageMode.Private;
                    for (int i = 1; i < connections.Count; i++)
                    {
						Packet packet = new Packet(new NetworkConnection(connections[i].IP, connections[i].Port), d.ToByteArray());
                        queue.Enqueue(packet);
                    }
                }
                else if (d.msgMode == NetworkMessageMode.Server)
                {
                    packetQueue.Add(d); //Wait for another script to pick it up
                }
            }
            else
            {
                packetQueue.Add(d); //Wait for another script to pick it up
            }
        }
        Client.BeginReceive(new AsyncCallback(Receiver), null);
    }
}