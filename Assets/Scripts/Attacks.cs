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

    public Animator screenShake;

    private bool isCharging = false;
    private bool isFiring = false;

    public float maxKi;
    public float currentKi;

    public float kiLerpSpeed = 0.1f;
    public float kiEaseLerpSpeed = 0.05f;

    public int chargeValue = 1;
    void Start()
    {
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
            chargeKi.SetActive(true);
            screenShake.SetTrigger("Shake");
        }

        if (Input.GetKeyUp(KeyCode.C) && isCharging)
        {
            isCharging = false;
            StartCoroutine(KiFade());
        }

        if (isCharging)
        {
            currentKi += chargeValue * Time.deltaTime * 100f; // adjust speed here
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

        if (movementScript != null)
            movementScript.enabled = false;

        kC.SetActive(true);

        yield return new WaitForSeconds(2f);

        canvas1.SetActive(true);
        kCB.SetActive(true);
        screenShake.SetTrigger("Shake");

        yield return new WaitForSeconds(3f);

        kC.SetActive(false);
        canvas1.SetActive(false);
        kCB.SetActive(false);

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