using UnityEngine;
using UnityEngine.InputSystem;

public class FlyController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float verticalSpeed = 5f;
    float targetFOV;
    public GameObject boostFly;
    public Animator boostFlyFade;
    InputAction MoveVert;

    public Camera cam;

    public Attacks attacksScript; 
    public float boostKiDrainPerSecond = 100f;

    private Rigidbody rb;
    private EnemyState characterState;

    public Transform enemyTarget;
    public float autoTargetRange = 100f;

    [Header("Boost Toggle (Controller)")]
    public KeyCode boostToggleButton = KeyCode.JoystickButton8;
    private bool boostActive = false;

    private Coroutine fadeCoroutine;

    public TrailRenderer[] boostTrailRenderers;
    private float[] originalTrailTimes;

    [Header("Movement damping")]
    [Tooltip("How quickly the player slows to a stop when there's no input (higher = faster).")]
    public float stopDamping = 12f;
    [Tooltip("Velocity magnitude under which horizontal velocity is snapped to zero.")]
    public float stopThreshold = 0.05f;

    [Header("Collision handling")]
    [Tooltip("If collision relative velocity is below this, zero horizontal drift on collision.")]
    public float collisionStopThreshold = 2f;
    [Tooltip("How quickly horizontal drift is reduced while staying in contact.")]
    public float collisionFriction = 8f;

    [Header("Idle drag")]
    [Tooltip("Linear drag applied when no input to kill residual drift.")]
    public float idleDrag = 8f;

    void Start()
    {
        MoveVert = InputSystem.actions.FindAction("MoveVert");

        rb = GetComponent<Rigidbody>();
        characterState = GetComponent<EnemyState>();

        if (boostFly != null)
            boostFly.SetActive(false);

        rb.angularDamping = 0f;

        rb.constraints |= RigidbodyConstraints.FreezeRotation;
        rb.angularDamping = 5f;

        rb.linearDamping = 0f;

        if ((boostTrailRenderers == null || boostTrailRenderers.Length == 0) && boostFly != null)
        {
            boostTrailRenderers = boostFly.GetComponentsInChildren<TrailRenderer>(true);
        }

        if (boostTrailRenderers != null && boostTrailRenderers.Length > 0)
        {
            originalTrailTimes = new float[boostTrailRenderers.Length];
            for (int i = 0; i < boostTrailRenderers.Length; i++)
            {
                var tr = boostTrailRenderers[i];
                originalTrailTimes[i] = tr != null ? tr.time : 0.5f;
            }
        }
    }

    void FixedUpdate()
    {
        if (characterState != null && characterState.isStunned)
            return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float rightStickY = 0f;
        if (Gamepad.current != null)
        {
            rightStickY = Gamepad.current.rightStick.ReadValue().y;
        }
        else if (MoveVert != null)
        {
            try
            {
                rightStickY = MoveVert.ReadValue<float>();
            }
            catch
            {
                try
                {
                    rightStickY = MoveVert.ReadValue<Vector2>().y;
                }
                catch
                {
                    rightStickY = 0f;
                }
            }
        }

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;

        Vector3 right = transform.right;
        right.y = 0f;
        right = right.sqrMagnitude > 0.0001f ? right.normalized : Vector3.right;

        float currentMoveSpeed;
        float currentVerticalSpeed;

        bool boostInputHold = Input.GetKey(KeyCode.LeftShift);
        bool hasKi = attacksScript == null || attacksScript.currentKi > 0f;
        bool boostRequested = boostInputHold || boostActive;
        bool canBoost = boostRequested && hasKi;

        if (canBoost)
        {
            currentMoveSpeed = 30f;
            currentVerticalSpeed = 30f;

            if (attacksScript != null)
                attacksScript.currentKi = Mathf.Max(0f, attacksScript.currentKi - boostKiDrainPerSecond * Time.fixedDeltaTime);
        }
        else
        {
            currentMoveSpeed = 20f;
            currentVerticalSpeed = 20f;

            if (boostActive && !hasKi)
            {
                boostActive = false;
                DisableBoostVisuals();
            }
        }

        Vector3 horizontalDesired = (forward * v + right * h) * currentMoveSpeed;

        bool hasHorizontalInput = Mathf.Abs(h) >= 0.1f || Mathf.Abs(v) >= 0.1f;
        bool manualVerticalActive = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftControl);
        float manualVertical = 0f;
        if (Input.GetKey(KeyCode.Space))
            manualVertical = currentVerticalSpeed;
        else if (Input.GetKey(KeyCode.LeftControl))
            manualVertical = -currentVerticalSpeed;

        const float rightStickDeadzone = 0.5f;
        if (Mathf.Abs(rightStickY) >= rightStickDeadzone)
        {
            manualVerticalActive = true;
            manualVertical = rightStickY > 0f ? currentVerticalSpeed : -currentVerticalSpeed;
        }

        if (rb == null) return;

        if (hasHorizontalInput || manualVerticalActive)
        {
            rb.linearDamping = 0f;

            Vector3 targetVelocity = rb.linearVelocity;

            if (hasHorizontalInput)
            {
                targetVelocity.x = horizontalDesired.x;
                targetVelocity.z = horizontalDesired.z;
            }

            if (manualVerticalActive)
            {
                targetVelocity.y = manualVertical;
            }
            else if (Mathf.Abs(rightStickY) < rightStickDeadzone && !Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftControl))
            {
                targetVelocity.y = 0f;
            }

            rb.linearVelocity = targetVelocity;
        }
        else
        {
            rb.linearDamping = idleDrag;

            Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            Vector3 damped = Vector3.Lerp(horizontalVel, Vector3.zero, stopDamping * Time.fixedDeltaTime);

            if (damped.magnitude < stopThreshold)
                damped = Vector3.zero;

            float currentY = rb.linearVelocity.y;
            Vector3 newVel = new Vector3(damped.x, currentY, damped.z);

            if (Mathf.Abs(rightStickY) < rightStickDeadzone && !Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftControl))
            {
                newVel.y = 0f;
            }

            rb.linearVelocity = newVel;
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
    }

    void Update()
    {
        if (Input.GetKeyDown(boostToggleButton))
        {
            boostActive = !boostActive;

            if (boostActive)
                EnableBoostVisuals();
            else
                DisableBoostVisuals();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
            EnableBoostVisuals();

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            if (!boostActive)
                DisableBoostVisuals();
        }

        if (attacksScript != null && attacksScript.currentKi <= 0f && (boostFly != null && boostFly.activeSelf || boostActive))
        {
            boostActive = false;
            DisableBoostVisuals();
        }

        if (boostTrailRenderers != null)
        {
            for (int i = 0; i < boostTrailRenderers.Length; i++)
            {
                var tr = boostTrailRenderers[i];
                if (tr == null) continue;
                if (Mathf.Abs(tr.time - 0.5f) > 0.0001f)
                    tr.time = 0.5f;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (rb == null) return;

        if (collision.collider.isTrigger) return;

        Vector3 v = rb.linearVelocity;
        v.x = 0f;
        v.z = 0f;
        rb.linearVelocity = v;
        rb.angularVelocity = Vector3.zero;
    }

    void OnCollisionStay(Collision collision)
    {
        if (rb == null) return;

        if (collision.collider.isTrigger) return;

        Vector3 vel = rb.linearVelocity;
        Vector3 horiz = new Vector3(vel.x, 0f, vel.z);
        Vector3 reduced = Vector3.Lerp(horiz, Vector3.zero, collisionFriction * Time.fixedDeltaTime);
        if (reduced.magnitude < stopThreshold) reduced = Vector3.zero;
        rb.linearVelocity = new Vector3(reduced.x, vel.y, reduced.z);
    }

    void EnableBoostVisuals()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        if (boostFly != null && !boostFly.activeSelf)
            boostFly.SetActive(true);

        if (boostFlyFade != null)
            boostFlyFade.SetBool("Fade", false);

        if (boostTrailRenderers != null)
        {
            for (int i = 0; i < boostTrailRenderers.Length; i++)
            {
                var tr = boostTrailRenderers[i];
                if (tr == null) continue;

                tr.Clear();
#if UNITY_2019_1_OR_NEWER
                tr.emitting = true;
#endif
            }
        }
    }

    void DisableBoostVisuals()
    {
        if (fadeCoroutine == null)
            fadeCoroutine = StartCoroutine(FadeTrailCoroutine());

        if (boostTrailRenderers != null)
        {
            for (int i = 0; i < boostTrailRenderers.Length; i++)
            {
                var tr = boostTrailRenderers[i];
                if (tr == null) continue;
#if UNITY_2019_1_OR_NEWER
                tr.emitting = false;
#endif
            }
        }
    }

    System.Collections.IEnumerator FadeTrailCoroutine()
    {
        if (boostFlyFade != null)
            boostFlyFade.SetBool("Fade", true);

        yield return new WaitForSeconds(0.25f);

        if (boostFly != null)
            boostFly.SetActive(false);

        fadeCoroutine = null;
    }
}