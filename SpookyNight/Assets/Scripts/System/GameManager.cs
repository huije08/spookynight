using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [HideInInspector] public float playTime = 0f;
    [HideInInspector] public int totalKills = 0;
    [HideInInspector] public int totalPumpkins = 0;
    [HideInInspector] public bool isGameOver = false;
    [HideInInspector] public bool isGameClear = false;

    private bool isPlaying = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Update()
    {
        if (isPlaying)
            playTime += Time.deltaTime;
    }

    public void StartGame()
    {
        ResetStats();
        isPlaying = true;
        SceneManager.LoadScene("GameScene");
    }

    public void GameOver()
    {
        isPlaying = false;
        isGameOver = true;
        UIManager.Instance.ShowGameOver();
    }

    public void GameClear()
    {
        isPlaying = false;
        isGameClear = true;

        PlayerStats player = FindObjectOfType<PlayerStats>();
        if (player != null)
            totalPumpkins = player.pumpkins;

        UIManager.Instance.ShowGameClear();
    }

    public void OnEnemyKilled()
    {
        totalKills++;
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        isPlaying = false;
        SceneManager.LoadScene("MenuScene");
    }

    void ResetStats()
    {
        playTime      = 0f;
        totalKills    = 0;
        totalPumpkins = 0;
        isGameOver    = false;
        isGameClear   = false;
    }

    public string GetFormattedTime()
    {
        int min = Mathf.FloorToInt(playTime / 60f);
        int sec = Mathf.FloorToInt(playTime % 60f);
        return $"{min:00}:{sec:00}";
    }
}
