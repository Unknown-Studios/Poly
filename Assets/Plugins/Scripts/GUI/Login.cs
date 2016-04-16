using SimpleJSON;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Login : MonoBehaviour
{
    public string jsonData = "";
    private string Username = "Username";
    private string Password = "Password";
    private bool Remember = false;

    private string LoadText = "Loading, please wait!";
    private string error = "";
    private bool LoggingIn = false;
    private string text = "Login";

    private static string Md5Sum(string strToEncrypt)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        byte[] bytes = encoding.GetBytes(strToEncrypt);

        // encrypt bytes
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, "0"[0]);
        }

        return hashString.PadLeft(32, "0"[0]);
    }

    private void Start()
    {
        if (GameObject.Find("Server"))
        {
            Destroy(GameObject.Find("Server"));
        }
        Game.SetMouse(true);
    }

    private void OnGUI()
    {
        if (!Game.ShowPopup)
        {
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin = Game.GUISKIN;
            GUI.color = Color.white;
            GUI.backgroundColor = Game.Color(39, 39, 39, 0.75f);
            if (!LoggingIn)
            {
                if (!PlayerPrefs.HasKey("Password") || error != "")
                {
                    Game.SetMouse(true);
                    GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 86, 200, 25), "Login form");

                    Username = GUI.TextField(Game.Rect(-2), Username, 25);
                    Password = GUI.PasswordField(Game.Rect(-1), Password, "#"[0], 25);
                    Remember = Game.ToggleBox(Game.Rect(0), Remember, "Remember Password");

                    if (GUI.Button(Game.Rect(2), text) || Input.GetKeyDown(KeyCode.Return))
                    {
                        if (Username.ToString().Length == 0 || Username.ToString() == "Username")
                        {
                            error = "Username is missing!";
                        }
                        else if (Password.ToString().Length == 0 || Password.ToString() == "Password")
                        {
                            error = "Password is missing!";
                        }
                        else {
                            StartCoroutine(login(Username, Md5Sum(Password), false));
                        }
                    }
                    if (GUI.Button(Game.Rect(3), "Quit"))
                    {
                        Application.Quit();
                    }
                    //Show errors to the client.
                    if (error != "")
                    {
                        Vector2 ErrorSize = GUI.skin.GetStyle("Label").CalcSize(new GUIContent("Error: " + error));
                        ErrorSize.x += 20;
                        GUI.backgroundColor = Game.Color(255, 0, 0, 0.4f);
                        GUI.Label(new Rect((Screen.width / 2) - (ErrorSize.x) / 2, 0, ErrorSize.x, 25), "Error: " + error);
                    }
                }
                else {
                    LoggingIn = true;
                    StartCoroutine(login(PlayerPrefs.GetString("Username"), PlayerPrefs.GetString("Password"), true));
                }
            }
            else {
                GUI.Label(new Rect((Screen.width / 2) - (300 / 2), (Screen.height / 2) - (50 / 2), 300, 50), LoadText);
            }
        }
    }

    private IEnumerator login(string username, string password, bool auto)
    {
        if (Game.Offline)
        {
            PlayerPrefs.SetString("Username", username);
            PlayerPrefs.SetString("UserID", Random.Range(1, 9999999).ToString());
            if (Remember)
            {
                PlayerPrefs.SetString("Password", password);
            }
            Game.LoadLevel("Project");
            yield break;
        }
        LoggingIn = true;
        string url = Game.Website + "/Login/LoginCheck.php?Username=" + username + "&Password=" + password;
        StartCoroutine(GetData(url));
        float time = Time.realtimeSinceStartup;
        while (string.IsNullOrEmpty(jsonData))
        {
            if (Time.realtimeSinceStartup - time > 10)
            {
                error = "Timeout!";
                break;
            }
            yield return null;
        }
        JSONNode N = JSON.Parse(jsonData);
        if (N == null)
        {
            Debug.LogError("An unknown error occurred, please try again later");
            LoggingIn = false;
            yield break;
        }
        if (N["Access"] == null)
        {
            Game.Popup("The username wasn't found in the database, do you want to create a user?", delegate () { Application.OpenURL(Game.Website + "/Login/#Signup"); });
            LoggingIn = false;
            error = "User not found in database, check your credentials and try again.";
            yield break;
        }
        if (N["GameOwner"].Value != "true")
        {
            if (auto || PlayerPrefs.HasKey("Username"))
            {
                PlayerPrefs.DeleteKey("Username");
                PlayerPrefs.DeleteKey("Password");
                Game.LoadLevel(SceneManager.GetActiveScene().name);
            }
            Game.Popup("You don't own the game, do you want to buy it?", delegate () { Application.OpenURL(Game.Website + "/Buy.php"); });
            LoggingIn = false;
            yield break;
        }

        if (N != null && N["Access"].Value == "true")
        {
            PlayerPrefs.SetString("Username", username);
            PlayerPrefs.SetString("UserID", N["UserID"].Value);
            if (Remember)
            {
                PlayerPrefs.SetString("Password", password);
            }
            PlayerPrefs.Save();
            Game.LoadLevel("Project");
        }
        else {
            error = "Wrong Username/Password";
            LoggingIn = false;
            yield break;
        }
    }

    private IEnumerator GetData(string url)
    {
        WWW dataW = new WWW(url);
        while (!dataW.isDone)
        {
            yield return null;
        }
        if (!string.IsNullOrEmpty(dataW.error))
        {
            Debug.LogError(dataW.error);
        }
        else {
            jsonData = dataW.text;
        }
    }
}