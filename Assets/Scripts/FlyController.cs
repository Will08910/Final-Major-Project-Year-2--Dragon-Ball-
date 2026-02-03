using UnityEngine;

public class FlyController : MonoBehaviour
{
    public float moveSpeed;
    public float maxFloatHeight = 10f;
    public float minFloatHeight;
    public Transform target;

    private float currentHeight;

    void Start()
    {
        currentHeight = transform.position.y;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (target == null) return;

        Vector3 lookDirection = target.position - transform.position;
        transform.rotation = Quaternion.LookRotation(lookDirection);

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (h != 0 || v != 0)
        {
            MoveCharacter(h, v);
        }

        currentHeight = Mathf.Clamp(currentHeight, minFloatHeight, maxFloatHeight);
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
    }

    private void MoveCharacter(float h, float v)
    {
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Vector3 direction = (forward * v + right * h).normalized;


        transform.position += direction * moveSpeed * Time.deltaTime;
    }
}
