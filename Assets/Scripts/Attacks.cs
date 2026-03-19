using System.Collections;
using UnityEngine;

public class Attacks : MonoBehaviour
{
    public GameObject kC;
    public GameObject kCB;
    public GameObject canvas1;

    public MonoBehaviour movementScript;

    public GameObject chargeKi;
    public Animator kiCharge;

    public Animator screenShake;

    private bool isCharging = false;
    private bool isFiring = false;

    void Start()
    {
        kC.SetActive(false);
        kCB.SetActive(false);
        canvas1.SetActive(false);
        chargeKi.SetActive(false);
    }

    void Update()
    {
        HandleCharge();
        HandleKamehameha();
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
    }

    IEnumerator KiFade()
    {
        kiCharge.SetTrigger("Fade");
        yield return new WaitForSeconds(1f);
        chargeKi.SetActive(false);
    }

    void HandleKamehameha()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isFiring && !isCharging)
        {
            StartCoroutine(KamehamehaRoutine());
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
        screenShake.ResetTrigger("Shake");

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