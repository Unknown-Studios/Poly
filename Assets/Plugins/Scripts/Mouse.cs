using UnityEngine;

public class Mouse : MonoBehaviour
{
    private float[] distances = new float[32];
    private float h;
    private Transform Hands;
    private float maximumY = 100f;
    private float minimumY = -60f;
    private GameObject player;
    private ProceduralTerrain PT;
    private float v;

    private void Start()
    {
        player = transform.root.gameObject;
        PT = GameObject.Find("GameController").GetComponent<ProceduralTerrain>();
        Hands = transform.root.Find("Hands").Find("Armature");
    }

    private void Update()
    {
        if (distances[12] != PlayerPrefs.GetInt("ViewDistance"))
        {
            distances[12] = PlayerPrefs.GetInt("ViewDistance");
            GetComponent<Camera>().layerCullDistances = distances;
        }
        if (Input.GetAxis("Mouse X") != 0.0f)
        {
            h += PlayerPrefs.GetFloat("Sensitivity X") * Input.GetAxis("Mouse X");
            player.transform.localEulerAngles = new Vector3(0, h, 0);
        }
        if (Input.GetAxis("Mouse Y") != 0.0f)
        {
            float mY = 0f;
            if (player.transform.position.y < PT.WaterLevel)
            {
                mY = -20f;
            }
            else
            {
                mY = minimumY;
            }

            v += PlayerPrefs.GetFloat("Sensitivity Y") * Input.GetAxis("Mouse Y");

            v = Mathf.Clamp(v, mY, maximumY);
            Hands.localEulerAngles = new Vector3((-v) - 90, 0, 0);
        }
    }
}