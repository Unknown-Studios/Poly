using UnityEngine;

public class ProceduralTree : MonoBehaviour
{
    public int ringVertices = 6;
    public int height = 3;
    public float Radius = 2.0f;

    public GameObject tmPrefab;

    private Vector3[] drawArray;

    // Use this for initialization
    private void Start()
    {
        GenerateTree();

        int op = 0;
        foreach (Vector3 pos in drawArray)
        {
            GameObject gm = GameObject.Instantiate(tmPrefab, pos, Quaternion.identity) as GameObject;
            TextMesh tm = gm.GetComponent<TextMesh>();
            if (tm != null)
            {
                tm.name = "Mesh: " + op.ToString();
                tm.text = op.ToString();
                tm.transform.parent = transform;
            }
            op++;
        }
    }

    private GameObject GenerateTree()
    {
        //Initialization
        GameObject tree = new GameObject();
        MeshFilter mft = tree.AddComponent<MeshFilter>();
        tree.AddComponent<MeshRenderer>();
        MeshCollider collider = tree.AddComponent<MeshCollider>();
        Mesh treeMesh = mft.mesh = new Mesh();

        Vector3[] vertices = new Vector3[ringVertices * height];
        int[] triangles = new int[vertices.Length * 6];

        //Vertices
        for (int h = 0; h < height; h++)
        {
            for (int v = 0; v < ringVertices; v++)
            {
                int index = (h * ringVertices) + v;
                float rad = (float)v / ringVertices * (2 * Mathf.PI);
                vertices[index] = new Vector3(Mathf.Cos(rad) * Radius, h, Mathf.Sin(rad) * Radius);
            }
        }

        drawArray = vertices;

        //Triangles
        int i = 0;
        for (int c = 0; c <= vertices.Length - ringVertices; c++)
        {
            Debug.Log(c);
            triangles[i++] = c;
            triangles[i++] = ringVertices;
            triangles[i++] = ringVertices + 1;

            triangles[i++] = c;
            triangles[i++] = ringVertices + 1;
            triangles[i++] = c + 1;
        }

        //Finish up and return
        treeMesh.vertices = vertices;
        treeMesh.triangles = triangles;

        treeMesh.RecalculateNormals();
        treeMesh.RecalculateBounds();
        treeMesh.Optimize();
        mft.mesh = treeMesh;
        if (collider.sharedMesh != null)
        {
            collider.sharedMesh.Clear();
        }
        collider.sharedMesh = treeMesh;

        return tree;
    }

    private void OnDrawGizmos()
    {
        if (drawArray != null)
        {
            for (int i = 0; i < drawArray.Length; i++)
            {
                Gizmos.DrawSphere(drawArray[i], 0.1f);
            }
        }
    }
}