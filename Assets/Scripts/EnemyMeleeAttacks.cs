using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeAttacks : MonoBehaviour
{
    [Header("Damage / Knockback")]
    public int normalDamage = 10;
    public int heavyDamage = 25;
    public float heavyKnockback = 12f;

    [Header("Hitbox / Timing")]
    public Collider hitbox;
    public float attackDuration = 0.15f;
    public float attackCooldown = 0.15f;
    public float comboResetTime = 1.0f;

    [Header("Effects")]
    public ParticleSystem hitEffect;
    public ParticleSystem hitEffect2;

    [Header("Camera")]
    public Animator camShake;

    [Header("Combo Cooldown")]
    public float comboCooldown = 1.5f;

    private bool isAttacking = false;
    private bool comboOnCooldown = false;

    private HashSet<int> hitPlayers =
        new HashSet<int>();

    private int comboIndex = 0;
    private float lastAttackTime = -10f;
    private int currentAttackStep = -1;

    void Start()
    {
        if (hitbox == null)
            hitbox = GetComponent<Collider>();

        if (hitbox != null)
        {
            hitbox.isTrigger = true;
            hitbox.enabled = false;
        }
    }

    public void TriggerAttack()
    {
        if (comboOnCooldown)
            return;

        if (!isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    public IEnumerator FullCombo()
    {
        for (int i = 0; i < 4; i++)
        {
            TriggerAttack();

            yield return new WaitForSeconds(
                attackDuration +
                attackCooldown +
                0.05f
            );
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        hitPlayers.Clear();

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
        {
            comboIndex = 0;
        }

        isAttacking = false;

        currentAttackStep = -1;

        if (attackStep == 3)
        {
            StartCoroutine(ComboCooldownCoroutine());
        }
    }

    IEnumerator ComboCooldownCoroutine()
    {
        comboOnCooldown = true;

        yield return new WaitForSeconds(comboCooldown);

        comboOnCooldown = false;

        comboIndex = 0;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isAttacking)
            return;

        if (!other.CompareTag("Player"))
            return;

        int id = other.gameObject.GetInstanceID();

        if (hitPlayers.Contains(id))
            return;

        hitPlayers.Add(id);

        bool isHeavy = currentAttackStep == 3;

        if (isHeavy)
        {
            ApplyDamage(other, heavyDamage);
        }
        else
        {
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
            }
        }
    }
}