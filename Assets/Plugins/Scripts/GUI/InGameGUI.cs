using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class InGameGUI : NetworkBehaviour
{
    [HideInInspector]
    public List<string> data = new List<string>();

    public bool ISClient;
    public bool ISServer;

    [HideInInspector]
    [SyncVar]
    public Game.SaveData PlayerData;

    public GameObject PlayerObject;
    private bool CanCancel;
    private string CurrentlyLoading;
    private int maxWindow;
    private ProceduralSphere PS;
    private float scrollPosition;
    private bool ShowConsole = false;
    private bool ShowInfo = false;
    private bool ShowLoading = true;
    private bool ShowPauseMenu = false;
    private bool ShowRespawnScreen = false;
    private Vector3 SpawnPos;

    private bool start = false;

    public void OnKilled()
    {
        NetworkServer.Destroy(Game.player);
        ShowRespawnScreen = true;
        ShowLoading = false;
        PlayerData = new Game.SaveData();
        SpawnPos = Vector3.zero;
    }

    private void AutoSave()
    {
        if (Network.isServer)
        {
            float time = Time.realtimeSinceStartup;
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (player.GetComponent<StoreVars>() != null)
                {
                    Game.SaveData data = GetPlayerData(player.GetComponent<StoreVars>().ID);
                    Game.Save(data);
                }
            }
            Game.Log("AutoSave complete, took: " + (Time.realtimeSinceStartup - time));
        }
    }

    private void AutoSave(bool Bool)
    {
        if (Network.isServer)
        {
            float time = Time.realtimeSinceStartup;
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (player.GetComponent<StoreVars>() != null)
                {
                    Game.SaveData data = GetPlayerData(player.GetComponent<StoreVars>().ID);
                    Game.Save(data);
                }
            }
            if (Bool)
            {
                Game.Notice("AutoSave complete, took: " + (Time.realtimeSinceStartup - time));
            }
        }
    }

    private void Awake()
    {
        if (!FindObjectOfType<NetworkManager>())
        {
            SceneManager.LoadScene("Project", LoadSceneMode.Single);
        }
        if (Game.noGraphics)
        {
            enabled = false;
        }
        else
        {
            QualitySettings.SetQualityLevel(0);
        }
        PS = GameObject.Find("GameController").GetComponent<ProceduralSphere>();
    }

    private Game.SaveData GetPlayerData(string ID)
    {
        GameObject p = GameObject.Find(ID);
        if (p == null)
        {
            Debug.Log("Player #" + ID + " wasn't found");
            return null;
        }
        if (p.GetComponent<StoreVars>() == null)
        {
            Debug.Log(p.name + "'s SaveData wasn't found");
            return null;
        }
        Game.SaveData dat = new Game.SaveData();
        Vector3 Pos = p.GetComponent<StoreVars>().Pos;
        dat.positionX = Mathf.RoundToInt(Pos.x);
        dat.positionY = Mathf.RoundToInt(Pos.y);
        dat.positionZ = Mathf.RoundToInt(Pos.z);
        dat.health = p.GetComponent<StoreVars>().Health;
        dat.water = p.GetComponent<StoreVars>().Water;
        dat.food = p.GetComponent<StoreVars>().Food;
        dat.Inventory = p.GetComponent<StoreVars>().Inventory;
        dat.Username = p.GetComponent<StoreVars>().Username;
        dat.ID = p.GetComponent<StoreVars>().ID;
        return dat;
    }

    private void OnGUI()
    {
        GUI.skin = Game.GUISKIN;
        GUI.backgroundColor = Game.GUIColor;
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        GUI.skin.button.alignment = TextAnchor.MiddleLeft;
        if (Game.ShowPopup)
        {
            GUI.enabled = false;
        }
        if (ShowLoading)
        {
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.backgroundColor = Game.SetColor(0, 0, 0, 1);
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "");
            GUI.color = Game.SetColor(255, 255, 255, 1);
            GUI.Label(Game.Rect(-1), CurrentlyLoading);
            GUI.color = Game.SetColor(127, 127, 127, 1);
            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2, 400, 25), PS.currentAction);
            GUI.color = Game.SetColor(255, 255, 255, 1);
            Game.ProgressBar(Game.Rect(1), Mathf.Floor(PS.progress * 100));
            if (CanCancel)
            {
                if (GUI.Button(Game.Rect(3), "Cancel"))
                {
                    Network.Disconnect(200);
                }
            }
        }
        else if (ShowRespawnScreen)
        {
            ShowInfo = false;
            ShowPauseMenu = false;
            Game.SetMouse(true);

            int FS = GUI.skin.label.fontSize;
            GUI.backgroundColor = Game.Color(0, 0, 0, 0);
            GUI.skin.label.fontSize = 60;
            GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(0, 50, Screen.width, 100), "You are dead", centeredStyle);

            GUI.backgroundColor = Game.GUIColor;
            GUI.skin.label.fontSize = FS;
            if (GUI.Button(Game.Rect(0), "Respawn"))
            {
                ShowRespawnScreen = false;
                ShowLoading = true;
                SpawnPos = new Vector3(Random.Range(150, PS.Width - 150), 0, Random.Range(150, PS.Width - 150));
                SpawnPlayer();
            }
            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
        }
        else
        {
            if (ShowPauseMenu)
            {
                if (Network.isServer)
                {
                    //Resume
                    if (GUI.Button(Game.Rect(-1), "Resume"))
                    {
                        ShowPauseMenu = false;
                    }
                    //Save
                    if (GUI.Button(Game.Rect(0), "Save"))
                    {
                        AutoSave(false);
                    }
                    //Disconnect
                    if (GUI.Button(Game.Rect(1), "Disconnect"))
                    {
                        Network.Disconnect();
                    }
                    //Quit
                    if (GUI.Button(Game.Rect(2), "Quit"))
                    {
                        Application.Quit();
                    }
                }
                else
                {
                    //Resume
                    if (GUI.Button(Game.Rect(-1), "Resume"))
                    {
                        ShowPauseMenu = false;
                    }
                    //Disconnect
                    if (GUI.Button(Game.Rect(0), "Disconnect"))
                    {
                        Network.Disconnect(200);
                    }
                    //Quit
                    if (GUI.Button(Game.Rect(1), "Quit"))
                    {
                        Application.Quit();
                    }
                }
            }
            if (ShowInfo || ShowConsole)
            {
                Rect WindowRect = new Rect(55, 55, Screen.width - 110, Screen.height - 110);
                GUI.Label(WindowRect, "");
                maxWindow = Mathf.FloorToInt(WindowRect.height / 26);
                WindowRect.height = Mathf.Max(0, WindowRect.height);
                if (ShowInfo)
                {
                    ShowPauseMenu = false;
                    if (Game.player)
                    {
                        data[0] = "Position: " + Game.player.transform.position;
                        data[7] = "Player ID: " + Game.player.name;
                    }
                    data[4] = "Season: " + Game.Season;
                    //data[5] = "Weather: " + sky.CurWeather;
                    data[10] = "Network players: " + (Network.connections.Length + 1);
                    scrollPosition = GUI.VerticalSlider(new Rect(Screen.width - 50, 50, 25, Screen.height - 100), scrollPosition, 0, 1);
                    scrollPosition = Mathf.Clamp01(scrollPosition);
                    int min = Mathf.RoundToInt(scrollPosition * (data.Count - maxWindow));
                    int max = Mathf.RoundToInt(min + maxWindow);
                    min = Mathf.Max(0, min);
                    max = Mathf.Min(data.Count, max);

                    for (int i = min; i < max; i++)
                    {
                        int cur = (i - min);
                        if (data[i] != null)
                        {
                            GUI.Label(new Rect(60, 60 + (26 * cur), WindowRect.width - 10, 25), "#" + (i + 1) + ": " + data[i]);
                        }
                    }
                }
                else if (ShowConsole)
                {
                    List<string> Console = Game.Console;
                    int height = Mathf.Min(Console.Count, maxWindow) - 1;
                    int offset = Screen.height - (95 + (Mathf.Max(0, height) * 25));
                    for (int t = height; t >= 0; t--)
                    {
                        GUI.Label(new Rect(60, offset + (26 * t), WindowRect.width - 10, 25), "#" + (t + 1) + ": " + Console[t]);
                    }
                }
            }
        }
    }

    private void OnPlayerDisconnected(NetworkPlayer player)
    {
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }

    private void SetPlayerData(Game.SaveData data)
    {
        if (data.ID == null)
        {
            Debug.Log("UserID not specified");
            return;
        }
        GameObject p = GameObject.Find(data.ID);
        if (p == null)
        {
            Debug.Log("Player not found.");
            return;
        }
        p.GetComponent<StoreVars>().Health = data.health;
        p.GetComponent<StoreVars>().Water = data.water;
        p.GetComponent<StoreVars>().Food = data.food;
        p.GetComponent<StoreVars>().Inventory = data.Inventory;
        p.GetComponent<StoreVars>().Username = data.Username;
        p.GetComponent<StoreVars>().ID = data.ID;
        p.GetComponent<StoreVars>().Pos = new Vector3(data.positionX, data.positionY, data.positionZ);
    }

    private void SpawnPlayer()
    {
        CurrentlyLoading = "Loading terrain..";
        ShowLoading = true;
        GameObject.Find("GameController").GetComponent<ProceduralSphere>().OnBeforeSpawn(SpawnPos);
    }

    private void Start()
    {
        if (Game.noGraphics)
        {
            enabled = false;
        }

        if (!PlayerPrefs.HasKey("UserID"))
        {
            if (!PlayerPrefs.HasKey("Seed"))
            {
                PlayerPrefs.SetInt("Seed", Random.Range(0, 1000000));
            }
            PlayerPrefs.SetString("UserID", PlayerPrefs.GetInt("Seed").ToString());
        }

        CanCancel = true;
        useGUILayout = false;

        data.Add("Position: None");
        data.Add("Map Resolution: " + PS.Width.ToString() + "x" + PS.Width.ToString());
        data.Add("Season: None");
        data.Add("Weather: None");
        data.Add("Player ID: " + PlayerPrefs.GetString("UserID"));
        data.Add("Player Name: " + PlayerPrefs.GetString("Username"));
        data.Add("Seed: " + PlayerPrefs.GetInt("Seed"));
        data.Add("Network Players: " + (Network.connections.Length + 1));
        data.Add("Connected: False");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        CurrentlyLoading = "Loading terrain..";
        InvokeRepeating("AutoSave", 60.0f, 60.0f); //Auto save every 60 second.

        Destroy(GameObject.Find("Server"));

        //I am the server, load the data from the HDD.
        if (Game.Load(PlayerPrefs.GetString("UserID"), PlayerPrefs.GetString("Username")) != null)
        {
            PlayerData = Game.Load(PlayerPrefs.GetString("UserID"), PlayerPrefs.GetString("Username"));
        }

        if (PlayerData != null)
        {
            //A save file was found, loading the position.
            SpawnPos = new Vector3(PlayerData.positionX, PlayerData.positionY + 0.5f, PlayerData.positionZ);
            SpawnPlayer();
        }
        else
        {
            //A save file wasn't found, creating a new character.
            SpawnPos = new Vector3(Random.Range(0, PS.Width), 0, Random.Range(0, PS.Width));
            SpawnPlayer();
        }

        start = true;
    }

    private void Update()
    {
        if (Game.noGraphics)
        {
            enabled = false;
        }
        if (isServer)
        {
            ISServer = true;
        }
        if (isClient)
        {
            ISClient = true;
        }
        if (start)
        {
            if (PS && PS.done)
            {
                if (Game.player != null)
                {
                    //The player and the terrain is already generated.
                    if (Input.GetKeyDown(KeyCode.F2))
                    {
                        ShowConsole = !ShowConsole;
                        Game.SetMouse(ShowConsole);
                    }
                    if (Input.GetKeyDown(KeyCode.F1))
                    {
                        ShowInfo = !ShowInfo;
                        Game.SetMouse(ShowInfo);
                    }
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        ShowPauseMenu = !ShowPauseMenu;
                        Game.SetMouse(ShowPauseMenu);
                    }
                    if (ShowInfo)
                    {
                        if (Input.GetAxis("Mouse ScrollWheel") != 0.0f)
                        {
                            scrollPosition -= (Input.GetAxis("Mouse ScrollWheel") * 10) / maxWindow;
                        }
                    }
                    CanCancel = false;
                    ShowLoading = false;
                }
                else
                {
                    ShowLoading = false;
                }
            }
        }
    }
}