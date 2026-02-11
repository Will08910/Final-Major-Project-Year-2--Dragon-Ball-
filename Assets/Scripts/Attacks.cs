using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Attacks : MonoBehaviour
{
    public GameObject KC;
    void Start()
    {
        KC.SetActive(false);
        
    }


    void Update()
    {
        Kamehameha();
    }
    
    public void Kamehameha()
    {
        if (Input.GetKey(KeyCode.R))
        {
            KC.SetActive(true);
            StartCoroutine(KCDelay());
        }
    }

    public IEnumerator KCDelay()
    {
        yield return new WaitForSeconds(6);
        KC.SetActive(false);
    }


}
