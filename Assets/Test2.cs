using UnityEngine;

public class Test2 : MonoBehaviour
{
    private ProceduralSphere PS;
    private bool Spawned;

    private void Start()
    {
        PS = FindObjectOfType<ProceduralSphere>();
    }

    private void Update()
    {
        if (PS != null)
        {
            if (!Spawned)
            {
                if (PS.isDone)
                {
                    Spawned = true;
                    gameObject.transform.position = PS.GetHeight(Random.onUnitSphere);
                }
            }
        }
    }
}