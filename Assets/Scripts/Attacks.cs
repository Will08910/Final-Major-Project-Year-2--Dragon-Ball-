using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Attacks : MonoBehaviour
{
    [Header("Kamehameha Variables")]
    public GameObject kC;
    public GameObject kCB;
    public Animator extraEffectsAnim;
    public AudioSource chargeSound;
    public AudioSource fireSound;

    [Header("Meteor Strike")]
    public GameObject meteorStrike;
    public Animator meteorStrikeAnim;
    public ParticleSystem effect1;
    public ParticleSystem effect2;
    public int meteorStrikeCost = 400;
    public float meteorCooldown = 4f;
    public AudioSource meteorSound;

    [Header("Super Saiyan")]
    public GameObject superSaiyan;
    public float superSaiyanDrain = 50f;
    public TrailRenderer boostTrail;
    public ParticleSystem boost;
    public Material hairColour;

    private Color originalTrailColor;
    private float originalStartAlpha;
    private float originalEndAlpha;

    private Color originalHairEmissionColor;
    private bool hairOriginallyHadEmission = false;

    public static bool isSuperSaiyan = false;
    public static float damageMultiplier = 1.3f;

    [Header("Camera Effects")]
    public GameObject canvas1;
    public CameraFovController camController;
    public GameObject lightning;
    public Animator cam;

    [Header("Ki UI")]
    public Slider kiBar;
    public Slider easeKiBar;

    public MonoBehaviour movementScript;

    public EnemyState characterState;

    [Header("Ki Charge")]
    public GameObject chargeKi;
    public Animator kiCharge;
    public float chargeCooldown = 0.5f;

    private bool isCharging = false;
    private bool chargeOnCooldown = false;
    private bool isFiring = false;

    public float maxKi;
    public float currentKi;

    public float range = 100f;

    [Header("Hit Effects")]
    public GameObject hitEffect;
    public GameObject kiHitEffect;

    [Header("Ki Blast")]
    public GameObject kiBlastPrefab;
    public Transform kiSpawnPoint;
    public Transform kiSpawnPointA;
    public Transform kiSpawnPointB;
    public float kiBlastSpeed = 40f;
    public float kiBlastHomingStrength = 5f;
    public float kiBlastLifetime = 8f;
    public float kiBlastCooldown = 0.5f;
    public int kiBlastCost = 50;

    private bool kiBlastOnCooldown = false;
    private bool meteorOnCooldown = false;

    public float kiLerpSpeed = 0.1f;
    public float kiEaseLerpSpeed = 0.05f;

    public int chargeValue = 1;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        meteorStrike.SetActive(false);

        if (superSaiyan != null) superSaiyan.SetActive(false);

        if (boostTrail != null)
        {
            originalTrailColor = boostTrail.startColor;
            originalStartAlpha = boostTrail.startColor.a;
            originalEndAlpha = boostTrail.endColor.a;
        }

        if (hairColour != null)
        {
            hairOriginallyHadEmission = hairColour.IsKeywordEnabled("_EMISSION");
            if (hairColour.HasProperty("_EmissionColor"))
                originalHairEmissionColor = hairColour.GetColor("_EmissionColor");
            else
                originalHairEmissionColor = Color.black;
        }

        cam.enabled = true;
        camController.enabled = false;
        kC.SetActive(false);
        kCB.SetActive(false);
        canvas1.SetActive(false);
        chargeKi.SetActive(false);
        lightning.SetActive(false);

        currentKi = maxKi;
        kiBar.value = currentKi;
        easeKiBar.value = currentKi;

        if (characterState == null)
            characterState = GetComponent<EnemyState>();
    }

    void Update()
    {
        bool isStunned = characterState != null && characterState.isStunned;

        if (!isStunned)
        {
            MeteorStrike();
            HandleCharge();
            HandleKamehameha();
            HandleKiBlastInput();
            HandleSuperSaiyan();
        }

        DrainSuperSaiyanKi();

        if (isSuperSaiyan && boost != null)
        {
            var emission = boost.emission;
            if (emission.enabled)
                emission.enabled = false;
        }

        if (kiBar.value != currentKi)
        {
            kiBar.value = Mathf.Lerp(kiBar.value, currentKi, kiLerpSpeed);
            if (Mathf.Abs(kiBar.value - currentKi) < 0.01f)
                kiBar.value = currentKi;
        }

        if (Mathf.Abs(kiBar.value - currentKi) < 0.001f)
        {
            if (easeKiBar.value != currentKi)
            {
                easeKiBar.value = Mathf.Lerp(easeKiBar.value, currentKi, kiEaseLerpSpeed);
                if (Mathf.Abs(easeKiBar.value - currentKi) < 0.01f)
                    easeKiBar.value = currentKi;
            }
        }
    }

    void HandleSuperSaiyan()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (!isSuperSaiyan && currentKi > 0)
            {
                isSuperSaiyan = true;

                if (hairColour != null)
                {
                    Color goldHDR = new Color(2f, 1.5f, 0f);
                    Color mixed = Color.Lerp(Color.white, goldHDR, 0.5f);
                    float emissionStrength = 1.5f;
                    Color emissive = mixed * emissionStrength;

                    hairColour.EnableKeyword("_EMISSION");
                    if (hairColour.HasProperty("_EmissionColor"))
                        hairColour.SetColor("_EmissionColor", emissive);
                }

                if (superSaiyan != null)
                    superSaiyan.SetActive(true);

                if (boostTrail != null)
                {
                    Color goldHDR = new Color(2f, 1.5f, 0f);
                    Color mixed = Color.Lerp(Color.white, goldHDR, 0.5f);

                    float emissionStrength = 1.5f;
                    Color emissive = mixed * emissionStrength;

                    Color start = emissive;
                    Color end = emissive;

                    start.a = originalStartAlpha;
                    end.a = originalEndAlpha;

                    boostTrail.startColor = start;
                    boostTrail.endColor = end;
                }

                if (boost != null)
                {
                    var emission = boost.emission;
                    emission.enabled = false;
                    boost.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
            else
            {
                isSuperSaiyan = false;

                if (hairColour != null)
                {
                    if (hairColour.HasProperty("_EmissionColor"))
                        hairColour.SetColor("_EmissionColor", originalHairEmissionColor);

                    if (!hairOriginallyHadEmission)
                        hairColour.DisableKeyword("_EMISSION");
                }

                if (superSaiyan != null)
                    superSaiyan.SetActive(false);

                if (boostTrail != null)
                {
                    boostTrail.startColor = originalTrailColor;

                    Color end = originalTrailColor;
                    end.a = originalEndAlpha;
                    boostTrail.endColor = end;
                }

                if (boost != null)
                {
                    var emission = boost.emission;
                    emission.enabled = true;
                    boost.Play();
                }
            }
        }
    }

    void DrainSuperSaiyanKi()
    {
        if (!isSuperSaiyan) return;

        currentKi -= superSaiyanDrain * Time.deltaTime;

        if (currentKi <= 0)
        {
            currentKi = 0;
            isSuperSaiyan = false;

            if (hairColour != null)
            {
                if (hairColour.HasProperty("_EmissionColor"))
                    hairColour.SetColor("_EmissionColor", originalHairEmissionColor);

                if (!hairOriginallyHadEmission)
                    hairColour.DisableKeyword("_EMISSION");
            }

            if (superSaiyan != null)
                superSaiyan.SetActive(false);

            if (boostTrail != null)
            {
                boostTrail.startColor = originalTrailColor;

                Color end = originalTrailColor;
                end.a = originalEndAlpha;
                boostTrail.endColor = end;
            }

            if (boost != null)
                boost.Play();
        }
    }

    public static int ApplyDamage(int baseDamage)
    {
        return isSuperSaiyan ? (int)(baseDamage * damageMultiplier) : baseDamage;
    }

    void MeteorStrike()
    {
        if (Input.GetKeyDown(KeyCode.E) && !meteorOnCooldown && currentKi >= meteorStrikeCost)
        {
            UseKi(meteorStrikeCost);
            meteorStrike.SetActive(true);
            meteorStrikeAnim.SetTrigger("Strike");
            meteorSound.Play();
            StartCoroutine(MeteorStrikeWait());
            StartCoroutine(MeteorCooldown());
        }
    }

    IEnumerator MeteorStrikeWait()
    {
        yield return new WaitForSeconds(0.2f);

        effect1.Emit(30);
        effect2.Play();
        cam.SetTrigger("Shake");

        Collider[] hits = Physics.OverlapSphere(meteorStrike.transform.position, 2f);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyHealth eh = hit.GetComponent<EnemyHealth>();
                if (eh != null)
                    eh.TakeDamage(ApplyDamage(1000));

                EnemyState state = hit.GetComponent<EnemyState>();

                if (state != null)
                {
                    state.Stun(1.5f);
                }

                Rigidbody enemyRb = hit.attachedRigidbody ?? hit.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    enemyRb.linearVelocity = Vector3.zero;
                    enemyRb.AddForce(Vector3.down * 60f, ForceMode.Impulse);
                }
            }
        }

        yield return new WaitForSeconds(0.3f);
        meteorStrike.SetActive(false);
    }

    IEnumerator MeteorCooldown()
    {
        meteorOnCooldown = true;
        yield return new WaitForSeconds(meteorCooldown);
        meteorOnCooldown = false;
    }

    void HandleKiBlastInput()
    {
        if (Input.GetMouseButtonDown(1) && !isFiring && !isCharging && kiBlastPrefab != null && !kiBlastOnCooldown && currentKi >= kiBlastCost)
        {
            UseKi(kiBlastCost);
            SpawnKiBlast();
            StartCoroutine(KiBlastCooldownCoroutine());
        }
    }

    public void SpawnKiBlast()
    {
        Camera camRef = Camera.main;
        Vector3 spawnPos;
        Vector3 forward;

        Transform chosenSpawn = null;

        if (kiSpawnPointA != null && kiSpawnPointB != null)
            chosenSpawn = (Random.value < 0.5f) ? kiSpawnPointA : kiSpawnPointB;
        else if (kiSpawnPoint != null)
            chosenSpawn = kiSpawnPoint;
        else if (kiSpawnPointA != null)
            chosenSpawn = kiSpawnPointA;
        else if (kiSpawnPointB != null)
            chosenSpawn = kiSpawnPointB;

        if (chosenSpawn != null)
        {
            spawnPos = chosenSpawn.position;
            forward = chosenSpawn.forward;
        }
        else if (camRef != null)
        {
            spawnPos = camRef.transform.position + camRef.transform.forward * 1.2f;
            forward = camRef.transform.forward;
        }
        else
        {
            spawnPos = transform.position + transform.forward * 1.2f;
            forward = transform.forward;
        }

        GameObject blast = Instantiate(kiBlastPrefab, spawnPos, Quaternion.LookRotation(forward));
        Rigidbody blastRb = blast.GetComponent<Rigidbody>();
        if (blastRb != null)
            blastRb.linearVelocity = forward * kiBlastSpeed;

        KiBlast kb = blast.GetComponent<KiBlast>();
        if (kb != null)
        {
            kb.speed = kiBlastSpeed;
            kb.homingStrength = kiBlastHomingStrength;
            kb.lifetime = kiBlastLifetime;
            kb.range = range;
        }
    }

    IEnumerator KiBlastCooldownCoroutine()
    {
        kiBlastOnCooldown = true;
        yield return new WaitForSeconds(kiBlastCooldown);
        kiBlastOnCooldown = false;
    }

    public void UseKi(int amount)
    {
        currentKi -= amount;
        currentKi = Mathf.Max(currentKi, 0);
    }

    void HandleCharge()
    {
        if (Input.GetKeyDown(KeyCode.C) && !isFiring && !isCharging && !chargeOnCooldown && (rb == null || rb.linearVelocity.magnitude == 0f))
        {
            isCharging = true;

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            chargeKi.SetActive(true);
        }

        if (Input.GetKeyUp(KeyCode.C) && isCharging)
        {
            isCharging = false;
            StartCoroutine(KiFade());
            StartCoroutine(ChargeCooldownCoroutine());
        }

        if (isCharging)
        {
            currentKi += chargeValue * Time.deltaTime * 150f;
            currentKi = Mathf.Clamp(currentKi, 0, maxKi);
        }
    }

    IEnumerator ChargeCooldownCoroutine()
    {
        chargeOnCooldown = true;
        yield return new WaitForSeconds(chargeCooldown);
        chargeOnCooldown = false;
    }

    IEnumerator KiFade()
    {
        kiCharge.SetTrigger("Fade");
        yield return new WaitForSeconds(1f);
        chargeKi.SetActive(false);
    }

    void HandleKamehameha()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isFiring && !isCharging && currentKi >= 2000)
        {
            StartCoroutine(KamehamehaRoutine());
            UseKi(2000);
            camController.enabled = true;
            cam.SetTrigger("Fov");
        }
    }

    IEnumerator KamehamehaRoutine()
    {
        isFiring = true;

        if (movementScript != null)
            movementScript.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        kC.SetActive(true);
        lightning.SetActive(true);

        yield return new WaitForSeconds(2f);

        extraEffectsAnim.SetTrigger("Fade");
        canvas1.SetActive(true);
        kCB.SetActive(true);

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] hits = Physics.RaycastAll(ray, range);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Ground"))
            {
                GameObject effect = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                GameObject effect2 = Instantiate(kiHitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(effect, 3f);
                Destroy(effect2, 3f);
                break;
            }
        }

        yield return new WaitForSeconds(3f);

        lightning.SetActive(false);
        
        kC.SetActive(false);
        canvas1.SetActive(false);
        kCB.SetActive(false);

        if (movementScript != null)
            movementScript.enabled = true;

        camController.enabled = false;

        ResetAttack();
    }

    void ResetAttack()
    {
        isFiring = false;
    }

    void OnDisable()
    {
        isFiring = false;

        if (movementScript != null)
            movementScript.enabled = true;
    }
}