using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Attacks : MonoBehaviour
{
    public GameObject kC;
    public GameObject kCB;
    public GameObject canvas1;
    public GameObject chargeKi;
    public Animator kiCharge;
    public Animator ScreenShake;

    void Start()
    {
        kC.SetActive(false);
        kCB.SetActive(false);
        canvas1.SetActive(false);
        chargeKi.SetActive(false);
    }


    void Update()
    {
        Kamehameha();
        ChargeKi();
    }
    
    public void Kamehameha()
    {
        if (Input.GetKey(KeyCode.R))
        {
            kC.SetActive(true);
            StartCoroutine(KCDelay());
        }
    }

    public IEnumerator KCDelay()
    {
        yield return new WaitForSeconds(2f);
        canvas1.SetActive(true);
        kCB.SetActive(true);
        StartCoroutine(KCDelay2());
        ScreenShake.SetTrigger("Shake");
    }

    public IEnumerator KCDelay2()
    {
        yield return new WaitForSeconds(3);
        kC.SetActive(false);
        canvas1.SetActive(false);
        kCB.SetActive(false);
        ScreenShake.ResetTrigger("Shake");
    }

    public void ChargeKi()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ScreenShake.SetTrigger("Shake");
            chargeKi.SetActive(true);
        }

        if (Input.GetKeyUp(KeyCode.C))
        {
            StartCoroutine(KiFade());
        }
    }

    public IEnumerator KiFade()
    {
        kiCharge.SetTrigger("Fade");
        yield return new WaitForSeconds(1);
        chargeKi.SetActive(false);
    }
}
