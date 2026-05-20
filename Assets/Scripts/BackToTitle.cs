using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackToTitle : MonoBehaviour
{
    public Slider playerHealth;
    public Slider enemyHealth;
    public GameObject player;
    public GameObject enemy;

    void Start()
    {
        
    }


    void Update()
    {
     if (playerHealth.value <= 0f || enemyHealth.value <= 0f || player == null || enemy == null)
        {
            SceneManager.LoadScene("FrontEnd");
        }
    }
}
