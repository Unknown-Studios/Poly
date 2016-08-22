using UnityEngine;
using System.Collections;
using System.Net;
using OneNetworking;
using System.Text.RegularExpressions;

public class NetworkTest : MonoBehaviour {
	string port = "";
	string oldport = "";
	string ipaddress = "";
	OneServer server;
	NetworkObject netobj;
	public string ip;

	void Start() {
		server = GetComponent<OneServer> ();
		netobj = GetComponent<NetworkObject> ();
		ip = "127.0.0.1";
	}

	void OnGUI() {
		GUILayout.Label("NetworkType: "+ OneServer.networkType.ToString ());
		GUILayout.Label ("IPv4 address: " + ip);
		if (OneServer.networkType == OneNetworking.NetworkType.NotConnected) {
			oldport = port;

			ipaddress = GUI.TextField (new Rect (100, 50, 100, 25), ipaddress);
			port = GUI.TextField (new Rect (100, 75, 100, 25), port);
			int po = 1024;
			if (port != "") {
				if (!int.TryParse (port, out po)) {
					port = oldport;
				}
			}

			if (GUI.Button (new Rect (100, 100, 100, 25), "Host")) {
				port = po.ToString ();

				if (po < 1 || po > 65535) {
					Debug.LogError ("Pleasse choose a port in the range 1-65535");
					return;
				}
				server.InitServer (po);
			}
			if (GUI.Button (new Rect (100, 125, 100, 25), "Connect")) {
				port = po.ToString ();

				if (po < 1 || po > 65535) {
					Debug.LogError ("Pleasse choose a port in the range 1-65535");
					return;
				}

				server.InitClient(IPAddress.Parse("127.0.0.1"), po);
			}
		} else {
			if (OneServer.networkType == NetworkType.Server) {
				GUILayout.Label ("Connection list");
				foreach (NetworkConnection con in OneServer.connections) {
					GUILayout.Label (con.IP + ":" + con.Port);
				}
			}

			GUI.Label(new Rect(Screen.width-200,0,200,25), "ReceivePort: " +OneServer.Port);
			GUI.Label(new Rect(Screen.width-200,25,200,25), "SendPort: " +OneServer.sendPort);
			if (OneServer.connections != null) {
				GUI.Label (new Rect (Screen.width - 200, 50, 200, 25), (string)OneServer.connections [0]);
			}

			if (GUI.Button (new Rect(100,100,200,25),"Send test message")) {
				Send ();
			}
		}
	}

	public void Spawn(NetworkConnection con, int test, string te) {
		Debug.Log (con.IP + ": " + test);
	}

	void Send() {
		netobj.SendMethod (NetworkMessageMode.All, "Spawn", UnityEngine.Random.Range(1,5), "Test: ");
	}
}
