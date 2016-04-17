using System.Collections;
using UnityEngine;

public class LODController : MonoBehaviour
{
    public int[] lodLevels;

    public LayerMask layerMask;

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
        StartCoroutine(StartUpdater());
    }

    private void UpdateMesh()
    {
        if (lodLevels != null && lodLevels.Length != 0)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, lodLevels[lodLevels.Length - 1]);
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