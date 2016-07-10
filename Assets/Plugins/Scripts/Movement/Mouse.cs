using UnityEngine;

public class Mouse : MonoBehaviour
{
    public float rotationX;
    public float rotationY;
    public float bobSpeed = 3.0f;
    private GameObject Cam;
    private float[] distances = new float[32];
    private Rigidbody body;
    private float timer = 0.0f;

    private void Start()
    {
        Cam = transform.Find("Camera").gameObject;
        body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (distances[12] != PlayerPrefs.GetInt("ViewDistance"))
        {
            distances[12] = PlayerPrefs.GetInt("ViewDistance");
            Cam.GetComponent<Camera>().layerCullDistances = distances;
        }

        transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0) * Time.deltaTime * 400f);
        rotationY += Input.GetAxis("Mouse Y") * Time.deltaTime * 400f;
        rotationY = Mathf.Clamp(rotationY, -40.0f, 60.0f);
        Cam.transform.localEulerAngles = new Vector3(-rotationY, 0, 0);

        if (body.velocity != Vector3.zero)
        {
            timer += Time.deltaTime * bobSpeed;
            Cam.transform.localPosition = new Vector3(Mathf.Cos(timer) * 0.5f, Mathf.Abs(Mathf.Sin(timer)) * 0.5f, 0);
        }
        else
        {
            Vector3 camPos = Cam.transform.localPosition;
            timer = Mathf.PI / 2.0f;
            float x = Mathf.Cos(timer) * 0.5f;
            float y = Mathf.Abs(Mathf.Sin(timer) * 0.5f);
            if (Mathf.Abs(camPos.x - x) > 0.05f)
            {
                x = Mathf.Lerp(camPos.x, Mathf.Cos(timer) * 0.5f, Time.deltaTime);
            }
            if (Mathf.Abs(camPos.y - y) > 0.05f)
            {
                y = Mathf.Lerp(camPos.y, Mathf.Abs(Mathf.Sin(timer)) * 0.5f, Time.deltaTime);
            }

            Vector3 newPosition = new Vector3(x, y, 0);
            Cam.transform.localPosition = newPosition;
        }
    }
}