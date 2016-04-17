using UnityEditor;
using UnityEngine;

public class CreateSphere : ScriptableWizard
{
    public bool addCollider = false;

    public float Radius = 50f;

    public int lengthSegments = 1;

    public string optionalName;

    public int widthSegments = 1;

    [MenuItem("GameObject/Create Other/Custom Sphere...")]
    private static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard("Create Sphere", typeof(CreateSphere));
    }

    private void OnWizardCreate()
    {
        GameObject sphere = new GameObject();

        if (!string.IsNullOrEmpty(optionalName))
            sphere.name = optionalName;
        else
            sphere.name = "Sphere";

        string sphereAssetName = sphere.name + ".asset";
        Mesh m = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/Editor/" + sphereAssetName, typeof(Mesh));

        if (m == null)
        {
            MeshFilter filter = sphere.AddComponent<MeshFilter>();
            sphere.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            filter.mesh = mesh;
            mesh.Clear();
            // Longitude |||
            int nbLong = widthSegments;
            // Latitude ---
            int nbLat = lengthSegments;

            Vector3[] vertices = new Vector3[(nbLong + 1) * nbLat + 2];
            float _pi = Mathf.PI;
            float _2pi = _pi * 2f;

            vertices[0] = Vector3.up * Radius;
            for (int lat = 0; lat < nbLat; lat++)
            {
                float a1 = _pi * (float)(lat + 1) / (nbLat + 1);
                float sin1 = Mathf.Sin(a1);
                float cos1 = Mathf.Cos(a1);

                for (int lon = 0; lon <= nbLong; lon++)
                {
                    float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
                    float sin2 = Mathf.Sin(a2);
                    float cos2 = Mathf.Cos(a2);

                    vertices[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * Radius;
                }
            }
            vertices[vertices.Length - 1] = Vector3.up * -Radius;

            Vector3[] normales = new Vector3[vertices.Length];
            for (int n = 0; n < vertices.Length; n++)
                normales[n] = vertices[n].normalized;

            Vector2[] uvs = new Vector2[vertices.Length];
            uvs[0] = Vector2.up;
            uvs[uvs.Length - 1] = Vector2.zero;
            for (int lat = 0; lat < nbLat; lat++)
                for (int lon = 0; lon <= nbLong; lon++)
                    uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));

            int nbFaces = vertices.Length;
            int nbTriangles = nbFaces * 2;
            int nbIndexes = nbTriangles * 3;
            int[] triangles = new int[nbIndexes];

            //Top Cap
            int i = 0;
            for (int lon = 0; lon < nbLong; lon++)
            {
                triangles[i++] = lon + 2;
                triangles[i++] = lon + 1;
                triangles[i++] = 0;
            }

            //Middle
            for (int lat = 0; lat < nbLat - 1; lat++)
            {
                for (int lon = 0; lon < nbLong; lon++)
                {
                    int current = lon + lat * (nbLong + 1) + 1;
                    int next = current + nbLong + 1;

                    triangles[i++] = current;
                    triangles[i++] = current + 1;
                    triangles[i++] = next + 1;

                    triangles[i++] = current;
                    triangles[i++] = next + 1;
                    triangles[i++] = next;
                }
            }

            //Bottom Cap
            for (int lon = 0; lon < nbLong; lon++)
            {
                triangles[i++] = vertices.Length - 1;
                triangles[i++] = vertices.Length - (lon + 2) - 1;
                triangles[i++] = vertices.Length - (lon + 1) - 1;
            }

            mesh.vertices = vertices;
            mesh.normals = normales;
            mesh.uv = uvs;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();
            mesh.Optimize();

            AssetDatabase.CreateAsset(mesh, "Assets/Editor/" + sphereAssetName);
            AssetDatabase.SaveAssets();
        }

        if (addCollider)
        {
            SphereCollider col = (SphereCollider)sphere.AddComponent(typeof(SphereCollider));
            col.radius = Radius;
        }

        Selection.activeObject = sphere;
    }

    private void OnWizardUpdate()
    {
        widthSegments = Mathf.Clamp(widthSegments, 1, 254);
        lengthSegments = Mathf.Clamp(lengthSegments, 1, 254);
    }
}