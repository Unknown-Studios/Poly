using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    public float speed = 5.0f;
    public float sprintSpeed = 10.0f;
    public float jumpSpeed = 8.0f;

    public GameObject planet;

    private Vector3 moveDirection = Vector3.zero;
    private bool isGrounded;
    private float slideAngle;

    private bool Swimming;

    private Rigidbody body;
    private GravityAttractor Attractor;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.constraints = RigidbodyConstraints.FreezeRotation;

        Attractor = planet.GetComponent<GravityAttractor>();
    }

    private void FixedUpdate()
    {
        Attractor.Attract(transform);

        //Movement
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (Input.GetButtonDown("Sprint"))
        {
            moveDirection *= sprintSpeed;
        }
        else
        {
            moveDirection *= speed;
        }

        //Apply
        body.MovePosition(body.position + transform.TransformDirection(moveDirection) * Time.fixedDeltaTime);

        //Gravity affected operations
        if (isGrounded)
        {
            if (Input.GetButton("Jump"))
            {
                body.AddForce(jumpSpeed * transform.up, ForceMode.Impulse);
            }
        }

        isGrounded = false;
    }

    private void OnCollisionStay(Collision collisionInfo)
    {
        if (collisionInfo.collider.tag == "Chunk")
        {
            isGrounded = true;
        }
    }
}