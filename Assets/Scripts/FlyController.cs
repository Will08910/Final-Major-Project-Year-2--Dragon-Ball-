using System.Collections;
using System.Net;
using UnityEngine;

public class FlyController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float verticalSpeed = 5f;
    float targetFOV;
    public GameObject boostFly;
    public Animator boostFlyFade;

    public Camera cam;

    public Attacks attacksScript; 
    public float boostKiDrainPerSecond = 100f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        boostFly.SetActive(false);

        rb.angularDamping = 0f;
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Vector3 move = (forward * v + right * h) * moveSpeed;

        float vertical = 0f;

        if (Input.GetKey(KeyCode.Space))
            vertical = verticalSpeed;
        else if (Input.GetKey(KeyCode.LeftControl))
            vertical = -verticalSpeed;

        Vector3 newVelocity = new Vector3(move.x, vertical, move.z);

        if (Mathf.Abs(h) < 0.1f && Mathf.Abs(v) < 0.1f)
        {
            newVelocity.x = 0f;
            newVelocity.z = 0f;
        }

        if (!Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftControl))
        {
            newVelocity.y = 0f;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftControl))
        {
            targetFOV = 70f;
        }
        else
        {
            targetFOV = 60f;
        }

        bool boostInput = Input.GetKey(KeyCode.LeftShift);
        bool hasKi = attacksScript == null || attacksScript.currentKi > 0f;
        bool canBoost = boostInput && hasKi;

        if (canBoost)
        {
            moveSpeed = 30f;
            verticalSpeed = 30f;

            if (attacksScript != null)
            {
                attacksScript.currentKi = Mathf.Max(0f, attacksScript.currentKi - boostKiDrainPerSecond * Time.fixedDeltaTime);
            }
        }
        else
        {
            moveSpeed = 20f;
            verticalSpeed = 20f;
        }

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 5f);

        rb.linearVelocity = newVelocity;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            boostFlyFade.SetBool("Fade", false);
            boostFly.SetActive(true);
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            StartCoroutine(FadeTrail());
        }

        if (attacksScript != null && attacksScript.currentKi <= 0f && boostFly.activeSelf)
        {
            boostFlyFade.SetBool("Fade", true);
        }
    }

    IEnumerator FadeTrail()
    {
        boostFlyFade.SetBool("Fade", true);
        yield return new WaitForSeconds(3f);
    }
}