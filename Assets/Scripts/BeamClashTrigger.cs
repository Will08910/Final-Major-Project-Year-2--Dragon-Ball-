using UnityEngine;

public class BeamClashTrigger : MonoBehaviour
{
    public BeamClashManager clashManager;

    private bool hasClashed = false;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("TRIGGER");
        if (hasClashed)
            return;

        bool validClash =
            CompareTag("PlayerBeam") &&
            other.CompareTag("EnemyBeam");

        bool reverseClash =
            CompareTag("EnemyBeam") &&
            other.CompareTag("PlayerBeam");

        if (validClash || reverseClash)
        {
            hasClashed = true;

            BeamClashTrigger otherTrigger =
                other.GetComponent<BeamClashTrigger>();

            if (otherTrigger != null)
            {
                otherTrigger.hasClashed = true;
            }

            if (clashManager != null)
            {
                clashManager.StartBeamClash();
            }

            gameObject.SetActive(false);
            other.gameObject.SetActive(false);
        }
    }
}