using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterController))]
public class Movement : NetworkBehaviour
{
    public float speed = 5.0f;
    public float sprintSpeed = 10.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Vector3 moveDirection = Vector3.zero;

    private float slideAngle;

    private Animator anim;
    private Animator anim1;
    private int SpeedHash = Animator.StringToHash("Speed");
    private int SprintHash = Animator.StringToHash("Sprint");
    private int CrouchHash = Animator.StringToHash("Crouch");

    private bool isSliding;

    private int terrainHeight;
    private float waterHeight;
    private ProceduralTerrain PT;
    private CharacterController controller;

    private float time;

    private float InitialHeight;

    private float speed1;

    private bool sprint;

    private float CurrentSpeed;

    private bool Swimming;

    private float curspeed;

    private bool cursprint;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        PT = GameObject.Find("GameController").GetComponent<ProceduralTerrain>();
        terrainHeight = PT.terrainHeight;
        waterHeight = PT.WaterLevel * 1f;
        anim = transform.Find("Hands").GetComponent<Animator>();
        anim1 = transform.Find("Player").GetComponent<Animator>();
        InvokeRepeating("TooLow", 0, 1.0f);

        InitialHeight = controller.height;
    }

    private void Update()
    {
        moveDirection.y = Mathf.Clamp(moveDirection.y, -9.81f, 9.81f);
        if (Swimming)
        {
            transform.position = new Vector3(transform.position.x, Mathf.Max(waterHeight - 1, transform.position.y), transform.position.z);
        }
        bool Crouch = Input.GetButtonDown("Crouch");
        if (controller.isGrounded || Swimming)
        {
            if (Crouch)
            {
                Debug.Log("Crouching");
                controller.height = InitialHeight / 2;
            }
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            if (moveDirection.x != 0 || moveDirection.z != 0)
            {
                speed1 = Mathf.MoveTowards(speed1, CurrentSpeed, Time.deltaTime);
                anim.SetFloat(SpeedHash, speed1);

                sprint = Input.GetButton("Sprint");
                anim.SetBool(SprintHash, sprint);
                if (sprint)
                {
                    CurrentSpeed = 2;
                    moveDirection *= sprintSpeed;
                }
                else
                {
                    CurrentSpeed = 1;
                    moveDirection *= speed;
                }
            }
            else
            {
                speed1 = 0;
                sprint = false;
                anim.SetBool(SprintHash, sprint);
                anim.SetFloat(SpeedHash, speed1);
            }

            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
            }
        }
        if (transform.position.y <= waterHeight - 1)
        {
            moveDirection.x /= 2;
            moveDirection.z /= 2;
            Swimming = true;
        }
        else if (Crouch)
        {
            moveDirection.x /= 2;
            moveDirection.z /= 2;
            Swimming = false;
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime;
            Swimming = false;
        }
        controller.Move(moveDirection * Time.deltaTime);
    }

    private void TooLow()
    {
        if (transform.position.y <= waterHeight)
        {
            Vector3 hitPos = transform.position;
            hitPos.y = terrainHeight;
            RaycastHit hit;
            Vector3 startPos = transform.position;
            startPos.y += 3.1f;
            if (Physics.Linecast(startPos, hitPos, out hit))
            {
                if (hit.collider)
                {
                    if (hit.collider.tag != "Player" && hit.collider.tag != "Water")
                    {
                        if (hit.collider.tag == "Chunk")
                        {
                            transform.position = new Vector3(transform.position.x, hit.point.y - 1, transform.position.z); ;
                            return;
                        }
                    }
                }
            }
            transform.position = new Vector3(transform.position.x, waterHeight, transform.position.z); ;
        }
    }

    [Command]
    private void CmdSyncState(string PlayerID, float speed2, bool sprint1)
    {
        if (anim1)
        {
            anim1.SetFloat(SpeedHash, speed2);
            anim1.SetBool(SprintHash, sprint1);
        }
    }
}