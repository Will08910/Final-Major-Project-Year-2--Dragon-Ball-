using UnityEngine;

public class LookAtEnemy : MonoBehaviour
{
    public Transform target;

    void Update()
    {
        if (target == null) return;

        Vector3 lookDirection = target.position - transform.position;

        // Prevent tilting (IMPORTANT)
        lookDirection.y = 0f;

        transform.rotation = Quaternion.LookRotation(lookDirection);
    }
}