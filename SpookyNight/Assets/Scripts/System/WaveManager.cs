using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("웨이브 설정")]
    public int totalWaves = 10;
    public float timeBetweenWaves = 3f;

    [Header("적 프리팹")]
    public GameObject BooPrefab;
    public GameObject AxeBooPrefab;
    public GameObject WitchBooPrefab;
    public GameObject BossPrefab;

    [Header("스폰 설정")]
    public float spawnRadius = 20f;

    private int currentWave = 0;
    private int enemiesAlive = 0;
    private bool waveInProgress = false;

    //  일반, 도끼, 마녀 
    private int[,] waveTable = new int[,]
    {
        { 5,  0,  0 },   // 웨이브 1   웨이브3 마다 상점출현
        { 8,  0,  0 },   // 웨이브 2
        { 0,  0,  0 },   // 웨이브 3 → 상점
        { 6,  4,  0 },   // 웨이브 4
        { 8,  5,  0 },   // 웨이브 5
        { 0,  0,  0 },   // 웨이브 6 → 상점
        { 5,  5,  3 },   // 웨이브 7
        { 6,  6,  4 },   // 웨이브 8
        { 0,  0,  0 },   // 웨이브 9 → 상점
        { 0,  0,  0 },   // 웨이브 10 → 보스
    };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(StartNextWave());
    }

    IEnumerator StartNextWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);

        currentWave++;
        if (currentWave > totalWaves) yield break;

        UIManager.Instance.UpdateWave(currentWave, totalWaves);
        UIManager.Instance.ShowWaveAnnounce(currentWave);

        // 상점 웨이브 (3, 6, 9)
        if (currentWave % 3 == 0 && currentWave != totalWaves)
        {
            ShopManager.Instance.OpenShop();
            yield break;
        }

        // 보스 웨이브 (10)
        if (currentWave == totalWaves)
        {
            SpawnBoss();
            yield break;
        }

        // 일반 웨이브
        SpawnWave(currentWave);
    }

    void SpawnWave(int wave)
    {
        int index = wave - 1;
        int BooCount = waveTable[index, 0];
        int AxeBooCount    = waveTable[index, 1];
        int WitchBooCount = waveTable[index, 2];

        enemiesAlive = BooCount + AxeBooCount + WitchBooCount;
        waveInProgress = true;

        StartCoroutine(SpawnEnemies(BooPrefab, BooCount, 0.3f));
        StartCoroutine(SpawnEnemies(AxeBooPrefab, AxeBooCount, 0.2f));
        StartCoroutine(SpawnEnemies(WitchBooPrefab, WitchBooCount, 0.5f));
    }

    IEnumerator SpawnEnemies(GameObject prefab, int count, float interval)
    {
        if (prefab == null || count == 0) yield break;

        for (int i = 0; i < count; i++)
        {
            Instantiate(prefab, GetSpawnPosition(), Quaternion.identity);
            yield return new WaitForSeconds(interval);
        }
    }

    Vector3 GetSpawnPosition()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Vector3 playerPos = playerObj != null ? playerObj.transform.position : Vector3.zero;

        Vector2 circle = Random.insideUnitCircle.normalized * spawnRadius;
        return new Vector3(playerPos.x + circle.x, 0f, playerPos.z + circle.y);
    }

    // 적 사망 시 호출
    public void OnEnemyDied()
    {
        enemiesAlive--;

        if (enemiesAlive <= 0 && waveInProgress)
        {
            waveInProgress = false;
            StartCoroutine(StartNextWave());
        }
    }

    // 상점 닫고 재개
    public void ResumeAfterShop()
    {
        StartCoroutine(StartNextWave());
    }

    // 보스 스폰
    void SpawnBoss()
    {
        if (BossPrefab == null) return;

        Instantiate(BossPrefab, GetSpawnPosition(), Quaternion.identity);
        enemiesAlive = 1;
        waveInProgress = true;
        Debug.Log("보스 등장!");
    }

    public void OnBossDefeated()
    {
        GameManager.Instance.GameClear();
    }

    public int GetCurrentWave() => currentWave;
}
