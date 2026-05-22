using System.Collections;
using UnityEngine;

public class FlyController : MonoBehaviour
{
    public EnemyState characterState;
    public float moveSpeed = 10f;
    public float verticalSpeed = 5f;
    float targetFOV;
    public GameObject boostFly;
    public Animator boostFlyFade;

    public Camera cam;

    public Attacks attacksScript; 
    public float boostKiDrainPerSecond = 100f;

    private Rigidbody rb;
    public AudioSource boostSound;

    [Tooltip("Optional: explicit enemy to move toward. If null the script will search for the closest enemy.")]
    public Transform enemyTarget;
    public float autoTargetRange = 100f;

    void Start()
    {
        if (characterState == null)
            characterState = GetComponent<EnemyState>();
        rb = GetComponent<Rigidbody>();
        boostFly.SetActive(false);

        rb.angularDamping = 0f;
    }

    void FixedUpdate()
    {
        if (rb.linearVelocity.magnitude < 0.1f)
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (characterState != null && characterState.isStunned)
        {
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 forward;
        Transform target = enemyTarget ?? FindClosestEnemy();
        if (target != null)
        {
            forward = (target.position - transform.position).normalized;
            if (forward.sqrMagnitude <= 0.0001f)
                forward = transform.forward;
        }
        else
        {
            forward = transform.forward;
        }

        Vector3 right = Vector3.Cross(Vector3.up, forward);
        if (right.sqrMagnitude <= 0.0001f)
            right = transform.right;
        right = right.normalized;

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

        Vector3 move = (forward * v + right * h) * currentMoveSpeed;

        float vertical = 0f;
        if (Input.GetKey(KeyCode.Space))
            vertical = currentVerticalSpeed;
        else if (Input.GetKey(KeyCode.LeftControl))
            vertical = -currentVerticalSpeed;

        Vector3 newVelocity = move + Vector3.up * vertical;


        if (Mathf.Abs(h) < 0.1f && Mathf.Abs(v) < 0.1f)
        {
            newVelocity.x = 0f;
            newVelocity.z = 0f;
        }


        if (!Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftControl))
        {

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
            boostSound.Play();
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            StartCoroutine(FadeTrail());
            boostSound.Stop();
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