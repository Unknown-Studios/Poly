using UnityEngine;

public class GravityAttracter : MonoBehaviour
{
    public float gravity = 9.81f;
    public float Range = 1000.0f;

    public LayerMask LM;

    public void Attract(Transform t)
    {
        Vector3 targetDir = (t.position - transform.position).normalized;
        Vector3 bodyUp = t.up;

        t.rotation = Quaternion.FromToRotation(bodyUp, targetDir) * t.rotation;

        t.GetComponent<Rigidbody>().AddForce(targetDir * -gravity);
    }

    private void FixedUpdate()
    {
        Collider[] col = Physics.OverlapSphere(transform.position, Range, LM);
        foreach (Collider c in col)
        {
            if (c.tag == "Player")
            {
                if (c.gameObject.GetComponent<Rigidbody>() != null)
                {
                    Rigidbody body = c.GetComponent<Rigidbody>();
                    body.useGravity = false;
                    body.constraints = RigidbodyConstraints.FreezeRotation;
                }
                Attract(c.transform);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, Range);
    }
}