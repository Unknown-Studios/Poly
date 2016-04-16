using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

public class StartMenu : MonoBehaviour
{
    public List<MenuItem> ConnectMenu = new List<MenuItem>();
    public GameObject GameController;
    public List<MenuItem> MainMenu = new List<MenuItem>();
    public Font MenuFont;
    public List<MenuItem> Options = new List<MenuItem>();
    public Color TitleColor;
    public Font TitleFont;
    private string ActualVersion;
    private string ConnectingText = "Loading level!";
    private int currentindex = 0;
    private string CurrentlyShowing;
    private List<MenuItem> currentmenu;
    private int Direction = 0;
    private float height;

    private string MaxPlayers = "32";
    private Game.SaveData PlayerData;
    private string Port = "35000";
    private ProceduralTerrain PT;
    private int QLevel;
    private Rect rectangle;
    private Rect rectangle1;
    private int Res = 0;
    private Resolution[] resolutions;
    private bool Respawn;
    private bool Running = false;
    private float scrollPosition;
    private float SDist;
    private int Seed;
    private string seedtest = "";

    private float SensiX;
    private float SensiY;
    private string ServerName = "No name";
    private bool ShowAudioMenu;
    private bool ShowConnecting;
    private bool ShowControlsMenu;
    private bool ShowCredits;
    private bool ShowGameplay;
    private bool ShowGraphicsMenu;
    private bool ShowHostMenu;
    private bool ShowJoinMenu;
    private bool ShowOptions;
    private bool showResolution = false;
    private Vector3 SpawnPos;
    private string switchtext;
    private float target;
    private bool VersionCheck;
    private string VersionChecker;
    private float ViewDist;
    private float volume;
    private float width;
    private float X;

    public void Awake()
    {
        width = Screen.width / 2;
        height = Screen.height / 2;

        X = Screen.width / 2;
        currentmenu = MainMenu;
        Action ma = () => { SwitchMenu(ConnectMenu); };
        MainMenu.Add(new MenuItem("Play", ma));

        Action ma1 = () => { ShowOptions = true; };
        MainMenu.Add(new MenuItem("Options", ma1));

        Action ma2 = () => { Application.Quit(); };
        MainMenu.Add(new MenuItem("Quit", ma2));

        Action ca = () => { ShowJoinMenu = true; };
        ConnectMenu.Add(new MenuItem("Join", ca));

        Action ca1 = () => { ShowHostMenu = true; };
        ConnectMenu.Add(new MenuItem("Host", ca1));
    }

    private void Connect(string t, HostData data)
    {
        if (FindObjectsOfType<DataHolder>() != null)
        {
            DataHolder dh = FindObjectOfType<DataHolder>();
            dh.connect = data;
            dh.Type = t;
        }
    }

    private IEnumerator GetData(string url)
    {
        WWW dataW = new WWW(url);
        while (!dataW.isDone)
        {
            yield return 0;
        }
        if (!string.IsNullOrEmpty(dataW.error))
        {
            Debug.LogError("Couldn't find the server, more info in log file!");
            Game.LogToFile(dataW.error);
        }
        else
        {
            VersionChecker = dataW.text;
        }
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= Game.Logger;
    }

    private void OnEnable()
    {
        Application.logMessageReceived += Game.Logger;
    }

