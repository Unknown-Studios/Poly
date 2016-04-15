using UnityEngine;
using UnityEngine.Networking;

public class Sync : NetworkBehaviour
{
    public static Sync _instance;

    [SyncVar]
    public float _TimeOfDay;

    [SyncVar]
    public float _WindSpeed;

    [SyncVar]
    public Vector2 _Wind;

    public static float TimeOfDay
    {
        get
        {
            return instance._TimeOfDay;
        }
        set
        {
            instance._TimeOfDay = value;
        }
    }

    public static float WindSpeed
    {
        get
        {
            return instance._WindSpeed;
        }
        set
        {
            instance._WindSpeed = value;
        }
    }

    public static Vector2 Wind
    {
        get
        {
            return instance._Wind;
        }
        set
        {
            instance._Wind = value;
        }
    }

    public static Sync instance
    {
        get
        {
            return _instance;
        }
    }

    public void Awake()
    {
        if (instance != this)
        {
            Destroy(this);
        }
        DontDestroyOnLoad(this);
    }
}