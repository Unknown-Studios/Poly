using OneNetworking;
using System.Collections;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine;

public class NetworkTest : MonoBehaviour
{
    public string ip;
    private string port = "";
    private string oldport = "";
    private string ipaddress = "";
    private OneServer server;
    private NetworkObject netobj;

    private Texture2D texture;

    public void Spawn(NetworkConnection con, int test, string te)
    {
        Debug.Log(con.IP + ": " + test);
    }

    public void UpdateTexture(NetworkConnection con, float r, float g, float b)
    {
        texture = new Texture2D(10, 10);
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, new Color(r, g, b));
            }
        }
        texture.Apply();
    }

    private void Start()
    {
        server = GetComponent<OneServer>();
        netobj = GetComponent<NetworkObject>();
        ip = "127.0.0.1";
    }

    private void OnGUI()
    {
        GUILayout.Label("NetworkType: " + OneServer.networkType.ToString());
        GUILayout.Label("IPv4 address: " + ip);
        if (OneServer.networkType == OneNetworking.NetworkType.NotConnected)
        {
            oldport = port;

            ipaddress = GUI.TextField(new Rect(100, 50, 100, 25), ipaddress);
            port = GUI.TextField(new Rect(100, 75, 100, 25), port);
            int po = 1024;
            if (port != "")
            {
                if (!int.TryParse(port, out po))
                {
                    port = oldport;
                }
            }

            if (GUI.Button(new Rect(100, 100, 100, 25), "Host"))
            {
                port = po.ToString();

                if (po < 1 || po > 65535)
                {
                    Debug.LogError("Pleasse choose a port in the range 1-65535");
                    return;
                }
                server.InitServer(po);
            }
            if (GUI.Button(new Rect(100, 125, 100, 25), "Connect"))
            {
                port = po.ToString();

                if (po < 1 || po > 65535)
                {
                    Debug.LogError("Pleasse choose a port in the range 1-65535");
                    return;
                }

                server.InitClient(IPAddress.Parse("127.0.0.1"), po);
            }
        }
        else
        {
            if (OneServer.networkType == NetworkType.Server)
            {
                GUILayout.Label("Connection list");
                foreach (NetworkConnection con in OneServer.connections)
                {
                    GUILayout.Label(con.IP + ":" + con.Port);
                }
            }

            GUI.Label(new Rect(Screen.width - 200, 0, 200, 25), "ReceivePort: " + OneServer.Port);
            GUI.Label(new Rect(Screen.width - 200, 25, 200, 25), "SendPort: " + OneServer.sendPort);
            if (OneServer.connections != null)
            {
                GUI.Label(new Rect(Screen.width - 200, 50, 200, 25), (string)OneServer.connections[0]);
            }

            if (GUI.Button(new Rect(100, 100, 200, 25), "Send test message"))
            {
				netobj.SendMethod (NetworkMessageMode.All, "Test", Random.Range (0, 10));
            }
        }
    }

	public void Test(int test) {
		Debug.Log (test);
	}
}