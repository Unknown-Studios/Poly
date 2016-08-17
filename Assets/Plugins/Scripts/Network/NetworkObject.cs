using OneNetworking;
using System;
using UnityEngine;
using System.Reflection;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;

public class NetworkObject : MonoBehaviour
{
	[HideInInspector]
    public string UID;
	[HideInInspector]
	public bool isMine = false;

	void Awake() {
		UID = new Guid ().ToString();
	}

	void Start() {

	}

	public void SendMethod(NetworkMessageMode mode, string name, params object[] parameters)
	{
		if (mode == NetworkMessageMode.Private) {
			Debug.LogError ("Please use SendMethod(NetworkConnection, string, params object[]) for private messages!");
			return;
		}
		Type ty = (new StackTrace().GetFrame(1).GetMethod().DeclaringType);
		MethodInfo mi = ty.GetMethod(name);

		List<object> param = new List<object> ();
		param.Add (new NetworkConnection (OneServer.publicIP));
		foreach (object o in parameters) {
			param.Add (o);
		}

		NetworkMessage nm = new NetworkMessage(mi.DeclaringType, mode, name, UID, param.ToArray());
		byte[] bytes = nm.ToByteArray();

		Packet packet = new Packet (new IPEndPoint (OneServer.connections [0].IP, OneServer.Port), bytes);
		OneServer.queue.Enqueue (packet);
	}

	/// <summary>
	/// Used to send a method to another client
	/// </summary>
	/// <param name="connection">Connection.</param>
	/// <param name="name">Name.</param>
	/// <param name="parameters">Parameters.</param>
	public void SendMethod(NetworkConnection connection, string name, params object[] parameters)
	{
		Type ty = (new StackTrace().GetFrame(1).GetMethod().DeclaringType);
		MethodInfo mi = ty.GetMethod(name);

		List<object> param = new List<object> ();
		param.Add (new NetworkConnection (OneServer.publicIP));
		foreach (object o in parameters) {
			param.Add (o);
		}

		NetworkMessage nm = new NetworkMessage(mi.DeclaringType, name, UID, param.ToArray());
		byte[] bytes = nm.ToByteArray();

		Packet packet = new Packet(new IPEndPoint(connection.IP, OneServer.Port), bytes);
		OneServer.queue.Enqueue(packet);
	}

	public void Update() {
		NetworkMessage[] netmsg = OneServer.GetPersonalPackages (UID);
		if (netmsg.Length != 0) {
			Debug.Log (netmsg.Length + " messages received");
			for (int i = 0; i < netmsg.Length; i++) {
				NetworkMessage msg = netmsg [i];
				GetComponent (msg.scriptType).BroadcastMessage (msg.methodName, msg.parameters);
			}
		}
	}
}