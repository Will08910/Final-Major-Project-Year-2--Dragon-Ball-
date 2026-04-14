using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Attacks : MonoBehaviour
{
    public GameObject kC;
    public GameObject kCB;
    public GameObject canvas1;
    public CameraFovController camController;
    public GameObject lightning;

    public Animator cam;

    public Slider kiBar;
    public Slider easeKiBar;

    public MonoBehaviour movementScript;

    public GameObject chargeKi;
    public Animator kiCharge;

    private bool isCharging = false;
    private bool isFiring = false;

    public float maxKi;
    public float currentKi;

    public float range = 100f;
    public GameObject hitEffect;
    public GameObject kiHitEffect;

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