using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttacks : MonoBehaviour
{
    [Header("Damage / Knockback")]
    public int normalDamage = 10;
    public int heavyDamage = 25;
    public float heavyKnockback = 12f;
    public float stunDuration = 1.0f;

    [Header("Hitbox / Timing")]
    public Collider hitbox;
    public float attackDuration = 0.15f;
    public float attackCooldown = 0.15f;
    public float comboResetTime = 1.0f; 
    public ParticleSystem hitEffect;
    public ParticleSystem hitEffect2;

    [Header("Camera")]
    public Animator camShake;

    [Header("Combo Cooldown")]
    public float comboCooldown = 1.5f;

    private bool isAttacking = false;
    private bool comboOnCooldown = false;
    private HashSet<int> hitEnemies = new HashSet<int>();
    private int comboIndex = 0;
    private float lastAttackTime = -10f;
    private int currentAttackStep = -1;

    private Dictionary<int, Coroutine> stunCoroutines = new Dictionary<int, Coroutine>();
    private Dictionary<int, RigidbodyConstraints> originalConstraints = new Dictionary<int, RigidbodyConstraints>();
    private Dictionary<int, bool> originalKinematic = new Dictionary<int, bool>();
    private Dictionary<int, List<MonoBehaviour>> disabledBehaviours = new Dictionary<int, List<MonoBehaviour>>();
    private Dictionary<int, bool> disabledCharacterController = new Dictionary<int, bool>();

    void Start()
    {
        if (hitbox == null)
            hitbox = GetComponent<Collider>();

        if (hitbox != null)
        {
            hitbox.isTrigger = true;
            hitbox.enabled = false;
        }
        else
        {
            Debug.LogWarning("MeleeAttacks: No hitbox Collider assigned or found. Assign a trigger collider for the weapon.");
        }
    }

    void Update()
    {
        if (Time.time - lastAttackTime > comboResetTime)
            comboIndex = 0;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (comboOnCooldown)
                return;

            if (!isAttacking)
            {
                StartCoroutine(AttackRoutine());
            }
            else
            {
                lastAttackTime = Time.time;
            }
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        hitEnemies.Clear();

        int attackStep = comboIndex;
        currentAttackStep = attackStep;

        if (hitbox != null)
            hitbox.enabled = true;

        lastAttackTime = Time.time;

        yield return new WaitForSeconds(attackDuration);

        if (hitbox != null)
            hitbox.enabled = false;

        yield return new WaitForSeconds(attackCooldown);

        comboIndex = (attackStep + 1) % 4;

        if (Time.time - lastAttackTime > comboResetTime)
            comboIndex = 0;

        isAttacking = false;
        currentAttackStep = -1;

        if (attackStep == 3)
            StartCoroutine(ComboCooldownCoroutine());
    }

    private IEnumerator ComboCooldownCoroutine()
    {
        comboOnCooldown = true;
        yield return new WaitForSeconds(comboCooldown);
        comboOnCooldown = false;
        comboIndex = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isAttacking)
            return;

        if (!other.CompareTag("Enemy"))
            return;

        int id = other.gameObject.GetInstanceID();
        if (hitEnemies.Contains(id))
            return;

        hitEnemies.Add(id);

        bool isHeavy = currentAttackStep == 3;

        if (isHeavy)
        {
            ApplyDamage(other, heavyDamage);
        }
        else
        {
            Rigidbody rb = other.attachedRigidbody ?? other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            ApplyDamage(other, normalDamage);
        }

        if (hitEffect != null)
            hitEffect.Emit(1);

        if (hitEffect2 != null)
            hitEffect2.Emit(20);

        if (camShake != null)
            camShake.SetTrigger("Shake");
    }

    void ApplyDamage(Collider other, int amount)
    {
        EnemyHealth player =
            other.GetComponent<EnemyHealth>();

        if (player != null)
        {
            player.TakeDamage(amount);
        }

        EnemyState state =
            other.GetComponent<EnemyState>();

        if (state != null)
        {
            bool heavyAttack = currentAttackStep == 3;

            if (heavyAttack)
            {
                state.Stun(1.2f);

                Vector3 dir =
                    (other.transform.position - transform.position)
                    .normalized;

                state.ApplyKnockback(dir, heavyKnockback);
            }
            else
            {
                state.Stun(0.35f);

                Vector3 dir =
                    (other.transform.position - transform.position)
                    .normalized;

            }
        }
    }

    void ApplyKnockback(Collider other, float force)
    {
        Rigidbody rb = other.attachedRigidbody ?? other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (other.transform.position - transform.position).normalized;
            rb.AddForce(direction * force, ForceMode.Impulse);

            EnemyState state = other.GetComponent<EnemyState>();

            if (state != null)
            {
                state.Stun(1.2f);
            }
        }
    }

    public void TriggerAttack()
    {
        if (!isAttacking && !comboOnCooldown)
        {
            StartCoroutine(AttackRoutine());
        }
    }
}