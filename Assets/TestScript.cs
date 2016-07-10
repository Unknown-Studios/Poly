using UnityEngine;

public class TestScript : MonoBehaviour
{
    public string txt;
    public int hej;
    public float x;
    private float hej1;

    private int ReturnInt()
    {
        Debug.Log(txt);
        return hej;
    }

    // Use this for initialization
    private void Start()
    {
        Debug.Log(ReturnInt());
    }

    // Update is called once per frame
    private void Update()
    {
        float variabel = 4 / x;
        if (Time.timeSinceLevelLoad > 5)
        {
            if (variabel == 20)
            {
                Debug.Log("Din variabel er 20");
            }
            else
            {
                Debug.Log("Din variabel er " + variabel);
            }
        }
    }
}