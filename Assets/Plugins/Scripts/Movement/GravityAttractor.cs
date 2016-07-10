using UnityEngine;

public class GravityAttractor : MonoBehaviour
{
    public float gravity = -9.81f;
    public float Range = 1000.0f;

    public LayerMask LM;

    public void Attract(Transform t)
    {
        Rigidbody body = t.GetComponent<Rigidbody>();
        body.useGravity = false;
        body.constraints = RigidbodyConstraints.FreezeRotation;

        Vector3 targetDir = (t.position - transform.position).normalized;
        Vector3 bodyUp = t.up;

        t.rotation = Quaternion.FromToRotation(bodyUp, targetDir) * t.rotation;

        body.AddForce(targetDir * gravity);
    }

    private void FixedUpdate()
    {
        Collider[] col = Physics.OverlapSphere(transform.position, Range, LM);
        foreach (Collider c in col)
        {
            if (c.tag == "Player")
            {
                Attract(c.transform);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, Range);
    }
}