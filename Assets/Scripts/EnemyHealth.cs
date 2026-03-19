using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public Slider healthSlider;
    public Slider easeHealthSlider;
    public float maxHealth;
    public float currentHealth;

    public float healthLerpSpeed = 0.1f; 
    public float easeLerpSpeed = 0.05f;   

    void Start()
    {
        currentHealth = maxHealth;
        healthSlider.value = currentHealth;
        easeHealthSlider.value = currentHealth;
    }

    void Update()
    {

        if (healthSlider.value != currentHealth)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, currentHealth, healthLerpSpeed);


            if (Mathf.Abs(healthSlider.value - currentHealth) < 0.01f)
                healthSlider.value = currentHealth;
        }


        if (Mathf.Abs(healthSlider.value - currentHealth) < 0.001f)
        {
            if (easeHealthSlider.value != currentHealth)
            {
                easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, currentHealth, easeLerpSpeed);

                if (Mathf.Abs(easeHealthSlider.value - currentHealth) < 0.01f)
                    easeHealthSlider.value = currentHealth;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}