using UnityEngine;

public class LockOnCamera : MonoBehaviour
{
    public Transform follow;
    public Transform lookTarget;
    public Vector3 offset;
    public float followSpeed = 10f;

    void LateUpdate()
    {
        if (follow == null || lookTarget == null) return;
        {
            Vector3 desiredPosition = follow.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        }
    }

}