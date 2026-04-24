using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Attacks : MonoBehaviour
{
    [Header("Kamehameha Variables")]
    public GameObject kC;
    public GameObject kCB;
    public Animator extraEffectsAnim;

    [Header("Camera Effects")]
    public GameObject canvas1;
    public CameraFovController camController;
    public GameObject lightning;
    public Animator cam;
    
    [Header("Ki UI")]
    public Slider kiBar;
    public Slider easeKiBar;

    public MonoBehaviour movementScript;
    
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
    public Transform kiSpawnPoint; // optional single spawn point (kept for compatibility)
    public Transform kiSpawnPointA; // new spawn point A
    public Transform kiSpawnPointB; // new spawn point B
    public float kiBlastSpeed = 40f;
    public float kiBlastHomingStrength = 5f;
    public float kiBlastLifetime = 8f;
    public float kiBlastCooldown = 0.5f;
    public int kiBlastCost = 50;

    private bool kiBlastOnCooldown = false;

    public float kiLerpSpeed = 0.1f;
    public float kiEaseLerpSpeed = 0.05f;

    public int chargeValue = 1;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

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
    }

    void Update()
    {
        HandleCharge();
        HandleKamehameha();
        HandleKiBlastInput();

        if (kiBar.value != currentKi)
        {
            kiBar.value = Mathf.Lerp(kiBar.value, currentKi, kiLerpSpeed);

            if (Mathf.Abs(kiBar.value - currentKi) < 0.01f)
            {
                kiBar.value = currentKi;
            }
        }

        if (Mathf.Abs(kiBar.value - currentKi) < 0.001f)
        {
            if (easeKiBar.value != currentKi)
            {
                easeKiBar.value = Mathf.Lerp(easeKiBar.value, currentKi, kiEaseLerpSpeed);

                if (Mathf.Abs(easeKiBar.value - currentKi) < 0.01f)
                {
                    easeKiBar.value = currentKi;
                }
            }
        }
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

    void SpawnKiBlast()
    {
        Camera camRef = Camera.main;
        Vector3 spawnPos;
        Vector3 forward;

        Transform chosenSpawn = null;

        if (kiSpawnPointA != null && kiSpawnPointB != null)
        {
            chosenSpawn = (Random.value < 0.5f) ? kiSpawnPointA : kiSpawnPointB;
        }
        else if (kiSpawnPoint != null)
        {
            chosenSpawn = kiSpawnPoint;
        }
        else if (kiSpawnPointA != null)
        {
            chosenSpawn = kiSpawnPointA;
        }
        else if (kiSpawnPointB != null)
        {
            chosenSpawn = kiSpawnPointB;
        }

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
            currentKi += chargeValue * Time.deltaTime * 100f;
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

        Debug.DrawRay(ray.origin, ray.direction * range, Color.red, 2f);

        foreach (RaycastHit hit in hits)
        {
            Debug.Log("Hit: " + hit.collider.name);

            if (hit.collider.CompareTag("Ground"))
            {
                GameObject effect = Instantiate(
                    hitEffect,
                    hit.point,
                    Quaternion.LookRotation(hit.normal)
                );

                GameObject effect2 = Instantiate(
                    kiHitEffect,
                    hit.point,
                    Quaternion.LookRotation(hit.normal)
                );

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