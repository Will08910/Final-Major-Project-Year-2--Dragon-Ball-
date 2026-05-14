using UnityEngine;
using UnityEngine.UI;

public class BeamClashButton : MonoBehaviour
{
    private BeamClashManager manager;

    public void Setup(BeamClashManager m)
    {
        manager = m;

        GetComponent<Button>()
            .onClick
            .AddListener(OnClicked);
    }

    void OnClicked()
    {
        manager.RegisterClick();

        Destroy(gameObject);
    }
}