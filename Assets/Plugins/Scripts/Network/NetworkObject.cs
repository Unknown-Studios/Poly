using NetworkLibrary;
using System;
using UnityEngine;

public class NetworkObject : MonoBehaviour
{
    public string UID;

    public void SendObjects(GameObject[] gameobjects)
    {
        foreach (GameObject g in gameobjects)
        {
            Instantiate(g);
        }
    }

    public void StartNetworking()
    {
        if (NetworkSystem.networkType == NetworkType.Server)
        {
            UID = Guid.NewGuid().ToString();
        }
        else
        {
        }
    }
}