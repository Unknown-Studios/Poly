using UnityEngine;
using System;
using OneNetworking;

public class Test2 : MonoBehaviour
{
	void ClientConnected(NetworkConnection con) {
		Debug.Log ("This IP just connected: "+con.IP);
	}

	void Start() {
		OneServer.OnClientConnected += ClientConnected;
	}
}