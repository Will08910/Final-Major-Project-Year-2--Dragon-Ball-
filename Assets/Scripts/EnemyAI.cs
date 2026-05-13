using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    public enum AIState
    {
        Idle,
        Chase,
        Retreat,
        ChargeKi,
        Melee,
        Wander
    }

    [Header("Target")]
    public Transform player;

    [Header("References")]
    public Rigidbody rb;
    public EnemyAttacks attacks;
    public EnemyMeleeAttacks melee;
    public EnemyHealth health;
    public EnemyState enemyState;
    public Slider healthSlider;

    [Header("Movement")]
    public float moveSpeed = 20f;
    public float boostSpeed = 35f;
    public float preferredDistance = 15f;
    public float meleeDistance = 4f;
    public float retreatDistance = 2f;
    public float minimumDistanceFromPlayer = 1.5f;

    [Header("Movement Randomness")]
    public float wanderSpeed = 18f;
    public float verticalMovementAmount = 6f;

    [Header("Decision Making")]
    public float minDecisionRate = 0.15f;
    public float maxDecisionRate = 1f;

    [Header("Transformation")]
    public bool canTransform = true;
    public float transformHealthThreshold = 0.4f;

    private AIState currentState;

    private float decisionTimer;

    private bool isComboAttacking = false;

    private bool aggressiveMode = true;

    private float personalityTimer;

    private Vector3 wanderDirection;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (attacks == null)
            attacks = GetComponent<EnemyAttacks>();

        if (melee == null)
            melee = GetComponent<EnemyMeleeAttacks>();

        if (health == null)
            health = GetComponent<EnemyHealth>();

        if (enemyState == null)
            enemyState = GetComponent<EnemyState>();

        if (player == null)
        {
            GameObject p =
                GameObject.FindGameObjectWithTag("Player");

            if (p != null)
                player = p.transform;
        }

        PickNewPersonality();
        PickNewWanderDirection();
    }

    void Update()
    {
        if (player == null)
            return;

        if (enemyState != null && enemyState.isStunned)
            return;

        if (attacks.isFiring)
        {
            StopMovement();
            return;
        }

        FacePlayer();

        HandleTransformation();

        personalityTimer -= Time.deltaTime;

        if (personalityTimer <= 0f)
        {
            PickNewPersonality();
        }

        decisionTimer -= Time.deltaTime;

        if (decisionTimer <= 0f)
        {
            MakeDecision();

            decisionTimer =
                Random.Range(
                    minDecisionRate,
                    maxDecisionRate
                );
        }

        HandleState();
    }

    void FacePlayer()
    {
        Vector3 dir =
            (player.position - transform.position).normalized;

        if (dir != Vector3.zero)
        {
            Quaternion targetRot =
                Quaternion.LookRotation(dir);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    Time.deltaTime * 8f
                );
        }
    }

    void PickNewPersonality()
    {
        aggressiveMode = Random.value > 0.4f;

        personalityTimer =
            Random.Range(3f, 8f);
    }

    void MakeDecision()
    {
        if (isComboAttacking)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        float random = Random.value;

        if (attacks.currentKi < attacks.maxKi * 0.15f)
        {
            currentState = AIState.ChargeKi;
            return;
        }

        if (distance < retreatDistance)
        {
            if (random < 0.5f)
            {
                currentState = AIState.Retreat;
            }
            else
            {
                currentState = AIState.Melee;
            }

            return;
        }

        if (distance <= meleeDistance)
        {
            if (aggressiveMode)
            {
                if (random < 0.65f)
                {
                    currentState = AIState.Melee;
                    return;
                }

                if (random < 0.9f)
                {
                    attacks.UseMeteorStrike();
                    return;
                }

                currentState = AIState.Wander;
                return;
            }
            else
            {
                if (random < 0.5f)
                {
                    currentState = AIState.Retreat;
                    return;
                }

                if (random < 0.7f)
                {
                    attacks.FireKiBlast();
                    currentState = AIState.Wander;
                    return;
                }

                currentState = AIState.Melee;
                return;
            }
        }

        if (distance <= preferredDistance)
        {
            if (aggressiveMode)
            {
                if (
                    random < 0.06f &&
                    attacks.currentKi >= 2000
                )
                {
                    attacks.UseKamehameha();
                    return;
                }

                if (random < 0.45f)
                {
                    currentState = AIState.Chase;
                    return;
                }

                if (random < 0.65f)
                {
                    attacks.FireKiBlast();
                    currentState = AIState.Chase;
                    return;
                }

                if (random < 0.8f)
                {
                    StartCoroutine(KiBlastBurst());
                    currentState = AIState.Wander;
                    return;
                }

                currentState = AIState.Wander;
                return;
            }
            else
            {
                if (
                    random < 0.06f &&
                    attacks.currentKi >= 2000
                )
                {
                    attacks.UseKamehameha();
                    return;
                }

                if (random < 0.4f)
                {
                    attacks.FireKiBlast();
                    currentState = AIState.Wander;
                    return;
                }

                if (random < 0.65f)
                {
                    StartCoroutine(KiBlastBurst());
                    currentState = AIState.Retreat;
                    return;
                }

                if (random < 0.8f)
                {
                    currentState = AIState.Retreat;
                    return;
                }

                currentState = AIState.Chase;
                return;
            }
        }

        if (distance > preferredDistance)
        {
            if (random < 0.35f)
            {
                attacks.FireKiBlast();
            }

            if (random < 0.55f)
            {
                StartCoroutine(KiBlastBurst());
            }

            if (random < 0.06f)
            {
                attacks.UseKamehameha();
                return;
            }

            currentState = AIState.Chase;
        }
    }

    void HandleState()
    {
        switch (currentState)
        {
            case AIState.Chase:
                ChasePlayer();
                break;

            case AIState.Retreat:
                Retreat();
                break;

            case AIState.ChargeKi:
                ChargeKi();
                break;

            case AIState.Melee:
                MeleeAttack();
                break;

            case AIState.Wander:
                WanderMovement();
                break;
        }
    }

    void ChasePlayer()
    {
        Vector3 targetPos =
            player.position +
            Random.insideUnitSphere * 3f;

        Vector3 toPlayer =
            targetPos - transform.position;

        float distance = toPlayer.magnitude;

        if (distance <= minimumDistanceFromPlayer)
        {
            StopMovement();
            return;
        }

        Vector3 dir = toPlayer.normalized;

        rb.linearVelocity = Vector3.Lerp(
            rb.linearVelocity,
            dir * boostSpeed,
            Time.deltaTime * 6f
        );
    }

    void Retreat()
    {
        Vector3 dir =
            (transform.position - player.position).normalized;

        dir += Random.insideUnitSphere * 0.4f;

        rb.linearVelocity = Vector3.Lerp(
            rb.linearVelocity,
            dir.normalized * moveSpeed,
            Time.deltaTime * 5f
        );
    }

    void WanderMovement()
    {
        if (Random.value < 0.03f)
        {
            PickNewWanderDirection();
        }

        rb.linearVelocity = Vector3.Lerp(
            rb.linearVelocity,
            wanderDirection * wanderSpeed,
            Time.deltaTime * 4f
        );
    }

    void PickNewWanderDirection()
    {
        Vector3 randomDir =
            Random.insideUnitSphere.normalized;

        randomDir.y *= verticalMovementAmount;

        wanderDirection = randomDir.normalized;
    }

    void ChargeKi()
    {
        StopMovement();

        attacks.ChargeKi();
    }

    void MeleeAttack()
    {
        if (isComboAttacking)
            return;

        StopMovement();

        StartCoroutine(ComboRoutine());
    }

    IEnumerator ComboRoutine()
    {
        isComboAttacking = true;

        yield return StartCoroutine(
            melee.FullCombo()
        );

        isComboAttacking = false;

        currentState = AIState.Chase;
    }

    IEnumerator KiBlastBurst()
    {
        bool massiveSpam =
            Random.value < 0.20f;

        int burstAmount;

        float delay;

        if (massiveSpam)
        {
            burstAmount =
                Random.Range(15, 35);

            delay =
                Random.Range(0.015f, 0.04f);
        }
        else
        {
            burstAmount =
                Random.Range(3, 8);

            delay =
                Random.Range(0.05f, 0.18f);
        }

        for (int i = 0; i < burstAmount; i++)
        {
            attacks.FireKiBlast();

            if (Random.value < 0.15f)
            {
                rb.linearVelocity +=
                    Random.insideUnitSphere * 3f;
            }

            yield return new WaitForSeconds(delay);
        }
    }

    void StopMovement()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void HandleTransformation()
    {
        print("Checking transformation conditions");

        if (healthSlider.value < 4000)
        {
            print("works");
            attacks.TransformSuperSaiyan();
        }
    }
}