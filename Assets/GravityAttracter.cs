using UnityEngine;

public class GravityAttracter : MonoBehaviour
{
    public float gravity = 9.81f;
    public float Range;

    public void Attract(Transform t)
    {
        Vector3 targetDir = (t.position - transform.position).normalized;
        Vector3 bodyUp = t.up;

        t.rotation = Quaternion.FromToRotation(bodyUp, targetDir) * t.rotation;
        t.GetComponent<Rigidbody>().AddForce(targetDir * -gravity);
    }

    private void FixedUpdate()
    {
        Collider[] col = Physics.OverlapSphere(transform.position, Range);
        foreach (Collider c in col)
        {
            if (c.GetComponent<Rigidbody>() != null)
            {
                Rigidbody body = c.GetComponent<Rigidbody>();
                body.useGravity = false;
                body.constraints = RigidbodyConstraints.FreezeRotation;
            }
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