using UnityEngine;

public class MeleeAttacks : MonoBehaviour
{

    public int M1Damage = 20;
    public float M1KnockbackForce = 5f;

    void Start()
    {
        
    }

    void Update()
    {

    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy") && Input.GetKeyDown(KeyCode.Mouse0))
        {
            ApplyDamage(other);
        }
    }

    void ApplyDamage(Collider other)
    {
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            int finalDamage = Mathf.RoundToInt(M1Damage);
            enemy.TakeDamage(finalDamage);
        }
    }
}
