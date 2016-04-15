using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using Color = UnityEngine.Color;

public class Game : MonoBehaviour
{
    public static ConsoleSystem console = new ConsoleSystem();
    public static float Min;
    public static bool noGraphics;
    public static string path;
    public static int Season = 2;
    public static bool ShowFPS;
    public static bool ShowPopup;
    public static float Weather;
    public static int WeatherLength;
    public Color _GUIColor;
    public GUISkin _GUISKIN;
    public bool _Offline = false;
    public GameObject _player;
    public float _time;
    public string _Version;
    public string _Website;
    public float DayLength;
    public int TimeOut;
    private static Game _instance;

    private static Action Clear = () => { };

    private static float CurrentTime;

    private static int CW;

    private static float dt;

    private static float fps;

    private static int frames;

    private static float intendedY;

    private static float Last1;

    private static float Last2;

    private static string LastLog;

    private static string MessageForLog;

    private static Action OnNo = () => { };

    private static Action OnYes = () => { };

    private static string PopupText;

    private static bool Reachable = true;

    private static Rect rect;

    private static int Repeat = 1;

    private static bool state;

    private static float time1;

    private static float Y = -50;

    private List<string> _Console = new List<string>();

    private string _ProjectName = "";

    private Color color;

    private bool Fad;

    private Color Fading;

    private string Msg;

    private Rect roct = new Rect(0, 0, 200, 25);

    public enum ItemType { Resource = 0, Weapon = 1, Consumable = 2, Building = 3 }

    public enum wt { Nothing = 0, Handgun = 1, Rifle = 2, Sniper = 3, Melee = 4 }

    public static List<string> Console
    {
        get
        {
            return instance._Console;
        }
    }

    public static GameObject gameobject
    {
        get
        {
            return instance.gameObject;
        }
    }

    public static Color GUIColor
    {
        get
        {
            return instance._GUIColor;
        }
        set
        {
            instance._GUIColor = value;
        }
    }

    public static GUISkin GUISKIN
    {
        get
        {
            return instance._GUISKIN;
        }
    }

