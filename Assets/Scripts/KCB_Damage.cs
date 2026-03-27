using UnityEngine;

public class KamehamehaDamage : MonoBehaviour
{
    public int damage = 50;
    public float knockbackForce = 5f;
    public bool useDamageOverTime = true;
    public ParticleSystem hitEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            ApplyDamage(other);
            ApplyKnockback(other);
        }

        if (other.CompareTag("Ground"))
        {
            Instantiate(hitEffect);
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (useDamageOverTime && other.CompareTag("Enemy"))
        {
            ApplyDamage(other);
        }
    }

    void ApplyDamage(Collider other)
    {
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            int finalDamage = Mathf.RoundToInt(damage * Time.deltaTime * 60f);
            enemy.TakeDamage(finalDamage);
        }
    }

    void ApplyKnockback(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 direction = -other.transform.forward;
            rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
        }
    }
}