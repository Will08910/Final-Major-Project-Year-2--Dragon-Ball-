using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Attacks : MonoBehaviour
{
    public GameObject KC;
    public GameObject KCB;

    void Start()
    {
        KC.SetActive(false);
        KCB.SetActive(false);

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
        yield return new WaitForSeconds(2f);
        KCB.SetActive(true);
        StartCoroutine(KCDelay2());
    }

    public IEnumerator KCDelay2()
    {
        yield return new WaitForSeconds(1);
        KC.SetActive(false);
        yield return new WaitForSeconds(5);
        KCB.SetActive(false);
    }


}
