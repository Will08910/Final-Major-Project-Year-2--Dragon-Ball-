using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BeamClashButton : MonoBehaviour
{
    private BeamClashManager manager;
    private bool triggered;

    enum ControllerButton { A = 0, B = 1, X = 2, Y = 3 }
    private ControllerButton requiredButton;

    private TMP_Text tmpLabel;
    private Text uiLabel;
    private Image buttonImage;

    public void Setup(BeamClashManager m)
    {
        manager = m;

        tmpLabel = GetComponentInChildren<TMP_Text>();
        if (tmpLabel == null)
            uiLabel = GetComponentInChildren<Text>();

        buttonImage = GetComponent<Image>();
        if (buttonImage == null)
            buttonImage = GetComponentInChildren<Image>();

        requiredButton = (ControllerButton)Random.Range(0, 4);

        string label = requiredButton.ToString();
        if (tmpLabel != null) tmpLabel.text = label;
        else if (uiLabel != null) uiLabel.text = label;

        ApplyColorForButton(requiredButton);

        var uiButton = GetComponent<Button>();
        if (uiButton != null)
            uiButton.onClick.AddListener(OnClicked);
    }

    void Update()
    {
        if (triggered)
            return;

        if (IsControllerButtonPressed(requiredButton))
            Trigger();
    }

    void OnClicked()
    {
        if (triggered)
            return;

        Trigger();
    }

    void Trigger()
    {
        triggered = true;

        if (manager != null)
            manager.RegisterClick();

        Destroy(gameObject);
    }

    void ApplyColorForButton(ControllerButton b)
    {
        Color fill = Color.white;
        Color textColor = Color.white;

        switch (b)
        {
            case ControllerButton.X:
                fill = new Color(0.0f, 0.5f, 1.0f); 
                textColor = Color.black;
                break;
            case ControllerButton.Y:
                fill = new Color(1.0f, 0.84f, 0.0f); 
                textColor = Color.black;
                break;
            case ControllerButton.B:
                fill = new Color(0.9f, 0.12f, 0.12f); 
                textColor = Color.black;
                break;
            case ControllerButton.A:
                fill = new Color(0.0f, 0.45f, 0.15f); 
                textColor = Color.black;
                break;
        }

        if (buttonImage != null)
            buttonImage.color = fill;

        if (tmpLabel != null)
            tmpLabel.color = textColor;
        else if (uiLabel != null)
            uiLabel.color = textColor;
    }

    bool IsControllerButtonPressed(ControllerButton b)
    {
        switch (b)
        {
            case ControllerButton.A:
                return Input.GetKeyDown(KeyCode.JoystickButton0);
            case ControllerButton.B:
                return Input.GetKeyDown(KeyCode.JoystickButton1);
            case ControllerButton.X:
                return Input.GetKeyDown(KeyCode.JoystickButton2);
            case ControllerButton.Y:
                return Input.GetKeyDown(KeyCode.JoystickButton3);
            default:
                return false;
        }
    }
}