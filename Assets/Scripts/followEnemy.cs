using UnityEngine;

public class followEnemy : MonoBehaviour
{
    public Transform enemy;

    void Start()
    {
        
    }

    void Update()
    {
        transform.position = enemy.position;
    }
}
