using UnityEngine;

public class KamehamehaDamage : MonoBehaviour
{
    public int damage = 50;

    public float knockbackForce = 40f;

    public float stunDuration = 2f;

    public bool useDamageOverTime = true;

    public ParticleSystem hitEffect;
    public ParticleSystem kiHitEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            ApplyKnockbackAndStun(other);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (useDamageOverTime && other.CompareTag("Enemy"))
        {
            ApplyDamage(other);
        }
    }

    // =========================================================
    // DAMAGE
    // =========================================================

    void ApplyDamage(Collider other)
    {
        EnemyHealth enemy =
            other.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            int finalDamage =
                Mathf.RoundToInt(
                    damage * Time.deltaTime * 60f
                );

            enemy.TakeDamage(finalDamage);
        }
    }

    // =========================================================
    // STUN + KNOCKBACK
    // =========================================================

    void ApplyKnockbackAndStun(Collider other)
    {
        EnemyState state =
            other.GetComponent<EnemyState>();

        if (state != null)
        {
            state.Stun(stunDuration);

            Vector3 dir =
                transform.forward.normalized;

            state.ApplyKnockback(
                dir,
                knockbackForce
            );
        }
    }
}