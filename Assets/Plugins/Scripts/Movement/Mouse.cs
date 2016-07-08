using UnityEngine;

public class Mouse : MonoBehaviour
{
    public float rotationX;
    public float rotationY;
    private GameObject Cam;
    private float[] distances = new float[32];
    private Transform Hands;

    private void Start()
    {
        Cam = transform.Find("Camera").gameObject;
        if (transform.root.Find("Hands"))
            Hands = transform.root.Find("Hands").Find("Armature");
    }

    private void Update()
    {
        if (distances[12] != PlayerPrefs.GetInt("ViewDistance"))
        {
            distances[12] = PlayerPrefs.GetInt("ViewDistance");
            GetComponent<Camera>().layerCullDistances = distances;
        }

        transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0) * Time.deltaTime * 400f);
        rotationY += Input.GetAxis("Mouse Y") * Time.deltaTime * 400f;
        rotationY = Mathf.Clamp(rotationY, -40.0f, 60.0f);
        Cam.transform.localEulerAngles = new Vector3(-rotationY, 0, 0);
    }
}