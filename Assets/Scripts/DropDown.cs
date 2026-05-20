using JetBrains.Annotations;
using UnityEngine;

public class DropDown : MonoBehaviour
{
    public Animator Drop;
    public bool isOpen = true;

    void Start()
    {
        
    }


    void Update()
    {
        
    }

    public void OnClick()
    {
        print("Clicked");

        if (isOpen == true)
        {
            Drop.SetTrigger("Close");
            isOpen = false;
        }

        else
        {
            Drop.SetTrigger("Open");
            isOpen = true;
        }
    }
}
