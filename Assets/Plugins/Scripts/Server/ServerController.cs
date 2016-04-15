using UnityEngine.Networking;

public class ServerController : NetworkBehaviour
{
    /*#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

        public static Windows.ConsoleWindow console = new Windows.ConsoleWindow();
        public static Windows.ConsoleInput input = new Windows.ConsoleInput();

        private string strInput;
        public static GameObject Gameobject;

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void OnInputText(string obj)
        {
            Game.console.Run(obj);
        }

        private void HandleLog(string message, string stackTrace, LogType type)
        {
            if (type == LogType.Warning)
                System.Console.ForegroundColor = System.ConsoleColor.Yellow;
            else if (type == LogType.Error)
                System.Console.ForegroundColor = System.ConsoleColor.Red;
            else
                System.Console.ForegroundColor = System.ConsoleColor.Gray;

            // We're half way through typing something, so clear this line ..
            if (System.Console.CursorLeft != 0)
                input.ClearLine();

            System.Console.WriteLine(message);

            // If we were typing something re-add it.
            input.RedrawInputLine();
        }

        private void Update()
        {
            input.Update();
        }

        private void OnDestroy()
        {
            if (console != null)
            {
                console.Shutdown();
            }
        }

        public int Seed = 0;
        public string HostName = "No Server Name";
        public int Port = 35000;
        public int MaxPlayers = 32;

        public GameObject GameGame;
        private ProceduralTerrain PT;

        private void Awake()
        {
            Game.noGraphics = true;
        }

        private void Start()
        {
            if (!GameObject.Find("Game"))
            {
                GameObject g = GameObject.Instantiate(GameGame);
                g.name = "Game";
            }
            PT = GameObject.Find("GameController").GetComponent<ProceduralTerrain>();
            Gameobject = this.gameObject;

            DontDestroyOnLoad(Gameobject);

            ServerController.console.Initialize();
            ServerController.console.SetTitle("Poly Server");

            input.OnInputText += OnInputText;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("Initializing..");

            if (!File.Exists(Application.dataPath + "/Server.cfg"))
            {
                string[] N = new string[4];
                if (PlayerPrefs.HasKey("Seed"))
                {
                    N[0] = "Seed " + PlayerPrefs.GetInt("Seed");
                }
                else
                {
                    N[0] = "Seed " + Random.Range(0, 999999);
                }
                N[1] = "HostName No Server Name";
                N[2] = "Port 35000";
                N[3] = "MaxPlayers 32";
                for (int i = 0; i < N.Length; i++)
                {
                    File.AppendAllText(Application.dataPath + "/Server.cfg", N[i] + "\n");
                }
            }
            string[] Lines = File.ReadAllLines(Application.dataPath + "/Server.cfg");
            HostName = "";
            foreach (string msg in Lines)
            {
                string[] split = msg.Split(" "[0]);
                if (msg.Contains("Seed"))
                {
                    if (int.TryParse(split[1], out Seed))
                    {
                        Debug.Log("Seed: " + Seed);
                    }
                }
                else if (msg.Contains("HostName"))
                {
                    for (int str = 1; str < split.Length; str++)
                    {
                        if (str != split.Length - 1)
                        {
                            HostName += split[str] + " ";
                        }
                        else
                        {
                            HostName += split[str];
                        }
                    }
                    Debug.Log("Host name: " + HostName);
                }
                else if (msg.Contains("Port"))
                {
                    if (int.TryParse(split[1], out Port))
                    {
                        Debug.Log("Port: " + Port);
                    }
                }
                else if (msg.Contains("MaxPlayers"))
                {
                    if (int.TryParse(split[1], out MaxPlayers))
                    {
                        Debug.Log("Max connections: " + MaxPlayers);
                    }
                }
            }
            PT.Seed = Seed;
            if (HostName != "")
            {
                ServerController.console.SetTitle("Poly Server - " + HostName);
            }
            else
            {
                Debug.LogError("Please define a hostname in the Server.cfg file");
            }
            StartCoroutine(StartServer());
        }

        private IEnumerator StartServer()
        {
            QualitySettings.SetQualityLevel(1);
            PT.ServerSpawn();
            while (PT.GeneratedTiles != 1)
            {
                yield return null;
            }
            Debug.Log("Starting Server, please wait..");
            bool useNat = !Network.HavePublicAddress();
            Network.InitializeServer(MaxPlayers, Port, useNat);
            MasterServer.RegisterHost("USPoly" + Game.Version, HostName);
            Debug.Log("Server, started. Waiting for connections..");
            InvokeRepeating("AutoSave", 60.0f, 60.0f); //Auto save every 60 second.
        }

        public GameObject PlayerObject;

        public virtual void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            SetPlayerData(PlayerData);
            var player = (GameObject)GameObject.Instantiate(PlayerObject, SpawnPos, Quaternion.identity);
            player.name = PlayerPrefs.GetString("Username");
            player.tag = "Player";
            Camera cam = (Camera)FindObjectOfType(typeof(Camera));
            cam.tag = "MainCamera";

            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
            Game.player = player;
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

        private Vector3 SpawnPos;
        private Game.SaveData PlayerData;

        private void SpawnPlayer()
        {
            ProceduralTerrain.OnBeforeSpawn(SpawnPos);
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

        private void OnPlayerConnected(NetworkPlayer player)
        {
            Debug.Log("Player connected from " + player.ipAddress + ":" + player.port);
        }

        private void OnPlayerDisconnected(NetworkPlayer player)
        {
            Debug.Log("Player disconnected from " + player.ipAddress + ":" + player.port);
            Network.RemoveRPCs(player);
            Network.DestroyPlayerObjects(player);
        }

    #else
        void Start() {
            Debug.LogError("Windows is currently the only supported server platform(Dedicated)");
            Application.Quit();
        }
    #endif*/
}