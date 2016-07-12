using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODController : MonoBehaviour
{
    public int[] lodLevels;

    private List<Collider> lastColliders;

    public IEnumerator StartUpdater()
    {
        while (true)
        {
            UpdateMesh();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void Start()
    {
        lastColliders = new List<Collider>();
        StartCoroutine(StartUpdater());
    }

    private void UpdateLastColliders(Collider[] list)
    {
        List<Collider> List = new List<Collider>();
        for (int i = 0; i < list.Length; i++)
        {
            List.Add(list[i]);
        }

        for (int i = 0; i < lastColliders.Count; i++)
        {
            if (!List.Contains(lastColliders[i]))
            {
                if (lastColliders[i].GetComponent<LOD>())
                {
                    lastColliders[i].GetComponent<LOD>().SetTargetLOD(4);
                }
            }
        }
        lastColliders = List;
    }

    private void UpdateMesh()
    {
        if (lodLevels != null && lodLevels.Length != 0)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, lodLevels[lodLevels.Length - 1]);
            UpdateLastColliders(colliders);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].GetComponent<LOD>())
                {
                    colliders[i].GetComponent<LOD>().UpdateLODSettings(transform.position, lodLevels);
                }
            }
        }
    }
}