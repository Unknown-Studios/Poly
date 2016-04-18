using UnityEngine;
using UnityEngine.Networking;

public class DataHolder : NetworkManager
{
    public HostData connect;
    public string Type;

    private NetworkClient myClient;

    public void OnConnected(NetworkMessage msg)
    {
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        var player = (GameObject)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }

    // called when a client disconnects
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        NetworkServer.DestroyPlayersForConnection(conn);
    }

    // called when a network error occurs
    public override void OnServerError(NetworkConnection conn, int errorCode)
    {
        Debug.LogError("Server Error: " + errorCode);
    }

    // called when a client is ready
    public override void OnServerReady(NetworkConnection conn)
    {
        NetworkServer.SetClientReady(conn);
    }

    // Create a client and connect to the server port
    public void SetupClient()
    {
        myClient = new NetworkClient();

        myClient.RegisterHandler(MsgType.Connect, OnConnected);
        myClient.Connect(connect.ip.ToString(), connect.port);
    }

    public void Update()
    {
        if (Type == "Server")
        {
            SetupServer();
            Type = "";
        }
        else if (Type == "Client")
        {
            SetupClient();
            Type = "";
        }
    }

    private void SetupServer()
    {
        NetworkManager.singleton.StartHost();

        if (myClient == null)
        {
            myClient = new NetworkClient();
        }
        myClient.RegisterHandler(MsgType.Connect, OnConnected);
    }
}