    private void OnGUI()
    {
        GUI.skin = Game.GUISKIN;
        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        GUI.skin.button.alignment = TextAnchor.UpperCenter;
        GUI.skin.label.normal.textColor = Color.white;
        GUI.backgroundColor = Game.GUIColor;
        Font fo = GUI.skin.font;
        if (Game.ShowPopup)
        {
            GUI.enabled = false;
        }
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        if (!ShowHostMenu && !ShowJoinMenu && !ShowConnecting && !ShowOptions && !ShowGraphicsMenu && !ShowAudioMenu && !ShowControlsMenu && !ShowGameplay && !ShowCredits)
        {
            if (!Running)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow) && currentindex + 1 < currentmenu.Count)
                {
                    StartCoroutine(SwitchSelection(1));
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow) && currentindex - 1 >= 0)
                {
                    StartCoroutine(SwitchSelection(-1));
                }
            }

            //Play

            GUI.skin.label.fontSize = 75;

            GUI.skin.font = TitleFont;
            GUI.color = Color.white;
            Vector2 sizo = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(Game.GetProjectName()));
            GUI.Label(new Rect(0, 5, Screen.width, sizo.y), Game.GetProjectName());
            GUI.skin.label.fontSize = 15;

            GUI.skin.button.fontSize = 60;

            if (currentmenu == null || currentmenu.Count == 0)
            {
                currentmenu = MainMenu;
                return;
            }
            CurrentlyShowing = currentmenu[currentindex].name;

            Vector2 siz = GUI.skin.GetStyle("Button").CalcSize(new GUIContent(CurrentlyShowing));
            if (Input.GetKeyDown(KeyCode.Return) || GUI.Button(new Rect(X - (siz.x / 2), (Screen.height / 2) - (siz.y / 2), siz.x, siz.y), CurrentlyShowing))
            {
                currentmenu[currentindex].action();
            }

            Vector2 swizo = GUI.skin.GetStyle("Button").CalcSize(new GUIContent(switchtext));
            if (GUI.Button(new Rect((X + (Screen.width * Direction)) - (swizo.x / 2), (Screen.height / 2) - (swizo.y / 2), swizo.x, swizo.y), switchtext))
            {
                currentmenu[currentindex].action();
            }
        }
        if (currentmenu != MainMenu && !ShowConnecting || ShowOptions || ShowControlsMenu || ShowGraphicsMenu || ShowAudioMenu || ShowGameplay || ShowCredits)
        {
            GUI.skin.font = TitleFont;
            GUI.skin.button.fontSize = 35;
            if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height - 80, 200, 100), "Back"))
            {
                ShowJoinMenu = false;
                ShowHostMenu = false;

                ShowOptions = false;
                ShowControlsMenu = false;
                ShowGraphicsMenu = false;
                ShowAudioMenu = false;
                ShowGameplay = false;
                ShowCredits = false;

                SwitchMenu(MainMenu);
            }
        }

        GUI.backgroundColor = Game.GUIColor;
        GUI.skin.font = fo;
        GUI.skin.button.fontSize = 15;
        if (ShowConnecting)
        {
            GUI.Label(new Rect(width - 250, height - 25, 500, 50), ConnectingText);
        }
        if (ShowJoinMenu)
        {
            //Not done yet.
        }
        if (ShowHostMenu)
        {
            GUI.skin.font = MenuFont;
            GUI.Label(new Rect(width - 100, height - 60, 200, 25), "Host");

            GUI.Label(Game.Rect(-1, -1), "Name: ");
            ServerName = GUI.TextField(Game.Rect(-1), ServerName, 40);

            GUI.Label(Game.Rect(0, -1), "Port: ");
            string port = Port;
            int porttest = 0;
            Port = GUI.TextField(Game.Rect(0), Port, 5);
            if (!int.TryParse(Port, out porttest))
            {
                Port = port;
            }

            GUI.Label(Game.Rect(1, -1), "Max players: ");
            string mp = MaxPlayers;
            int mptest = 0;
            MaxPlayers = GUI.TextField(Game.Rect(1), MaxPlayers, 3);
            if (!int.TryParse(MaxPlayers, out mptest))
            {
                MaxPlayers = mp;
            }

            GUI.Label(Game.Rect(2, -1), "Seed: ");
            string seed = seedtest;
            int somenumber = 0;
            Rect REekt = Game.Rect(2);
            REekt.width -= 26;
            seedtest = GUI.TextField(REekt, seedtest.ToString(), 7);
            if (!int.TryParse(seedtest, out somenumber))
            {
                seedtest = seed;
            }
            else
            {
                Seed = somenumber;
            }
            GUI.backgroundColor = new Color(1, 1, 0, 0.35f);
            if (GUI.Button(new Rect(width + 75, height + 52, 25, 25), ""))
            {
                Seed = Random.Range(0, 999999);
                seedtest = Seed.ToString();
            }
            GUI.backgroundColor = Game.GUIColor;

            if (GUI.Button(Game.Rect(4), "Start server"))
            {
                if (ServerName == "" || ServerName.ToLower() == "no name" || ServerName.Length < 5)
                {
                    if (ServerName.Length < 5)
                    {
                        Game.Notice("Server name has to be longer than 5 letters!", 1);
                    }
                    else
                    {
                        Game.Notice("Server name isn't set.", 1);
                    }
                    return;
                }
                if (Port == "")
                {
                    Port = "35000";
                }
                if (MaxPlayers == "" || MaxPlayers == "0")
                {
                    MaxPlayers = "32";
                }
                if (Seed == 0)
                {
                    Seed = Random.Range(1, 999999);
                }
                PlayerPrefs.SetString("ServerName", ServerName);
                PlayerPrefs.SetInt("Seed", Seed);
                PlayerPrefs.SetString("ServerPort", Port);
                PlayerPrefs.SetInt("ServerMaxPlayers", int.Parse(MaxPlayers));
                PlayerPrefs.Save();
                ShowHostMenu = false;
                ShowConnecting = true;

                Connect("Server", null);
            }
        }

        if (ShowOptions)
        {
        }
    }

    private void Start()
    {
        if (!PlayerPrefs.HasKey("FirstTimeRun"))
        {
            PlayerPrefs.SetString("ServerName", "No name");
            PlayerPrefs.SetString("ServerPort", "35000");
            PlayerPrefs.SetString("ServerMaxPlayers", "32");
            PlayerPrefs.SetInt("Seed", Random.Range(0, 999999));
            PlayerPrefs.SetInt("ShadowDistance", 150);
            PlayerPrefs.SetFloat("MaxFramerate", 60);
            PlayerPrefs.SetInt("GrassDistance", 150);
            PlayerPrefs.SetInt("ViewDistance", 150);
            PlayerPrefs.SetFloat("Sensitivity X", 15.0f);
            PlayerPrefs.SetFloat("Sensitivity Y", 15.0f);
            PlayerPrefs.SetInt("Volume", 100);

            Application.OpenURL(Application.dataPath + "/../GetPermission.bat");

            PlayerPrefs.SetString("FirstTimeRun", "Done");
        }
        else
        {
            ServerName = PlayerPrefs.GetString("ServerName");
            Port = PlayerPrefs.GetString("ServerPort");
            seedtest = PlayerPrefs.GetInt("Seed").ToString();
            MaxPlayers = PlayerPrefs.GetInt("ServerMaxPlayers").ToString();
        }
        resolutions = Screen.resolutions;
        useGUILayout = false;

        //Update check
        if (!Game.Offline)
        {
            if (Application.isEditor)
            {
                if (File.Exists(Application.dataPath + "/../Version"))
                {
                    StreamReader sr = new StreamReader(Application.dataPath + "/../Version");
                    string fileContents = sr.ReadToEnd();
                    sr.Close();
                    string[] lines = fileContents.Split("\n"[0]);
                    ActualVersion = lines[0];
                    GetData("https://dl.dropboxusercontent.com/s/vz0o49eumh6rzxl/Version");
                }
                else
                {
                    Debug.Log("Version Check failed! (File not found)");
                }
            }
            else
            {
                Debug.Log("Checking current version!");
                if (File.Exists(Application.dataPath + "/Version"))
                {
                    StreamReader sr1 = new StreamReader(Application.dataPath + "/Version");
                    string fileContents1 = sr1.ReadToEnd();
                    sr1.Close();
                    string[] lines1 = fileContents1.Split("\n"[0]);
                    ActualVersion = lines1[0];
                    Debug.Log("Current Version: " + ActualVersion);
                    GetData("https://dl.dropboxusercontent.com/s/vz0o49eumh6rzxl/Version");
                }
                else
                {
                    Debug.Log("Version Check failed! (File not found)");
                }
            }
        }
    }

    private void StartUpdater()
    {
        Game.StartUpdater();
    }

    private void SwitchMenu(List<MenuItem> menu)
    {
        currentindex = 0;
        currentmenu = menu;
    }

    private IEnumerator SwitchSelection(int direction)
    {
        Direction = direction;
        switchtext = currentmenu[currentindex + Direction].name;
        if (Direction > 0) //Right
        {
            target = -(Screen.width / 2);
            Running = true;
        }
        else if (Direction < 0) //Left
        {
            target = Screen.width * 1.5f;
            Running = true;
        }
        while (X != target)
        {
            X = Mathf.MoveTowards(X, target, Time.deltaTime * Screen.width);
            yield return null;
        }

        currentindex += Direction;
        CurrentlyShowing = currentmenu[currentindex].name;
        target = 0f;
        X = Screen.width / 2;
        switchtext = "";
        Running = false;
    }

    public class MenuItem
    {
        public Action action = () => { };
        public string name;

        public MenuItem(string n, Action func)
        {
            name = n;
            action = func;
        }
    }
}