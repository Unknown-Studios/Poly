using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class StoreVars : NetworkBehaviour
{
    [SyncVar]
    public float Food;

    [SyncVar]
    public float Health;

    [SyncVar]
    public string ID;

    public Game.inventorySlot[] Inventory;

    public NetworkPlayer network;

    [SyncVar]
    public Vector3 Pos;

    [SyncVar]
    public bool ReloadVars = false;

    [SyncVar]
    public string Username;

    [SyncVar]
    public float Water;

    private bool Check;

    private void Awake()
    {
        Health = 100;
        Water = 100;
        Food = 100;
    }

    [Command]
    private void CmdServerDeath(string id)
    {
        if (File.Exists(Game.path + PlayerPrefs.GetInt("Seed").ToString() + "/" + id + ".dat"))
        {
            File.Delete(Game.path + PlayerPrefs.GetInt("Seed").ToString() + "/" + id + ".dat");
        }
    }

    private void Death()
    {
        Debug.Log("Death was coming for this guy");
        if (isServer)
        {
            if (File.Exists(Game.path + PlayerPrefs.GetInt("Seed").ToString() + "/" + ID + ".dat"))
            {
                File.Delete(Game.path + PlayerPrefs.GetInt("Seed").ToString() + "/" + ID + ".dat");
            }
        }
        else if (isClient)
        {
            CmdServerDeath(ID);
        }
        GameObject.Find("GameController").GetComponent<ProceduralTerrain>().OnKilled();
        GameObject.Find("GUI").GetComponent<InGameGUI>().OnKilled();
    }

    private void health()
    {
        if (Water == 0)
        {
            Health -= 1;
        }
        else
        {
            Water -= 1;
        }
        if (Food == 0)
        {
            Health -= 1;
        }
        else
        {
            Food -= 1;
        }
    }

    private void Start()
    {
        if (isLocalPlayer)
        {
            Username = PlayerPrefs.GetString("Username");
            ID = PlayerPrefs.GetString("UserID");
            name = ID;
            InvokeRepeating("health", 0, 10);
            InvokeRepeating("Synco", 1, 30);

            ReloadVars = true;
        }
    }

    private void Synco()
    {
        if (Network.connections.Length != 0)
        {
            int[] InvI = new int[Inventory.Length];
            int[] InvA = new int[Inventory.Length];
            int[] InvS = new int[Inventory.Length];
            for (int i = 0; i < Inventory.Length; i++)
            {
                InvI[i] = Inventory[i].ID;
                InvA[i] = Inventory[i].Amount;
                InvS[i] = Inventory[i].Slot;
            }
        }
    }

    private void Update()
    {
        Pos = transform.position;
        Health = Mathf.Clamp(Health, 0, 100);
        Water = Mathf.Clamp(Water, 0, 100);
        Food = Mathf.Clamp(Food, 0, 100);
        if (Health <= 0)
        {
            Death();
        }

        if (ReloadVars && isServer)
        {
            Game.SaveData data = Game.Load(ID, Username);

            if (data != null)
            {
                Health = data.health;
                Water = data.water;
                Food = data.food;
                Inventory = data.Inventory;
                Username = data.Username;
                ID = data.ID;
                Pos = new Vector3(data.positionX, data.positionY, data.positionZ);

                transform.position = Pos;
            }
        }

        if (transform.position == Vector3.zero && Terrain.activeTerrain != null && !Check)
        {
            TerrainData td = Terrain.activeTerrain.terrainData;
            transform.position = new Vector3(Random.Range(0, td.size.x), 0, Random.Range(0, td.size.z));
            Check = true;
        }
    }
}