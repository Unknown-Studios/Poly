using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreeGenerator : MonoBehaviour
{
    public GameObject Quad;

    public Billboard.BillboardClass[] Trees;

    private bool isDone = false;

    public IEnumerator GenerateBillboard()
    {
        foreach (Billboard.BillboardClass b in Trees)
        {
            GameObject Cam = new GameObject();
            Camera cam = Cam.AddComponent<Camera>();

            int imageHeight = Screen.height / 2;
            int imageWidth = Mathf.RoundToInt(imageHeight * 0.625f);

            //grab the main camera and mess with it for rendering the object - make sure orthographic
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;

            //render to screen rect area equal to out image size
            float rw = imageWidth; rw /= Screen.width;
            float rh = imageHeight; rh /= Screen.height;
            cam.rect = new Rect(0, 0, rw, rh);

            GameObject T = GameObject.Instantiate(b.tree);

            //grab size of object to render - place/size camera to fit
            Bounds bb = T.GetComponent<Renderer>().bounds;

            //place camera looking at centre of object - and backwards down the z-axis from it
            Vector3 Pos = bb.center;
            Pos = bb.center;
            Pos.z = -1.0f + (bb.min.z * 2.0f);
            cam.transform.position = Pos;
            //make clip planes fairly optimal and enclose whole mesh
            cam.nearClipPlane = 0.5f;
            cam.farClipPlane = -cam.transform.position.z + 10.0f + bb.max.z;
            cam.transform.position = Pos;
            //set camera size to just cover entire mesh
            cam.orthographicSize = 1.01f * Mathf.Max((bb.max.y - bb.min.y) / 2.0f, (bb.max.x - bb.min.x) / 2.0f);
            Pos.y += cam.orthographicSize * 0.05f;
            cam.transform.position = Pos;

            //render
            yield return new WaitForEndOfFrame();

            Texture2D tex = new Texture2D(imageWidth, imageHeight, TextureFormat.ARGB32, false);
            // Read screen contents into the texture
            tex.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();

            //turn all pixels == background-color to transparent
            Color bCol = cam.backgroundColor;
            for (int y = 0; y < imageHeight; y++)
            {
                for (int x = 0; x < imageWidth; x++)
                {
                    if (tex.GetPixel(x, y).r == bCol.r)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            b.billboard = tex;
            b.UpdateSize();
            //Clean up
            GameObject.Destroy(T);
            GameObject.Destroy(Cam);
        }
        isDone = true;
    }

    private IEnumerator GenerateTrees()
    {
        while (!isDone)
        {
            yield return null;
        }
        Terrain t = Terrain.activeTerrain;
        int tx = Mathf.RoundToInt(t.terrainData.size.x);
        int tz = Mathf.RoundToInt(t.terrainData.size.z);
        for (int x = 0; x < tx; x += 2)
        {
            for (int y = 0; y < tz; y += 2)
            {
                if (Random.Range(0, 250) == 1)
                {
                    int TreeIndex = Random.Range(0, Trees.Length);
                    GameObject tree = GameObject.Instantiate(Trees[TreeIndex].tree, new Vector3(x, t.terrainData.GetInterpolatedHeight(x / tx, y / tz), y), Quaternion.identity) as GameObject;
                    Billboard component = tree.AddComponent<Billboard>();
                    component.tree = Trees[TreeIndex];
                    component.quad = Quad;
                }
            }
            if (x % 100 == 0)
            {
                yield return null;
            }
        }
        yield break;
    }

    // Use this for initialization
    private void Start()
    {
        if (Camera.main == null)
        {
            GameObject CamObject = new GameObject();
            CamObject.name = "Camera";
            CamObject.tag = "MainCamera";
            CamObject.AddComponent<Camera>();
        }
        StartCoroutine(GenerateBillboard());
        StartCoroutine(GenerateTrees());
    }
}