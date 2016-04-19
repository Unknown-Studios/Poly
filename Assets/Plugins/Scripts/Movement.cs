using UnityEngine;
using UnityEngine.Networking;

public class Movement : NetworkBehaviour
{
    public float speed = 5.0f;
    public float sprintSpeed = 10.0f;
    public float jumpSpeed = 8.0f;
    public Vector3 moveDirection = Vector3.zero;

    private float slideAngle;

    private Animator anim;
    private Animator anim1;
    private int SpeedHash = Animator.StringToHash("Speed");
    private int SprintHash = Animator.StringToHash("Sprint");

    private bool isSliding;

    private float time;

    private float InitialHeight;

    private float speed1;

    private bool sprint;

    private float CurrentSpeed;

    private bool Swimming;

    private float curspeed;

    private bool cursprint;

    private Rigidbody body;

    private bool isGrounded;

    private void Start()
    {
        anim = transform.Find("Hands").GetComponent<Animator>();
        anim1 = transform.Find("Player").GetComponent<Animator>();
        body = GetComponent<Rigidbody>();

        InitialHeight = GetComponent<CapsuleCollider>().height;
    }

    private void OnCollisionEnter(Collision collisionInfo)
    {
        if (collisionInfo.collider.tag == "Chunk")
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collisionInfo)
    {
        isGrounded = false;
    }

    private void Update()
    {
        bool Crouch = Input.GetButtonDown("Crouch");
        if (isGrounded)
        {
            if (Crouch)
            {
                Debug.Log("Crouching");
                GetComponent<CapsuleCollider>().height = InitialHeight / 2;
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
        body.AddForce(moveDirection * Time.deltaTime);
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