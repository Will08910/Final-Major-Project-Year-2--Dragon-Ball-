using System.Collections;
using TMPro;
using UnityEngine;

public class BeamClashManager : MonoBehaviour
{
    [Header("UI")]
    public RectTransform buttonContainer;
    public GameObject clashButtonPrefab;
    public TMP_Text timerText;
    public TMP_Text scoreText;

    [Header("Settings")]
    public float clashDuration = 6f;
    public float aftermathDuration = 3f;
    public float spawnRate = 0.2f;
    public int clicksNeededToWin = 20;

    [Header("Animator")]
    public Animator animator;

    [Header("Results")]
    public bool playerWon = false;

    [Header("Cutscene Objects")]
    public GameObject currentPlayer;
    public GameObject currentEnemy;
    public GameObject clashPlayer;
    public GameObject clashEnemy;

    [Header("Damage")]
    public int lossDamage = 500;

    [Header("Combat Scripts")]
    public MonoBehaviour[] playerCombatScripts;
    public MonoBehaviour[] enemyCombatScripts;

    private int currentClicks;
    private bool clashRunning;

    public GameObject A17;

    void Start()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        A17.SetActive(true);
    }

    public void StartBeamClash()
    {
        StopAllCoroutines();

        if (animator != null)
        {
            animator.SetBool("Win", false);
            animator.SetBool("Lose", false);
        }

        gameObject.SetActive(true);

        playerWon = false;
        currentClicks = 0;
        clashRunning = true;

        foreach (MonoBehaviour script in playerCombatScripts)
        {
            if (script != null)
                if(clashRunning == true)
                    script.enabled = false;
        }

        foreach (MonoBehaviour script in enemyCombatScripts)
        {
            if (script != null)
                if (clashRunning == true)
                    script.enabled = false;
        }

        if (clashPlayer != null)
        {
            clashPlayer.SetActive(false);
            clashPlayer.SetActive(true);
        }

        if (clashEnemy != null)
        {
            clashEnemy.SetActive(false);
            clashEnemy.SetActive(true);
        }

        StartCoroutine(ClashRoutine());
        StartCoroutine(SpawnButtonsRoutine());
    }

    IEnumerator ClashRoutine()
    {
        float timer = clashDuration;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            timerText.text =
                Mathf.CeilToInt(timer).ToString();

            scoreText.text =
                currentClicks.ToString() +
                " / " +
                clicksNeededToWin;

            yield return null;
        }

        clashRunning = false;

        playerWon =
            currentClicks >= clicksNeededToWin;

        if (animator != null)
        {
            animator.SetBool("Win", playerWon);
            animator.SetBool("Lose", !playerWon);
        }

        yield return new WaitForSeconds(
            aftermathDuration
        );

        EndClash();
    }

    IEnumerator SpawnButtonsRoutine()
    {
        while (clashRunning)
        {
            if (currentClicks >= clicksNeededToWin)
                yield break;

            SpawnButton();

            yield return new WaitForSeconds(
                spawnRate
            );
        }
    }

    void SpawnButton()
    {
        GameObject button = Instantiate(
            clashButtonPrefab,
            buttonContainer
        );

        RectTransform rect =
            button.GetComponent<RectTransform>();

        float width =
            buttonContainer.rect.width;

        float height =
            buttonContainer.rect.height;

        float randomX = Random.Range(
            -width / 2f,
            width / 2f
        );

        float randomY = Random.Range(
            -height / 2f,
            height / 2f
        );

        rect.anchoredPosition =
            new Vector2(randomX, randomY);

        BeamClashButton clashButton =
            button.GetComponent<BeamClashButton>();

        clashButton.Setup(this);

        Destroy(button, 1f);
    }

    public void RegisterClick()
    {
        if (!clashRunning)
            return;

        currentClicks++;

        currentClicks = Mathf.Clamp(
            currentClicks,
            0,
            clicksNeededToWin
        );
    }

    void EndClash()
    {
        StopAllCoroutines();

        clashRunning = false;

        foreach (Transform child in buttonContainer)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        if (playerWon)
        {
            if (currentEnemy != null)
            {
                EnemyHealth eh =
                    currentEnemy.GetComponent<EnemyHealth>();

                if (eh != null)
                {
                    eh.TakeDamage(lossDamage);
                }
            }
        }
        else
        {
            if (currentPlayer != null)
            {
                EnemyHealth ph =
                    currentPlayer.GetComponent<EnemyHealth>();

                if (ph != null)
                {
                    ph.TakeDamage(lossDamage);
                }
            }
        }

        if (clashPlayer != null)
            clashPlayer.SetActive(false);

        if (clashEnemy != null)
            clashEnemy.SetActive(false);

        if (animator != null)
        {
            animator.SetBool("Win", false);
            animator.SetBool("Lose", false);
        }

        foreach (MonoBehaviour script in playerCombatScripts)
        {
            if (clashRunning == false)
                script.enabled = true;
        }

        foreach (MonoBehaviour script in enemyCombatScripts)
        {
            if (clashRunning == false)
                script.enabled = true;
        }

        StartCoroutine(DelayCutscene());

        gameObject.SetActive(false);
    }

    IEnumerator DelayCutscene()
    {
        yield return new WaitForSeconds(4f);

        foreach (MonoBehaviour script in playerCombatScripts)
        {
                if (clashRunning == false)
                    script.enabled = true;
        }

        foreach (MonoBehaviour script in enemyCombatScripts)
        {
                if (clashRunning == false)
                    script.enabled = true;
        }
    }
}