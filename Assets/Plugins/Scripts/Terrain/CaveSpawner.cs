using System.Collections.Generic;
using UnityEngine;

public class CaveSpawner : MonoBehaviour
{
    public int Amount;
    public List<GameObject> Caves;

    private void Start()
    {
        Terrain t = Terrain.activeTerrain;
        if (t == null || t.terrainData == null)
        {
            Debug.Log("Cannot spawn objects, when no terrain is existent.");
            return;
        }

        float minX = t.transform.position.x;
        float minY = t.transform.position.z;

        float maxX = minX + t.terrainData.size.x;
        float maxY = minY + t.terrainData.size.z;

        for (int i = 0; i < Amount; i++)
        {
            GameObject ob = (GameObject)Instantiate(Caves[Random.Range(0, Caves.Count)], new Vector3(Random.Range(minX, maxX), 0, Random.Range(minY, maxY)), Quaternion.identity);
            Vector3 bo = ob.transform.position;
            bo.y = t.terrainData.GetInterpolatedHeight(bo.x / t.terrainData.size.x, bo.z / t.terrainData.size.z);
            ob.transform.position = bo;
        }
    }
}