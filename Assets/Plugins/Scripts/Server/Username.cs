using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(GUIText))]
public class Username : NetworkBehaviour
{
    public int TextDistance = 30;
    private GameObject me;
    private GameObject player;
    private GUIText UsernameText;

    private void Start()
    {
        tag = "Player";
        player = transform.root.gameObject;
        UsernameText = GetComponent<GUIText>();
        if (isLocalPlayer)
        {
            name = PlayerPrefs.GetString("UserID");
            Game.player = player;

            enabled = false;
            UsernameText.enabled = false;
        }
        me = GameObject.Find(PlayerPrefs.GetString("UserID"));
    }

    private void Update()
    {
        if (me != null && player != null)
        {
            var distance = Vector3.Distance(player.transform.position, me.transform.position);
            if (distance < TextDistance)
            {
                UsernameText.text = player.name;
                var textpos = player.transform.position;
                textpos.y += 2;
                UsernameText.fontSize = Mathf.RoundToInt(distance / TextDistance) * 10;
                UsernameText.transform.position = player.GetComponentInChildren<Camera>().WorldToViewportPoint(textpos);
            }
        }
    }
}