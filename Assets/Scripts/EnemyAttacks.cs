using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAttacks : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Rigidbody rb;

    [Header("UI")]
    public Slider kiBar;
    public Slider easeKiBar;

    [Header("Ki")]
    public float maxKi = 10000f;
    public float currentKi = 10000f;

    public float passiveKiRegen = 150f;
    public float chargeValue = 1200f;

    public float kiLerpSpeed = 0.1f;
    public float kiEaseLerpSpeed = 0.05f;

    [Header("Ki Blast")]
    public GameObject kiBlastPrefab;
    public Transform kiSpawnPoint;
    public Transform kiSpawnPointA;
    public Transform kiSpawnPointB;

    public float kiBlastSpeed = 40f;
    public float kiBlastHomingStrength = 5f;
    public float kiBlastLifetime = 8f;
    public float range = 100f;

    public int kiBlastCost = 50;
    public float kiBlastCooldown = 0.5f;

    [Header("Meteor Strike")]
    public GameObject meteorStrike;
    public Animator meteorStrikeAnim;
    public ParticleSystem effect1;
    public ParticleSystem effect2;

    public int meteorStrikeCost = 400;
    public float meteorCooldown = 4f;

    [Header("Kamehameha")]
    public GameObject kC;
    public GameObject kCB;

    [Header("Transformation")]
    public GameObject superSaiyan;
    public bool isSuperSaiyan = false;
    public float damageMultiplier = 1.3f;

    private bool kiBlastOnCooldown = false;
    private bool meteorOnCooldown = false;

    public bool isFiring = false;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (player == null)
        {
            GameObject p =
                GameObject.FindGameObjectWithTag("Player");

            if (p != null)
                player = p.transform;
        }

        currentKi = maxKi;

        if (meteorStrike != null)
            meteorStrike.SetActive(false);

        if (superSaiyan != null)
            superSaiyan.SetActive(false);

        if (kC != null)
            kC.SetActive(false);

        if (kCB != null)
            kCB.SetActive(false);

        if (kiBar != null)
        {
            kiBar.maxValue = maxKi;
            kiBar.value = currentKi;
        }

        if (easeKiBar != null)
        {
            easeKiBar.maxValue = maxKi;
            easeKiBar.value = currentKi;
        }
    }

    void Update()
    {
        currentKi += passiveKiRegen * Time.deltaTime;

        currentKi = Mathf.Clamp(
            currentKi,
            0,
            maxKi
        );

        UpdateKiUI();
    }

    void UpdateKiUI()
    {
        if (kiBar != null)
        {
            kiBar.value = Mathf.Lerp(
                kiBar.value,
                currentKi,
                kiLerpSpeed
            );

            if (Mathf.Abs(
                kiBar.value - currentKi
            ) < 0.01f)
            {
                kiBar.value = currentKi;
            }
        }

        if (easeKiBar != null)
        {
            easeKiBar.value = Mathf.Lerp(
                easeKiBar.value,
                currentKi,
                kiEaseLerpSpeed
            );

            if (Mathf.Abs(
                easeKiBar.value - currentKi
            ) < 0.01f)
            {
                easeKiBar.value = currentKi;
            }
        }
    }

    public void FireKiBlast()
    {
        if (kiBlastOnCooldown)
            return;

        if (currentKi < kiBlastCost)
            return;

        UseKi(kiBlastCost);

        SpawnKiBlast();

        StartCoroutine(
            KiBlastCooldownCoroutine()
        );
    }

    public void SpawnKiBlast()
    {
        if (kiBlastPrefab == null)
            return;

        Vector3 spawnPos;

        Vector3 forward;

        Transform chosenSpawn = null;

        if (
            kiSpawnPointA != null &&
            kiSpawnPointB != null
        )
        {
            chosenSpawn =
                Random.value < 0.5f
                ? kiSpawnPointA
                : kiSpawnPointB;
        }
        else if (kiSpawnPoint != null)
        {
            chosenSpawn = kiSpawnPoint;
        }

        spawnPos =
            chosenSpawn != null
            ? chosenSpawn.position
            : transform.position +
              transform.forward * 2f;

        if (player != null)
        {
            forward =
                (
                    player.position -
                    spawnPos
                ).normalized;
        }
        else
        {
            forward = transform.forward;
        }

        GameObject blast = Instantiate(
            kiBlastPrefab,
            spawnPos,
            Quaternion.LookRotation(forward)
        );

        Rigidbody blastRb =
            blast.GetComponent<Rigidbody>();

        if (blastRb != null)
        {
            blastRb.linearVelocity =
                forward * kiBlastSpeed;
        }

        EnemyKiBlast kb =
            blast.GetComponent<EnemyKiBlast>();

        if (kb != null)
        {
            kb.speed = kiBlastSpeed;
            kb.homingStrength =
                kiBlastHomingStrength;
            kb.lifetime = kiBlastLifetime;
            kb.range = range;
        }
    }

    IEnumerator KiBlastCooldownCoroutine()
    {
        kiBlastOnCooldown = true;

        yield return new WaitForSeconds(
            kiBlastCooldown
        );

        kiBlastOnCooldown = false;
    }

    public void ChargeKi()
    {
        currentKi +=
            chargeValue * Time.deltaTime;

        currentKi = Mathf.Clamp(
            currentKi,
            0,
            maxKi
        );
    }

    public void TransformSuperSaiyan()
    {
        if (isSuperSaiyan)
            return;

        if (currentKi <= 0)
            return;

        isSuperSaiyan = true;

        if (superSaiyan != null)
            superSaiyan.SetActive(true);
    }

    public void UseMeteorStrike()
    {
        if (meteorOnCooldown)
            return;

        if (currentKi < meteorStrikeCost)
            return;

        UseKi(meteorStrikeCost);

        StartCoroutine(
            MeteorStrikeRoutine()
        );
    }

    IEnumerator MeteorStrikeRoutine()
    {
        meteorOnCooldown = true;

        if (meteorStrike != null)
            meteorStrike.SetActive(true);

        if (meteorStrikeAnim != null)
            meteorStrikeAnim.SetTrigger("Strike");

        yield return new WaitForSeconds(0.2f);

        if (effect1 != null)
            effect1.Emit(30);

        if (effect2 != null)
            effect2.Play();

        Collider[] hits =
            Physics.OverlapSphere(
                meteorStrike.transform.position,
                2f
            );

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                EnemyHealth ph =
                    hit.GetComponent<EnemyHealth>();

                if (ph != null)
                {
                    int damage =
                        isSuperSaiyan
                        ? Mathf.RoundToInt(
                            1000 *
                            damageMultiplier
                          )
                        : 1000;

                    ph.TakeDamage(damage);
                }

                Rigidbody playerRb =
                    hit.attachedRigidbody;

                if (playerRb != null)
                {
                    playerRb.linearVelocity =
                        Vector3.zero;

                    playerRb.AddForce(
                        Vector3.down * 60f,
                        ForceMode.Impulse
                    );
                }
            }
        }

        yield return new WaitForSeconds(0.3f);

        if (meteorStrike != null)
            meteorStrike.SetActive(false);

        yield return new WaitForSeconds(
            meteorCooldown
        );

        meteorOnCooldown = false;
    }

    public void UseKamehameha()
    {
        if (isFiring)
            return;

        if (currentKi < 2000)
            return;

        UseKi(2000);

        StartCoroutine(
            KamehamehaRoutine()
        );
    }

    IEnumerator KamehamehaRoutine()
    {
        isFiring = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (kC != null)
            kC.SetActive(true);

        yield return new WaitForSeconds(2f);

        if (kCB != null)
            kCB.SetActive(true);

        RaycastHit[] hits =
            Physics.RaycastAll(
                transform.position,
                transform.forward,
                range
            );

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Player"))
            {
                EnemyHealth ph =
                    hit.collider.GetComponent<EnemyHealth>();

                if (ph != null)
                {
                    int damage =
                        isSuperSaiyan
                        ? Mathf.RoundToInt(
                            500 *
                            damageMultiplier
                          )
                        : 500;

                    ph.TakeDamage(damage);
                }
            }
        }

        yield return new WaitForSeconds(3f);

        if (kC != null)
            kC.SetActive(false);

        if (kCB != null)
            kCB.SetActive(false);

        isFiring = false;
    }

    public void UseKi(int amount)
    {
        currentKi -= amount;

        currentKi = Mathf.Max(
            currentKi,
            0
        );
    }
}