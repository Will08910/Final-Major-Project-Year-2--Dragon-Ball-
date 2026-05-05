using UnityEngine;
using UnityEngine.UI;

public class EnemyLockOn : MonoBehaviour
{
    public Transform target;
    public Transform player;

    public float minDistance = 2f;
    public float maxDistance = 10f;
    public float fadeSpeed = 5f;

    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Update()
    {
        if (target == null || player == null) return;

        transform.position = target.position;

        float distance = Vector3.Distance(player.position, target.position);

        float targetAlpha = Mathf.InverseLerp(minDistance, maxDistance, distance);
        targetAlpha = Mathf.Clamp01(targetAlpha);

        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
    }
}