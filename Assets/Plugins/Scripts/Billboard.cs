using System;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public GameObject quad;
    public BillboardClass tree;
    private Renderer QuadRenderer;

    private void CreateQuad()
    {
        GameObject Quad = GameObject.CreatePrimitive(PrimitiveType.Quad); //Create quad (Used to display billboard)
        Destroy(Quad.GetComponent<MeshCollider>());

        Bounds bb = tree.tree.GetComponent<Renderer>().bounds;
        Quad.transform.parent = transform; //Make parent of the tree
        Quad.transform.localPosition = new Vector3(0, bb.max.y / 2, 0); //Reset position

        Mesh mesh = Quad.GetComponent<MeshFilter>().mesh;

        mesh.vertices = new Vector3[]
        {
            new Vector3( tree.Scale.x, tree.Scale.y, 0),
            new Vector3( tree.Scale.x, 0, 0),
            new Vector3(0, tree.Scale.y, 0),
            new Vector3(0, 0, 0),
        };

        Quad.GetComponent<MeshFilter>().mesh = mesh;
        Quad.GetComponent<Renderer>().material = tree.mat;
        Quad.GetComponent<Renderer>().material.mainTexture = tree.billboard;
    }

    private void Start()
    {
        if (Game.noGraphics)
        {
            enabled = false;
            return;
        }

        gameObject.layer = 12;
        CreateQuad();
    }

    [Serializable]
    public class BillboardClass
    {
        public Texture2D billboard;
        public Material mat;

        [HideInInspector]
        public Vector3 Scale;

        public GameObject tree;

        public void UpdateSize()
        {
            Bounds bb = tree.GetComponent<Renderer>().bounds;
            Scale = new Vector3(bb.max.x - bb.min.x, bb.max.y - bb.min.y, 1);
        }
    }
}