using System.Collections;
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

    [Tooltip("Optional: explicit enemy to move toward. If null the script will search for the closest enemy.")]
    public Transform enemyTarget;
    public float autoTargetRange = 100f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        boostFly.SetActive(false);

        rb.angularDamping = 0f;
    }

    void FixedUpdate()
    {
        // Acquire input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Determine forward direction: prefer explicit target or find closest enemy.
        Vector3 forward;
        Transform target = enemyTarget ?? FindClosestEnemy();
        if (target != null)
        {
            // Use full 3D direction toward the enemy (includes Y)
            forward = (target.position - transform.position).normalized;
            if (forward.sqrMagnitude <= 0.0001f)
                forward = transform.forward;
        }
        else
        {
            // fallback to character forward
            forward = transform.forward;
        }

        // Compute right relative to the forward direction. If forward is almost vertical, fall back safely.
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        if (right.sqrMagnitude <= 0.0001f)
            right = transform.right;
        right = right.normalized;

        // Determine current speeds without overwriting inspector values
        float currentMoveSpeed;
        float currentVerticalSpeed;
        bool boostInput = Input.GetKey(KeyCode.LeftShift);
        bool hasKi = attacksScript == null || attacksScript.currentKi > 0f;
        bool canBoost = boostInput && hasKi;

        if (canBoost)
        {
            currentMoveSpeed = 30f;
            currentVerticalSpeed = 30f;

            if (attacksScript != null)
            {
                attacksScript.currentKi = Mathf.Max(0f, attacksScript.currentKi - boostKiDrainPerSecond * Time.fixedDeltaTime);
            }
        }
        else
        {
            currentMoveSpeed = 20f;
            currentVerticalSpeed = 20f;
        }

        // Movement includes the Y component from 'forward' so W/S moves directly toward/away from the enemy (including vertical)
        Vector3 move = (forward * v + right * h) * currentMoveSpeed;

        // Additional manual vertical control (space / left control) is additive
        float vertical = 0f;
        if (Input.GetKey(KeyCode.Space))
            vertical = currentVerticalSpeed;
        else if (Input.GetKey(KeyCode.LeftControl))
            vertical = -currentVerticalSpeed;

        // Combine: keep move.y (toward target) and add manual vertical input
        Vector3 newVelocity = move + Vector3.up * vertical;

        // If small input, zero horizontal movement (keeps Y from 'move' intact)
        if (Mathf.Abs(h) < 0.1f && Mathf.Abs(v) < 0.1f)
        {
            newVelocity.x = 0f;
            newVelocity.z = 0f;
        }

        // If the user is not explicitly pressing vertical keys we don't overwrite the Y component — this preserves moving up/down toward the enemy
        if (!Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftControl))
        {
            // Do nothing here so we keep move.y (direction-to-enemy Y). This comment explains why we don't set newVelocity.y = 0.
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

    private Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float bestDist = autoTargetRange;
        Transform best = null;

        foreach (var go in enemies)
        {
            if (go == null) continue;
            float d = Vector3.Distance(transform.position, go.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = go.transform;
            }
        }

        return best;
    }
}