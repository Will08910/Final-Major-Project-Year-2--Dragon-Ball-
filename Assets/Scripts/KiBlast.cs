using System.Collections;
using UnityEngine;

public class KiBlast : MonoBehaviour
{
    [HideInInspector] public float speed = 40f;
    [HideInInspector] public float homingStrength = 5f;
    [HideInInspector] public float lifetime = 8f;
    [HideInInspector] public float range = 100f;

    private Rigidbody rb;
    private Transform target;
    private float spawnTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        spawnTime = Time.time;
        AcquireTarget();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        if (rb.linearVelocity.sqrMagnitude == 0f)
            rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (target != null)
        {
            Vector3 toTarget = (target.position - transform.position).normalized;
            Vector3 currentVel = rb.linearVelocity.normalized;
            Vector3 newDir = Vector3.Slerp(currentVel, toTarget, homingStrength * Time.fixedDeltaTime).normalized;
            rb.linearVelocity = newDir * speed;
            transform.rotation = Quaternion.LookRotation(newDir);
        }
        else
        {
            // try reacquire target periodically
            if (Time.time - spawnTime < lifetime)
            {
                if (Random.value < 0.02f) // cheap periodic attempt
                    AcquireTarget();
            }
        }
    }

    void AcquireTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float bestDist = float.MaxValue;
        Transform best = null;
        foreach (var go in enemies)
        {
            if (go == null) continue;
            float d = Vector3.Distance(transform.position, go.transform.position);
            if (d < bestDist && d <= range)
            {
                bestDist = d;
                best = go.transform;
            }
        }
        target = best;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth eh = other.GetComponent<EnemyHealth>();
            if (eh != null)
                eh.TakeDamage(60);

            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