    public static Game instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Game>();
            }
            return _instance;
        }
    }

    public static bool Offline
    {
        get
        {
            return instance._Offline;
        }
    }

    public static GameObject player
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance._player;
        }
        set
        {
            instance._player = value;
        }
    }

    public static float time
    {
        get
        {
            if (instance == null)
            {
                return 0.0f;
            }
            return instance._time;
        }
        set
        {
            instance._time = value;
        }
    }

    public static string Version
    {
        get
        {
            return instance._Version;
        }
    }

    public static string Website
    {
        get
        {
            return instance._Website;
        }
    }

    public static void AddToConsole(string txt)
    {
        instance._Console.Add(txt);
        if (instance._Console.Count > 200)
        {
            instance._Console.RemoveAt(200);
        }
    }

    public static Color Color(int r, int g, int b, float a)
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f, a);
    }

    public static Color Color(int r, int g, int b)
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f, 1);
    }

    public static int[,] DecodeInv(Game.inventorySlot[] inven)
    {
        int[,] decInv = new int[3, inven.Length];
        for (int i = 0; i < inven.Length; i++)
        {
            decInv[0, i] = inven[i].ID;
            decInv[1, i] = inven[i].Amount;
            decInv[2, i] = inven[i].Slot;
        }

        return decInv;
    }

    public static float DragBar(Rect rct, float min, float max, float current)
    {
        Color c = Game.GUIColor;
        GUI.backgroundColor = new Color(c.r, c.g, c.b, 0.50f);
        GUI.Label(rct, "");
        current = GUI.HorizontalSlider(rct, current, min, max);
        GUI.Label(new Rect(rct.x, rct.y, rct.height, (rct.width / max) * current), "");
        GUI.backgroundColor = Game.Color(0, 0, 0, 0);
        GUI.Label(rct, Mathf.RoundToInt(current).ToString());
        GUI.backgroundColor = Game.GUIColor;
        return current;
    }

    public static float DragBar(Rect rct, float min, float max, float current, Color color)
    {
        current = Mathf.Clamp(current, min, max);
        Color c = Game.GUIColor;
        GUI.backgroundColor = new Color(c.r, c.g, c.b, 0.50f);
        GUI.Label(rct, "");
        current = GUI.HorizontalSlider(rct, current, min, max);
        color.a -= 0.5f;
        GUI.backgroundColor = color;
        GUI.Label(new Rect(rct.x, rct.y, rct.width * ((current - min) / (max - min)), rct.height), "");
        GUI.backgroundColor = Game.GUIColor;
        return current;
    }

    public static int DropDown(int current, string[] values, bool show)
    {
        int chosen = current;
        if (show)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (i == chosen)
                {
                    GUI.backgroundColor = UnityEngine.Color.green;
                }
                if (GUI.Button(Game.Rect(i - Mathf.RoundToInt(values.Length / 2), 1), values[i]))
                {
                    chosen = i;
                    show = !show;
                }
                GUI.backgroundColor = Game.GUIColor;
            }
        }
        return chosen;
    }

    public static Game.inventorySlot[] EncodeInv(int[,] encInv)
    {
        int length = encInv.Length / 3;
        Game.inventorySlot[] Inven = new Game.inventorySlot[length];
        for (int i = 0; i < length; i++)
        {
            Inven[i] = new Game.inventorySlot();
            Inven[i].ID = encInv[0, i];
            Inven[i].Amount = encInv[1, i];
            Inven[i].Slot = encInv[2, i];
        }

        return Inven;
    }

    public static int FPS()
    {
        return Mathf.RoundToInt(Game.fps);
    }

    public static string GetProjectName()
    {
        return instance._ProjectName;
    }

    public static Color InterColor(Color color)
    {
        return new Color(1.0f - color.r, 1.0f - color.g, 1.0f - color.b, 1);
    }

    public static SaveData Load(string UserID, string Username)
    {
        if (File.Exists(path + "Saves/" + PlayerPrefs.GetInt("Seed").ToString() + "/" + UserID + ".dat"))
        {
            FileStream file = File.Open(path + "Saves/" + PlayerPrefs.GetInt("Seed").ToString() + "/" + UserID + ".dat", FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            SaveData dat = bf.Deserialize(file) as SaveData;
            file.Close();

            if (dat.Version != Version)
            {
                File.Delete(path + PlayerPrefs.GetInt("Seed").ToString() + "/" + UserID + ".dat");
                Debug.Log("Player " + Username + "'s config was outdated.");
            }
            else
            {
                dat.ID = UserID;
                return dat as SaveData;
            }
        }
        return null;
    }

    public static void LoadLevel(string N)
    {
        Debug.Log("Loading level");
        SceneManager.LoadScene(N, LoadSceneMode.Single);
        if (instance == null)
        {
            SceneManager.LoadScene(N, LoadSceneMode.Single);
            return;
        }
        instance.StartCoroutine(instance.ll(N));
    }

    public static void Log(string Ms)
    {
        Game.LogToFile(Ms);
        Game.AddToConsole(Ms);
    }

    public static void Logger(string logString, string stackTrace, LogType type)
    {
        Game.AddToConsole(logString);
        if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception)
        {
            Game.MessageForLog = logString;
            if (!string.IsNullOrEmpty(stackTrace))
            {
                Game.LogToFile(logString + "\n" + stackTrace, "ERROR");
            }
            else
            {
                Game.LogToFile(logString, "ERROR");
            }
        }
        else if (type == LogType.Warning)
        {
            Game.LogToFile(logString, "WARNING");
        }
        else
        {
            Game.LogToFile(logString, "LOG");
        }
    }

    public static void LogToFile(string s)
    {
        if (LastLog == s)
        {
            Repeat++;
            File.AppendAllText(Application.dataPath + "/../UnknownStudios.txt",
                               string.Format("{0:MM/dd/yy hh:mm:ss}  " + Repeat + "x [CLIENT]: " + s, DateTime.Now));
        }
        else
        {
            LastLog = s;
            Repeat = 1;
            File.AppendAllText(Application.dataPath + "/../UnknownStudios.txt",
                               string.Format("\r\n{0:MM/dd/yy hh:mm:ss}  [CLIENT]: " + s, DateTime.Now));
        }
    }

    public static void LogToFile(string s, string name)
    {
        name = name.ToUpper();
        if (LastLog == s)
        {
            Repeat++;
            File.AppendAllText(Application.dataPath + "/../UnknownStudios.txt",
                               String.Format("{0:MM/dd/yy hh:mm:ss}  " + Repeat + "x [" + name + "]: " + s, DateTime.Now));
        }
        else
        {
            LastLog = s;
            Repeat = 1;
            File.AppendAllText(Application.dataPath + "/../UnknownStudios.txt",
                               String.Format("\r\n{0:MM/dd/yy hh:mm:ss}  [" + name + "]: " + s, DateTime.Now));
        }
    }

    public static void Notice(string Msg)
    {
        if (!string.IsNullOrEmpty(Msg))
        {
            instance.Msg = Msg;
            instance.color = Game.GUIColor;
            instance.StartCoroutine(instance.notice(2));
        }
    }

    public static void Notice(string Msg, float ti)
    {
        if (!string.IsNullOrEmpty(Msg) || ti > 0)
        {
            instance.Msg = Msg;
            instance.color = Game.GUIColor;
            instance.StartCoroutine(instance.notice(ti));
        }
    }

    public static void Notice(string Msg, Color Col)
    {
        if (!string.IsNullOrEmpty(Msg))
        {
            instance.Msg = Msg;
            instance.color = Col;
            instance.StartCoroutine(instance.notice(2));
        }
    }

    public static void Notice(string Msg, float ti, Color Col)
    {
        if (!string.IsNullOrEmpty(Msg) || ti > 0)
        {
            instance.Msg = Msg;
            instance.color = Col;
            instance.StartCoroutine(instance.notice(ti));
        }
    }

    public static void Popup(string text, Action onYes)
    {
        Game.ShowPopup = true;
        Game.PopupText = text;
        OnYes = onYes;
    }

    public static void Popup(string text, Action onYes, Action onNo)
    {
        Game.ShowPopup = true;
        Game.PopupText = text;
        OnYes = onYes;
        OnNo = onNo;
    }

    public static void ProgressBar(Rect rct, float Progress)
    {
        Progress = Mathf.Clamp(Progress, 0, 100);
        Color c = Game.GUIColor;
        GUI.backgroundColor = new Color(c.r, c.g, c.b, 0.50f);
        GUI.Label(rct, "");
        GUI.backgroundColor = Game.Color(33, 125, 48, 0.50f);
        Rect ProgressRect = new Rect(rct.x, rct.y, (rct.width / 100.0f) * Progress, rct.height);
        GUI.Label(ProgressRect, "");
        GUI.backgroundColor = Game.Color(0, 0, 0, 0);
        GUI.Label(rct, Progress.ToString() + "/100%");
        GUI.backgroundColor = Game.GUIColor;
    }

    public static void ProgressBar(Rect rct, float Progress, Color color)
    {
        Progress = Mathf.Clamp(Progress, 0, 100);
        Color c = Game.GUIColor;
        GUI.backgroundColor = new Color(c.r, c.g, c.b, 0.50f);
        GUI.Label(rct, "");
        GUI.backgroundColor = color;
        Rect ProgressRect = new Rect(rct.x, rct.y, (rct.width / 100.0f) * Progress, rct.height);
        GUI.Label(ProgressRect, "");
        GUI.backgroundColor = Game.Color(0, 0, 0, 0);
        GUI.Label(rct, Progress.ToString() + "/100%");
        GUI.backgroundColor = Game.GUIColor;
    }

    public static Rect Rect(int i)
    {
        instance.roct.x = Screen.width / 2 - 100;
        instance.roct.y = (Screen.height / 2) + (26 * i);
        return instance.roct;
    }

    public static Rect Rect(int y, int x)
    {
        instance.roct.x = (Screen.width / 2 - 100) + (201 * x);
        instance.roct.y = (Screen.height / 2) + (26 * y);
        return instance.roct;
    }

    public static float RoundToDecimals(float number, int decimals)
    {
        float divideNumber = Mathf.Pow(10, decimals);
        return Mathf.Round(number * divideNumber) / divideNumber;
    }

    public static void Save(SaveData data)
    {
        if (data != null)
        {
            if (!Directory.Exists(Game.path + "Saves/" + PlayerPrefs.GetInt("Seed").ToString()))
            {
                Directory.CreateDirectory(Game.path + "Saves/" + PlayerPrefs.GetInt("Seed").ToString());
                Debug.Log("Creating save directory");
            }
            FileStream stream = File.Create(Game.path + "Saves/" + PlayerPrefs.GetInt("Seed").ToString() + "/" + data.ID + ".dat");
            BinaryFormatter bf = new BinaryFormatter();

            SaveData SD = new SaveData();
            SD.positionX = data.positionX;
            SD.positionY = data.positionY;
            SD.positionZ = data.positionZ;
            SD.Inventory = data.Inventory;
            SD.health = data.health;
            SD.food = data.food;
            SD.water = data.water;
            SD.Version = Game.Version;
            SD.Username = data.Username;

            bf.Serialize(stream, SD);
            stream.Close();
        }
    }

    public static Color SetColor(int r, int g, int b, float a)
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f, a);
    }

    public static Color SetColor(int r, int g, int b)
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f, 1);
    }

    public static void SetMouse(bool state)
    {
        if (Cursor.visible != state)
        {
            Cursor.visible = state;
            CursorLockMode CursorState;
            if (state)
            {
                CursorState = CursorLockMode.None;
            }
            else
            {
                CursorState = CursorLockMode.Locked;
            }
            Cursor.lockState = CursorState;
            if (Game.player)
            {
                Game.player.transform.Find("Hands").Find("Armature").Find("Camera").GetComponent<Mouse>().enabled = !state;
            }
        }
    }

    public static void SetTime(float tim)
    {
        tim /= 24.0f;
        tim = Mathf.Clamp01(tim);
        instance._time = tim;
    }

    public static void StartUpdater()
    {
        if (!Application.isEditor)
        {
            Application.OpenURL(Application.dataPath + "/../Download.bat");
            Application.Quit();
        }
    }

    public static bool ToBoolean(string txt)
    {
        txt = txt.ToLower();
        if (txt == "true" || txt == "1")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool ToBoolean(int i)
    {
        if (i == 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool ToggleBox(Rect rct, bool value, string name)
    {
        Color col = GUI.backgroundColor;
        Color colo;
        if (value)
        {
            colo = Color(100, 255, 100, 0.75f);
        }
        else
        {
            colo = Color(255, 100, 100, 0.75f);
        }
        GUI.backgroundColor = colo;
        Rect rct1 = new Rect(rct.x, rct.y, rct.height, rct.height);
        if (GUI.Button(rct1, ""))
        {
            return !value;
        }
        GUI.backgroundColor = col;
        Rect rct2 = new Rect(rct.x + rct.height + 1, rct.y, rct.width - rct.height - 1, rct.height);
        if (GUI.Button(rct2, name))
        {
            return !value;
        }
        return value;
    }

    public IEnumerator ll(string N)
    {
        StartCoroutine(instance.Fade(false));
        while (instance.Fad)
        {
            yield return null;
        }
        SceneManager.LoadScene(N, LoadSceneMode.Single);
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            if (this != _instance)
            {
                Destroy(this.gameObject);
            }
        }
    }

    private IEnumerator CheckConnection()
    {
        WWW www = new WWW("https://www.google.com/");
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            Reachable = false;
        }
        else
        {
            Reachable = true;
        }
        StartCoroutine(CheckConnection());
    }

    private IEnumerator Fade(bool Out)
    {
        Fad = true;
        float target = 1.0f;

        if (Out)
        {
            target = 0.0f;
        }
        while (Fading.a != target)
        {
            Fading.a = Mathf.MoveTowards(Fading.a, target, Time.deltaTime / 2);
            yield return null;
        }

        Fad = false;
    }

    private IEnumerator notice(float time)
    {
        Y = -50;
        intendedY = 0;
        while (Y != intendedY)
        {
            Y = Mathf.MoveTowards(Y, intendedY, Time.deltaTime * 50);
            yield return null;
        }
        yield return new WaitForSeconds(time);
        intendedY = -50;
        while (Y != intendedY)
        {
            Y = Mathf.MoveTowards(Y, intendedY, Time.deltaTime * 50);
            yield return null;
        }
        if (Y == intendedY)
        {
            Msg = "";
            color = Game.GUIColor;
        }
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= Logger;
    }

    private void OnEnable()
    {
        Application.logMessageReceived += Logger;
    }

    private void OnGUI()
    {
        if (!noGraphics)
        {
            GUI.skin = GUISKIN;
            GUI.backgroundColor = GUIColor;
            GUI.depth = 1;
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;

            GUI.backgroundColor = Fading;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "");
            GUI.backgroundColor = GUIColor;

            if (ShowFPS)
            {
                GUI.Label(new Rect(0, 0, 75, 25), RoundToDecimals(fps, 2).ToString());
            }
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.backgroundColor = color;
            float Width = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(Msg)).x;
            if (Msg != "")
            {
                Width += 20;
            }
            else
            {
                Width = 0;
            }
            GUI.Label(new Rect(Screen.width / 2 - Width / 2, Y, Width, 40), Msg);
            if ((Network.isServer || Network.isClient) && CurrentTime - time1 > 2.0f)
            {
                if (!Reachable)
                {
                    if (CurrentTime - time1 >= TimeOut)
                    {
                        Debug.Log("Timeout reached, disconnecting");
                        Network.Disconnect(200);
                    }
                    GUI.Label(new Rect(Screen.width - 250, 0, 250, 25), "Disconnecting in: " + (Mathf.Round(CurrentTime - time1)) + "/" + TimeOut + "!");
                }
                else
                {
                    time1 = CurrentTime;
                }
            }
            if (ShowPopup)
            {
                GUI.backgroundColor = Game.GUIColor;
                GUI.Label(new Rect(Screen.width / 2 - 125, Screen.height / 2 - 100, 250, 98), PopupText);
                if (GUI.Button(new Rect(Screen.width / 2 - 125, Screen.height / 2, 124, 25), "Yes"))
                {
                    OnYes();
                    ShowPopup = false;
                    OnYes = Clear;
                }
                if (GUI.Button(new Rect(Screen.width / 2 + 1, Screen.height / 2, 124, 25), "No"))
                {
                    OnNo();
                    ShowPopup = false;
                    OnNo = Clear;
                }
            }
        }
    }

    private void OnLevelWasLoaded()
    {
        StartCoroutine(Fade(true));
    }

    private void Start()
    {
        string dp = Application.persistentDataPath;
        string[] s = dp.Split("/"[0]);
        _ProjectName = s[s.Length - 1];

        path = Application.persistentDataPath + "/";
        time = Time.realtimeSinceStartup;
        StartCoroutine(CheckConnection());
        Min = UnityEngine.Random.Range(0, DayLength);
        if (PlayerPrefs.HasKey("ShowFPS"))
        {
            ShowFPS = ToBoolean(PlayerPrefs.GetString("ShowFPS"));
        }
        console.Start();
    }

    private void Update()
    {
        float DL = DayLength;
        if (time > 1.0f)
        {
            time = 0.0f;
        }
        time = Mathf.Clamp01(time);
        time += (Time.deltaTime / DL) / 60;
        if (Input.GetKeyDown(KeyCode.F12))
        {
            Notice("Screenshot captured!");
            if (!Directory.Exists(path + "Screenshots"))
            {
                Directory.CreateDirectory(path + "Screenshots");
            }
            Application.CaptureScreenshot(path + "Screenshots/Screenshot " + string.Format("{0:MM/dd/yy hh:mm:ss}", DateTime.Now) + ".png");
        }
        CurrentTime = Time.realtimeSinceStartup;
        frames++;
        dt += Time.deltaTime;
        if (dt >= 1f)
        {
            fps = frames / dt;
            frames = 0;
            dt -= 1f;
        }
        if (Season > 4)
        {
            Season = 1;
        }
        if (((Time.realtimeSinceStartup - Last1) / 60) / DayLength >= 3)
        {
            Last1 = Time.realtimeSinceStartup;
            Season++;
            if (Season > 4 || Season < 1)
            {
                Season = 1;
            }
        }
        if (((Time.realtimeSinceStartup - Last2) / 60) / Min >= 1)
        {
            Last2 = Time.realtimeSinceStartup;
            Min = UnityEngine.Random.Range(0.0f, DayLength);
            Weather = UnityEngine.Random.Range(0, 101);
        }
        if (!string.IsNullOrEmpty(MessageForLog))
        {
            Notice("Error: " + MessageForLog, SetColor(200, 0, 0, 0.5f));
            MessageForLog = "";
        }

        transform.rotation = Quaternion.Euler(360f * time, 90f, 0f);
    }

    public class ConsoleSystem
    {
        private List<ConsoleCommand> cmds = new List<ConsoleCommand>();

        public void Run(string name)
        {
            for (int i = 0; i < cmds.Count; i++)
            {
                if (cmds[i].Name == name)
                {
                    Debug.Log("Running the command: " + name);
                    cmds[i].command(cmds[i].Name);
                }
            }
        }

        public void Start()
        {
            ConsoleCommand cmd1 = new ConsoleCommand("quit", Quit);
            cmds.Add(cmd1);
        }

        private void Quit(string no)
        {
            Application.Quit();
        }

        private class ConsoleCommand
        {
            public Action<string> command;
            public string Name;

            public ConsoleCommand(string _name, Action<string> callback)
            {
                Name = _name;
                command = callback;
            }
        }
    }

    [Serializable]
    public class inventorySlot
    {
        public int Amount = 0;
        public int ID = -1;
        public int Slot = 0;
    }

    [Serializable]
    public class SaveData
    {
        public float food;
        public float health;
        public string ID;
        public inventorySlot[] Inventory;
        public float positionX;
        public float positionY;
        public float positionZ;
        public string Username;
        public string Version;
        public float water;
    }
}