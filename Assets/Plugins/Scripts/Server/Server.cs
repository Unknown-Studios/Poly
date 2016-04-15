using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Server : MonoBehaviour
{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

    private void Start()
    {
        string[] args = Environment.GetCommandLineArgs();
        foreach (string msg in args)
        {
            if (msg == "-Server")
            {
                SceneManager.LoadScene("Server");
            }
        }
    }

#endif
}