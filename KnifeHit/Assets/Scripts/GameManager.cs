using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI Settings")]
    public Text inGameScoreText;
    public Text knivesText;

    [Header("Knife Settings")]
    public GameObject[] knifePrefabs;
    public Transform knifeSpawnPoint;

    [Header("Target Prefabs")]
    public GameObject[] normalTargetPrefabs;
    public GameObject[] bossTargetPrefabs;
    public Transform targetSpawnPoint;

    private int targetsDestroyed = 0; 
    private int score = 0;
    private int level = 1;
    private int knivesLeft;
    private int totalKnivesForLevel;

    private GameObject currentKnife;
    private GameObject currentTargetObject;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        targetsDestroyed = 0; 
        SpawnNewTarget();
    }

    void SpawnNewTarget()
    {
        bool isBossLevel = (level % 5 == 0);

        totalKnivesForLevel = isBossLevel ? Random.Range(10, 14) : Random.Range(4, 8);
        knivesLeft = totalKnivesForLevel;

        if (currentTargetObject != null)
        {
            Destroy(currentTargetObject);
        }

        GameObject[] prefabsToUse = isBossLevel ? bossTargetPrefabs : normalTargetPrefabs;

        if (prefabsToUse.Length > 0)
        {
            GameObject selectedPrefab = prefabsToUse[Random.Range(0, prefabsToUse.Length)];

            currentTargetObject = Instantiate(selectedPrefab, targetSpawnPoint.position, Quaternion.identity);

            TargetRotation rotationScript = currentTargetObject.GetComponent<TargetRotation>();
            if (rotationScript != null)
            {
                rotationScript.InitTarget(isBossLevel, targetsDestroyed);
            }

            targetsDestroyed++;
        }

        UpdateUI();
        Invoke("SpawnKnife", 0.1f);
    }

    public void OnHit()
    {
        score += (level % 5 == 0) ? 2 : 1;
        knivesLeft--;

        if (audioSource != null) audioSource.Play();

        UpdateUI();
        currentKnife = null;

        if (knivesLeft > 0)
        {
            Invoke("SpawnKnife", 0.1f);
        }
        else
        {
            NextLevel();
        }
    }

    void NextLevel()
    {
        level++;
        SpawnNewTarget();
    }

    void SpawnKnife()
    {
        if (currentKnife != null) return;

        int selectedIndex = PlayerPrefs.GetInt("SelectedKnife", 0);
        if (selectedIndex >= knifePrefabs.Length) selectedIndex = 0;

        currentKnife = Instantiate(knifePrefabs[selectedIndex], knifeSpawnPoint.position, Quaternion.identity);

        SpriteRenderer sr = currentKnife.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            float targetHeight = 3f;
            float currentHeight = sr.bounds.size.y;
            float scaleFactor = targetHeight / currentHeight;
            currentKnife.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        }
    }

    void ClearKnives()
    {
        GameObject[] knives = GameObject.FindGameObjectsWithTag("Knife");
        foreach (GameObject k in knives)
        {
            Destroy(k);
        }
    }

    void UpdateUI()
    {
        if (inGameScoreText) inGameScoreText.text = score.ToString();

        if (knivesText)
        {
            int currentKnifeNumber = (totalKnivesForLevel - knivesLeft) + 1;
            if (currentKnifeNumber > totalKnivesForLevel) currentKnifeNumber = totalKnivesForLevel;

            knivesText.text = $"{currentKnifeNumber}/{totalKnivesForLevel}";
        }
    }

    public void GameOver()
    {
        PlayerPrefs.SetInt("LastScore", score);

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (score > highScore) PlayerPrefs.SetInt("HighScore", score);

        PlayerPrefs.SetInt("TotalCoins", PlayerPrefs.GetInt("TotalCoins", 0) + score);
        PlayerPrefs.Save();

        SceneManager.LoadScene("GameOver");
    }
}