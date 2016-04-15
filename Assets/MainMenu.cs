using System.Collections;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public Font FontAwesome;
    public GUISkin guiskin;
    public Font MainFont;
    public Font TitleFont;

    private bool ShowJoin;
    private bool ShowOptions;
    private bool ShowPlay;
    private bool ShowServer;

    private string N = "Default Name";
    private int port = 35000;
    private int mp = 20;

    private bool ShowGraphicsMenu = false;
    private bool ShowControlsMenu = false;
    private bool ShowAudioMenu = false;
    private bool ShowGameplay = false;
    private bool ShowCredits = false;
    private bool ShowResolutions = false;

    private bool Fullscreen;
    private Resolution[] resolutions;
    private int Res;
    private float GrassDist;
    private float SDist;
    private float ViewDist;
    private int f;
    private float MaxFramerate;
    private float SensiX;
    private float SensiY;
    private float volume;

    private bool VersionCheck;
    private string VersionChecker;

    private void OnGUI()
    {
        if (Game.ShowPopup)
        {
            GUI.enabled = false;
        }
        GUI.skin = guiskin;
        GUI.backgroundColor = Color.clear;
        GUI.skin.button.fontSize = 15;
        GUI.skin.button.font = MainFont;
        GUI.skin.textField.font = MainFont;
        GUI.backgroundColor = Game.GUIColor;
        GUI.color = Color.white;

        GUI.Box(new Rect(0, Screen.height - 50, Screen.width, 50), "");

        if (ShowOptions)
        {
            GUI.skin.font = MainFont;
            if (!ShowGraphicsMenu && !ShowControlsMenu && !ShowAudioMenu && !ShowGameplay && !ShowCredits)
            {
                GUI.backgroundColor = new Color(0, 0, 0, 0);

                GUI.skin.font = TitleFont;
                GUI.color = Color.white;

                GUI.skin.label.fontSize = 75;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(0, 5, Screen.width, 100), "Options");
                GUI.skin.label.fontSize = 15;
                GUI.skin.font = MainFont;

                GUI.backgroundColor = Game.GUIColor;

                if (GUI.Button(Game.Rect(-2), "Update game"))
                {
                    Game.Popup("The game will close before updating, are you sure?", Game.StartUpdater);
                }

                if (GUI.Button(Game.Rect(-1), "Graphics"))
                {
                    ShowGraphicsMenu = true;
                }
                if (GUI.Button(Game.Rect(0), "Controls"))
                {
                    ShowControlsMenu = true;
                }
                if (GUI.Button(Game.Rect(1), "Audio"))
                {
                    ShowAudioMenu = true;
                }
                if (GUI.Button(Game.Rect(2), "Gameplay"))
                {
                    ShowGameplay = true;
                }
                if (GUI.Button(Game.Rect(3), "Credits"))
                {
                    ShowCredits = true;
                }
            }
            if (ShowGraphicsMenu)
            {
                GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 86, 200, 25), "Graphic settings");
                GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                Fullscreen = Game.ToggleBox(Game.Rect(-2), Fullscreen, "Fullscreen");
                if (Fullscreen)
                {
                    f = 1;
                }
                else
                {
                    f = 0;
                }
                if (resolutions != null && GUI.Button(Game.Rect(-1), "Resolution: " + resolutions[Res].width + "x" + resolutions[Res].height))
                {
                    ShowResolutions = !ShowResolutions;
                }
                string[] resos = new string[resolutions.Length];
                for (int i = 0; i < resos.Length; i++)
                {
                    resos[i] = resolutions[i].width + "x" + resolutions[i].height;
                }
                Res = Game.DropDown(Res, resos, ShowResolutions);
                GrassDist = Game.DragBar(Game.Rect(0), 0.0f, 250.0f, GrassDist, Color.green);
                string s3;
                if (GrassDist == 0)
                {
                    s3 = "Grass: off";
                }
                else
                {
                    s3 = "Grass Dis stance: " + Mathf.RoundToInt(GrassDist).ToString();
                }
                GUI.backgroundColor = new Color(0, 0, 0, 0);
                GUI.Label(Game.Rect(0), s3);
                GUI.backgroundColor = Game.GUIColor;
                SDist = Game.DragBar(Game.Rect(1), 0f, 500f, SDist, Color.black);
                string s;
                s = "Shadow Distance: " + Mathf.RoundToInt(SDist).ToString();
                SDist = Mathf.RoundToInt(SDist);
                if (SDist == 0)
                {
                    s = "Shadows: off";
                }
                GUI.backgroundColor = new Color(0, 0, 0, 0);
                GUI.Label(Game.Rect(1), s);
                ViewDist = Game.DragBar(Game.Rect(2), 250f, 1000f, ViewDist, new Color(130, 202, 255));
                string v = "View Distance: " + Mathf.RoundToInt(ViewDist).ToString();
                GUI.backgroundColor = new Color(0, 0, 0, 0);
                GUI.Label(Game.Rect(2), v);

                GUI.backgroundColor = Game.GUIColor;
                if (GUI.Button(Game.Rect(4), "Apply"))
                {
                    PlayerPrefs.SetInt("Screenmanager Resolution Height", resolutions[Res].height);
                    PlayerPrefs.SetInt("Screenmanager Resolution Width", resolutions[Res].width);
                    PlayerPrefs.SetInt("Screenmanager Is Fullscreen mode", f);
                    PlayerPrefs.SetFloat("ShadowDistance", SDist);
                    PlayerPrefs.SetFloat("GrassDistance", GrassDist);
                    PlayerPrefs.SetFloat("ViewDistance", ViewDist);
                    QualitySettings.shadowDistance = SDist;
                    if (SDist == 0)
                    {
                        Game.gameobject.GetComponent<Light>().shadows = LightShadows.None;
                    }
                    else
                    {
                        Game.gameobject.GetComponent<Light>().shadows = LightShadows.Hard;
                    }
                    Screen.SetResolution(resolutions[Res].width, resolutions[Res].height, Fullscreen);
                    PlayerPrefs.Save();
                    Game.Log("Settings applied!");
                }
                if (GUI.Button(Game.Rect(5), "Back"))
                {
                    ShowResolutions = false;
                    ShowCredits = false;
                    ShowAudioMenu = false;
                    ShowControlsMenu = false;
                    ShowGameplay = false;
                    ShowGraphicsMenu = false;
                }
            }
            if (ShowControlsMenu)
            {
                GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 + ((26 * -3) + 6), 200, 25), "Controls");
                if (GUI.Button(Game.Rect(1), "Apply"))
                {
                    PlayerPrefs.Save();
                    Game.Log("Settings applied!");
                }
                if (GUI.Button(Game.Rect(2), "Back"))
                {
                    ShowResolutions = false;
                    ShowCredits = false;
                    ShowAudioMenu = false;
                    ShowControlsMenu = false;
                    ShowGameplay = false;
                    ShowGraphicsMenu = false;
                }
            }
            if (ShowGameplay)
            {
                GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 + ((26 * -2) - 6), 200, 25), "Gameplay Settings:");
                MaxFramerate = Game.DragBar(Game.Rect(0), 30, 256, MaxFramerate, Color.magenta);
                string Framerate;
                if (MaxFramerate == 256)
                {
                    Framerate = "Max FPS: Infinite";
                }
                else
                {
                    Framerate = "Max FPS: " + MaxFramerate.ToString();
                }

                SensiX = Game.DragBar(Game.Rect(-1), 1, 30, SensiX, Color.yellow);
                GUI.backgroundColor = new Color(0, 0, 0, 0);
                GUI.Label(Game.Rect(-1), "Sensitivity X: " + Game.RoundToDecimals(SensiX, 2).ToString());

                SensiY = Game.DragBar(Game.Rect(1), 1, 30, SensiY, Color.yellow);
                GUI.backgroundColor = new Color(0, 0, 0, 0);
                GUI.Label(Game.Rect(0), "Sensitivity Y: " + Game.RoundToDecimals(SensiY, 2).ToString());
                GUI.backgroundColor = Game.GUIColor;

                GUI.backgroundColor = new Color(0, 0, 0, 0);
                GUI.Label(Game.Rect(1), Framerate);
                GUI.backgroundColor = Game.GUIColor;

                Game.ShowFPS = Game.ToggleBox(Game.Rect(2), Game.ShowFPS, "Show FPS");

                if (GUI.Button(Game.Rect(4), "Apply"))
                {
                    PlayerPrefs.SetFloat("MaxFramerate", MaxFramerate);
                    PlayerPrefs.SetFloat("Sensitivity X", SensiX);
                    PlayerPrefs.SetFloat("Sensitivity Y", SensiY);
                    PlayerPrefs.SetString("ShowFPS", Game.ShowFPS.ToString());
                    if (MaxFramerate == 501)
                    {
                        Application.targetFrameRate = -1;
                    }
                    else
                    {
                        Application.targetFrameRate = Mathf.RoundToInt(MaxFramerate);
                    }
                    PlayerPrefs.Save();
                    Game.Log("Settings applied!");
                }
                if (GUI.Button(Game.Rect(5), "Back"))
                {
                    ShowResolutions = false;
                    ShowCredits = false;
                    ShowAudioMenu = false;
                    ShowControlsMenu = false;
                    ShowGameplay = false;
                    ShowGraphicsMenu = false;
                }
            }
            if (ShowAudioMenu)
            {
                GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 + ((26 * -1) - 16), 200, 25), "Audio Settings:");
                volume = Game.DragBar(Game.Rect(0), 0f, 100f, volume, Game.Color(100, 255, 100));
                string s1;
				if (volume == 0)
                {
                    s1 = "Audio: off";
                }
                else
                {
                    s1 = "Volume: " + volume.ToString() + "%";
                }
                GUI.backgroundColor = new Color(0, 0, 0, 0);
                GUI.Label(Game.Rect(0), s1);
                GUI.backgroundColor = Game.GUIColor;
                if (GUI.Button(Game.Rect(2), "Apply"))
                {
                    PlayerPrefs.SetFloat("Volume", volume);
                    PlayerPrefs.Save();
                    Game.Log("Settings applied!");
                }
                if (GUI.Button(Game.Rect(3), "Back"))
                {
                    ShowResolutions = false;
                    ShowCredits = false;
                    ShowAudioMenu = false;
                    ShowControlsMenu = false;
                    ShowGameplay = false;
                    ShowGraphicsMenu = false;
                }
            }
            if (ShowCredits)
            {
                GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 + ((26 * -2) - 16), 200, 25), "Credits:");
                if (GUI.Button(Game.Rect(-1), "Coder: Markus Moltke"))
                {
                    Application.OpenURL(Game.Website + "/Member/?UID=76561198060672807");
                }
                if (GUI.Button(Game.Rect(0), "Designer: Markus Moltke"))
                {
                    Application.OpenURL(Game.Website + "/Member/?UID=76561198060672807");
                }
                if (GUI.Button(Game.Rect(2), "Back"))
                {
                    ShowResolutions = false;
                    ShowCredits = false;
                    ShowAudioMenu = false;
                    ShowControlsMenu = false;
                    ShowGameplay = false;
                    ShowGraphicsMenu = false;
                }
            }
        }
        else if (ShowServer)
        {
            //Start server GUI
            N = GUI.TextField(Game.Rect(-2), N);
            int.TryParse(GUI.TextField(Game.Rect(-1), port.ToString()), out port);
            port = Mathf.Clamp(port, 1023, 65535);
            int.TryParse(GUI.TextField(Game.Rect(0), mp.ToString()), out mp);
            mp = Mathf.Clamp(mp, 1, 255);

            GUI.skin.button.fontSize = 15;
            if (GUI.Button(Game.Rect(2), "Start") || Input.GetKeyDown(KeyCode.Return))
            {
                if (N == "" || N == "Default Name")
                {
                    Game.Notice("Please choose another server name");
                    return;
                }
                HostData hd = new HostData();
                hd.gameName = N;
                hd.port = port;
                hd.playerLimit = mp;
                hd.useNat = !Network.HavePublicAddress();

                PlayerPrefs.SetString("ServerName", N);
                PlayerPrefs.SetInt("ServerPort", port);
                PlayerPrefs.SetInt("ServerMP", mp);
                PlayerPrefs.Save();

                DataHolder dh = GameObject.Find("ServerManager").GetComponent<DataHolder>();
                dh.connect = hd;
                dh.Type = "Server";
            }
        }
        else if (ShowJoin)
        {
            //Join server GUI
        }
        else if (ShowPlay)
        {
            //Sub-menu for joining/hosting server
            GUI.backgroundColor = Color.clear;
            GUI.skin.button.fontSize = 40;
            GUI.skin.button.font = FontAwesome;
            if (GUI.Button(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 62.5f, 200, 125), " \n Host"))
            {
                ShowServer = true;
            }
            if (GUI.Button(new Rect(Screen.width / 2, Screen.height / 2 - 62.5f, 200, 125), " \n Connect"))
            {
                ShowJoin = true;
            }
            GUI.skin.button.font = MainFont;
        }
        GUI.backgroundColor = Color.clear;
        GUI.color = Color.white;
        GUI.skin.button.fontSize = 20;
        GUI.skin.button.font = MainFont;
        if (GUI.Button(new Rect(Screen.width / 2 - 300, Screen.height - 65, 200, 75), "Play"))
        {
            ShowPlay = !ShowPlay;
            ShowOptions = false;
            ShowServer = false;
            ShowJoin = false;
        }
        if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height - 65, 200, 75), "Options"))
        {
            ShowOptions = !ShowOptions;
            ShowPlay = false;
            ShowServer = false;
            ShowJoin = false;
        }
        if (GUI.Button(new Rect(Screen.width / 2 + 100, Screen.height - 65, 200, 75), "Quit"))
        {
            Application.Quit();
        }

        GUI.skin.button.fontSize = 14;
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

    private void Update()
    {
        if (!VersionCheck && !string.IsNullOrEmpty(VersionChecker))
        {
            VersionCheck = true;
            string[] lineV = Game.Version.Split("."[0]);
            string[] lineSV = VersionChecker.Split("."[0]);
            string linenV = "";
            string linenSV = "";
            foreach (string line1 in lineV)
            {
                linenV += line1;
            }
            foreach (string line2 in lineSV)
            {
                linenSV += line2;
            }

            int CurVersion = int.Parse(linenV.ToString());
            int CurServerVersion = 0;
            if (!int.TryParse(linenSV.ToString(), out CurServerVersion))
            {
                Debug.LogError("The updating server seems to be down!");
            }
            if (CurVersion < CurServerVersion)
            {
                Debug.Log(VersionChecker);
                Game.Popup("A new version of " + Game.GetProjectName() + " was found, do you want to update now? (" + VersionChecker + ")", Game.StartUpdater);
            }
            else
            {
                Game.Notice("The newest version of " + Game.GetProjectName() + " is already installed (" + VersionChecker + ")", 15);
            }
        }
    }

    private void Start()
    {
        if (!PlayerPrefs.HasKey("FirstTimeRun"))
        {
            PlayerPrefs.SetString("ServerName", "No name");
            PlayerPrefs.SetString("ServerPort", "35000");
            PlayerPrefs.SetString("ServerMP", "32");
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
            PlayerPrefs.Save();
        }

        //Retrieve saved values from registry
        N = PlayerPrefs.GetString("ServerName");
        port = PlayerPrefs.GetInt("ServerPort");
        mp = PlayerPrefs.GetInt("ServerMP");

        resolutions = Screen.resolutions;
        useGUILayout = false;

        //Update check
        if (!Game.Offline)
        {
            GetData("https://dl.dropboxusercontent.com/s/vz0o49eumh6rzxl/Version");
        }
    }
}