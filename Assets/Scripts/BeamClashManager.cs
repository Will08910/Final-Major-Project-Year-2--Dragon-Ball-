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

    private int currentClicks;

    private bool clashRunning;

    void Start()
    {
        gameObject.SetActive(false);
    }

    public void StartBeamClash()
    {
        gameObject.SetActive(true);

        playerWon = false;

        currentClicks = 0;

        clashRunning = true;

        ToggleGameplayCharacter(
            currentPlayer,
            true
        );

        ToggleGameplayCharacter(
            currentEnemy,
            true
        );

        if (clashPlayer != null)
            clashPlayer.SetActive(true);

        if (clashEnemy != null)
            clashEnemy.SetActive(true);

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
            animator.SetBool(
                "Win",
                playerWon
            );

            animator.SetBool(
                "Lose",
                !playerWon
            );
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
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        ToggleGameplayCharacter(
            currentPlayer,
            true
        );

        ToggleGameplayCharacter(
            currentEnemy,
            true
        );

        if (clashPlayer != null)
            clashPlayer.SetActive(false);

        if (clashEnemy != null)
            clashEnemy.SetActive(false);

        EnemyAttacks enemyAttacks =
    currentEnemy.GetComponent<EnemyAttacks>();

        if (enemyAttacks != null)
        {
            if (enemyAttacks.kC != null)
                enemyAttacks.kC.SetActive(false);

            if (enemyAttacks.kCB != null)
                enemyAttacks.kCB.SetActive(false);

            enemyAttacks.isFiring = false;
        }

        Attacks playerAttacks =
    currentPlayer.GetComponent<Attacks>();

        if (playerAttacks != null)
        {
            if (playerAttacks.kC != null)
                playerAttacks.kC.SetActive(false);

            if (playerAttacks.kCB != null)
                playerAttacks.kCB.SetActive(false);

            playerAttacks.enabled = true;
        }

        gameObject.SetActive(false);
    }

    void ToggleGameplayCharacter(
    GameObject obj,
    bool enabledState
)
    {
        if (obj == null)
            return;

        Renderer[] renderers =
            obj.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            r.enabled = enabledState;
        }

        Collider[] colliders =
            obj.GetComponentsInChildren<Collider>();

        foreach (Collider c in colliders)
        {
            c.enabled = enabledState;
        }

        MonoBehaviour[] scripts =
            obj.GetComponentsInChildren<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            if (
                script is BeamClashTrigger ||
                script is BeamClashManager
            )
            {
                continue;
            }

            script.enabled = enabledState;
        }

        Rigidbody rb =
            obj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

}