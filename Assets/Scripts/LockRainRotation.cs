using UnityEngine;

public class LockRainRotation : MonoBehaviour
{
    [Tooltip("Transform of the player to follow. If left null, will try to find a GameObject tagged 'Player' on Reset.")]
    public Transform player;

    [Tooltip("Vertical offset above the player (in world units).")]
    public float yOffset = 10f;

    [Tooltip("Smooth movement towards the target position.")]
    public bool smooth = true;

    [Tooltip("Smoothing speed (higher = faster).")]
    public float followSpeed = 5f;

    private void Reset()
    {
        if (player == null)
        {
            var found = GameObject.FindWithTag("Player");
            if (found != null)
                player = found.transform;
        }
    }
    
    private void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 targetPosition = player.position + Vector3.up * yOffset;

        if (smooth)
        {
            float t = Mathf.Clamp01(Time.deltaTime * followSpeed);
            transform.position = Vector3.Lerp(transform.position, targetPosition, t);
        }
        else
        {
            transform.position = targetPosition;
        }
    }
}
