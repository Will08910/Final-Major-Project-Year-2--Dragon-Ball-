using System.Collections;
using UnityEngine;

public class EnemyState : MonoBehaviour
{
    [HideInInspector]
    public bool isStunned = false;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Stun(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration)
    {
        isStunned = true;

        yield return new WaitForSeconds(duration);

        isStunned = false;

        // STOP leftover drifting
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (rb == null)
            return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(direction * force, ForceMode.Impulse);
    }
}