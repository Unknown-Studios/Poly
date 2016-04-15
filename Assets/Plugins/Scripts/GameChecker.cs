using UnityEngine;

public class GameChecker : MonoBehaviour
{
    public GameObject GameAObject;

    private void Awake()
    {
        if (!GameObject.Find("Game"))
        {
            GameObject game = Instantiate(GameAObject);
            game.name = "Game";
        }
        Destroy(this);
    }
}