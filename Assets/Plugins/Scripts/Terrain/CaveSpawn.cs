using System.Collections.Generic;
using UnityEngine;

public class CaveSpawn : MonoBehaviour
{
    #region Fields

    public List<GameObject> animals;

    #endregion Fields

    #region Methods

    private void Start()
    {
        if (animals != null && animals.Count != 0)
        {
            //AISpawner.Spawn(animals[Random.Range(0, animals.Count)], transform.Find("SpawnPoint").position);
        }
    }

    #endregion Methods
}