using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Transform target;

    void Start()
    {
        
    }


    void Update()
    {
        if (target == null) return;

        Vector3 lookDirection = target.position - transform.position;
        transform.rotation = Quaternion.LookRotation(lookDirection);
    }
}
