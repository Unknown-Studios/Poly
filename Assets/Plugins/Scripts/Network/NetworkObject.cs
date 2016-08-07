using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using NetworkLibrary;


public class NetworkObject : MonoBehaviour {
	public string UID;

		void Start() {
			if (NetworkSystem.networkType == NetworkSystem.NetworkType.Server) {
			UID = Guid.NewGuid().ToString();
			}
			UdpClient receiver = new UdpClient(NetworkSystem.port);
			receiver.BeginReceive(DataReceived, receiver);
		}

		private void DataReceived(IAsyncResult ar)
		{
			UdpClient c = (UdpClient)ar.AsyncState;
			IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
			Byte[] receivedBytes = c.EndReceive(ar, ref receivedIpEndPoint);
			NetworkMessage nm = NetworkMessage.ToNM (receivedBytes);
			RunMethod(nm);
			c.BeginReceive(DataReceived, c);
		}


	private void RunMethod(NetworkMessage nm) {
		nm.scriptType.GetMethod(nm.methodName).Invoke(GetComponent(nm.scriptType), nm.parameters);
	}
}