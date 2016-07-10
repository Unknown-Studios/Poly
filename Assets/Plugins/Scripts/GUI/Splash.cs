using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Splash : MonoBehaviour
{
    public Texture2D icon;
    public string Name;
    private Texture2D iconInvert;
    private Color color = new Color(1, 1, 1, 1);

    private bool fadeIn = true;

    private bool fadeOut = false;

    private float transparency = 0;

    private void OnGUI()
    {
        GUI.skin = Game.GUISKIN;
        GUI.color = color;
        GUI.skin.label.normal.textColor = color;
        int fonts = GUI.skin.label.fontSize;
        GUI.skin.label.fontSize = 25;
        GUI.backgroundColor = Color.clear;
        GUI.Label(new Rect(0, Screen.height / 2, Screen.width, 50), Name);
        GUI.DrawTexture(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 100, 100, 100), iconInvert);
        GUI.skin.label.fontSize = fonts;
    }

    private void Start()
    {
        Screen.SetResolution(PlayerPrefs.GetInt("Screenmanager Resolution Width"), PlayerPrefs.GetInt("Screenmanager Resolution Height"), Game.ToBoolean(PlayerPrefs.GetInt("Screenmanager Is Fullscreen mode")));
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        iconInvert = new Texture2D(icon.width, icon.height, TextureFormat.RGBA32, true);
        for (int x = 0; x < icon.width; x++)
        {
            for (int y = 0; y < icon.height; y++)
            {
                iconInvert.SetPixel(x, y, Game.InterColor(icon.GetPixel(x, y)));
            }
        }
        iconInvert.Apply();
    }

    private void Update()
    {
        if (Input.anyKey)
        {
            SceneManager.LoadScene("Login");
        }
        if (fadeIn)
        {
            transparency = Mathf.MoveTowards(transparency, 75, 25 * Time.deltaTime);
            if (transparency == 75)
            {
                StartCoroutine(Wait());
            }
        }
        else if (fadeOut)
        {
            transparency = Mathf.MoveTowards(transparency, 0, 25 * Time.deltaTime);
            if (transparency == 0)
            {
                SceneManager.LoadScene("Login");
            }
        }
        color.a = transparency / 100.0f;
    }

    private IEnumerator Wait()
    {
        fadeIn = false;
        yield return new WaitForSeconds(1);
        fadeOut = true;
    }
}