using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Attacks : MonoBehaviour
{
    public GameObject kC;
    public GameObject kCB;
    public GameObject canvas1;

    public Slider kiBar;
    public Slider easeKiBar;

    public MonoBehaviour movementScript;

    public GameObject chargeKi;
    public Animator kiCharge;

    private bool isCharging = false;
    private bool isFiring = false;

    public float maxKi;
    public float currentKi;

    public float kiLerpSpeed = 0.1f;
    public float kiEaseLerpSpeed = 0.05f;

    public int chargeValue = 1;

    private Rigidbody rb; // ✅ NEW

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // ✅ GET RIGIDBODY

        kC.SetActive(false);
        kCB.SetActive(false);
        canvas1.SetActive(false);
        chargeKi.SetActive(false);

        currentKi = maxKi;
        kiBar.value = currentKi;
        easeKiBar.value = currentKi;
    }

    void Update()
    {
        HandleCharge();
        HandleKamehameha();

        // Smooth ki bar
        if (kiBar.value != currentKi)
        {
            kiBar.value = Mathf.Lerp(kiBar.value, currentKi, kiLerpSpeed);

            if (Mathf.Abs(kiBar.value - currentKi) < 0.01f)
            {
                kiBar.value = currentKi;
            }
        }

        // Ease bar lag
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

    public void UseKi(int amount)
    {
        currentKi -= amount;
        currentKi = Mathf.Max(currentKi, 0);
    }

    void HandleCharge()
    {
        if (Input.GetKeyDown(KeyCode.C) && !isFiring)
        {
            isCharging = true;

            // 🔥 STOP movement when charging starts
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
        }

        if (isCharging)
        {
            currentKi += chargeValue * Time.deltaTime * 100f;
            currentKi = Mathf.Clamp(currentKi, 0, maxKi);
        }
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
        }
    }

    IEnumerator KamehamehaRoutine()
    {
        isFiring = true;

        // Disable movement
        if (movementScript != null)
            movementScript.enabled = false;

        // 💥 HARD STOP PLAYER
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        kC.SetActive(true);

        yield return new WaitForSeconds(2f);

        canvas1.SetActive(true);
        kCB.SetActive(true);

        yield return new WaitForSeconds(3f);

        kC.SetActive(false);
        canvas1.SetActive(false);
        kCB.SetActive(false);

        // Re-enable movement
        if (movementScript != null)
            movementScript.enabled = true;

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