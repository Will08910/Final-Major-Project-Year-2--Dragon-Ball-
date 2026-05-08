using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public Animator fadeStuff;
    void Start()
    {
        
    }


    void Update()
    {
        
    }

    public void OnClickStart()
    {
        StartCoroutine(LoadBattle());
    }

    public void OnClickEnd()
    {
        Application.Quit();
    }

    IEnumerator LoadBattle()
    {
        fadeStuff.SetBool("FadeOut", true);
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("Battle");
    }
}
