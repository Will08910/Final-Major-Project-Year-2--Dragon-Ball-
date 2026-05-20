using UnityEngine;

public class BeamClashTrigger : MonoBehaviour
{
    public BeamClashManager clashManager;

    private bool hasClashed = false;

    void OnEnable()
    {
        // Reset so the trigger can fire again when the beam object is reused.
        hasClashed = false;
        Debug.LogFormat("[BeamClashTrigger] OnEnable - '{0}' reset hasClashed = false", gameObject.name);
    }

    void OnDisable()
    {
        Debug.LogFormat("[BeamClashTrigger] OnDisable - '{0}' (hasClashed={1})", gameObject.name, hasClashed);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.LogFormat("[BeamClashTrigger] OnTriggerEnter - '{0}' collided with '{1}'. hasClashed={2}", gameObject.name, other.gameObject.name, hasClashed);

        if (hasClashed)
        {
            Debug.LogFormat("[BeamClashTrigger] Ignoring collision because hasClashed is true on '{0}'", gameObject.name);
            return;
        }

        bool validClash =
            this.CompareTag("PlayerBeam") &&
            other.CompareTag("EnemyBeam");

        bool reverseClash =
            this.CompareTag("EnemyBeam") &&
            other.CompareTag("PlayerBeam");

        Debug.LogFormat("[BeamClashTrigger] validClash={0}, reverseClash={1}", validClash, reverseClash);

        if (validClash || reverseClash)
        {
            hasClashed = true;
            Debug.LogFormat("[BeamClashTrigger] Clash registered on '{0}'", gameObject.name);

            BeamClashTrigger otherTrigger = other.GetComponent<BeamClashTrigger>();

            if (otherTrigger != null)
            {
                otherTrigger.hasClashed = true;
                Debug.LogFormat("[BeamClashTrigger] Marked other trigger '{0}' hasClashed = true", other.gameObject.name);
            }

            if (clashManager != null)
            {
                Debug.Log("[BeamClashTrigger] Starting beam clash via manager");
                clashManager.StartBeamClash();
            }
            else
            {
                Debug.LogWarning("[BeamClashTrigger] clashManager is null - cannot start clash");
            }

            // deactivate both beam objects (existing behavior)
            gameObject.SetActive(false);
            other.gameObject.SetActive(false);
            Debug.LogFormat("[BeamClashTrigger] Deactivated beams '{0}' and '{1}'", gameObject.name, other.gameObject.name);
        }
    }
}