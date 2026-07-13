using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("HP")]
    public float maxHp = 100f;
    private float currentHp;

    [Header("경험치 & 레벨")]
    public int level = 1;
    public int currentExp = 0;
    public int expToNextLevel = 50;

    [Header("호박")]
    public int pumpkins = 0;

    [Header("증강 스탯")]
    [HideInInspector] public float lifesteal = 0f;
    [HideInInspector] public float shieldChance = 0f;

    void Start()
    {
        currentHp = maxHp;
        UIManager.Instance.UpdateHp(currentHp, maxHp);
    }

    // ─── HP ─────────────────────────────────────────────────
    public void TakeDamage(float dmg)
    {
        if (shieldChance > 0 && Random.value < shieldChance)
        {
            Debug.Log("실드 발동! 피격 무효");
            return;
        }

        currentHp -= dmg;
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        UIManager.Instance.UpdateHp(currentHp, maxHp);

        if (currentHp <= 0)
            GameOver();
    }

    public void Heal(float amount)
    {
        currentHp = Mathf.Min(currentHp + amount, maxHp);
        UIManager.Instance.UpdateHp(currentHp, maxHp);
    }

    // ─── 흡혈 ────────────────────────────────────────────────
    public void OnDamageDealt(float dmg)
    {
        if (lifesteal <= 0) return;
        Heal(dmg * lifesteal);
    }

    // ─── 경험치 & 레벨업 ─────────────────────────────────────
    public void GainExp(int amount)
    {
        currentExp += amount;
        if (currentExp >= expToNextLevel)
            LevelUp();
    }

    void LevelUp()
    {
        level++;
        currentExp -= expToNextLevel;
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.3f);
        Debug.Log($"레벨업! 현재 레벨: {level}");

        // 증강 선택 UI 열기
        AugmentManager.Instance.OpenAugmentUI();
    }

    void GameOver()
    {
        GameManager.Instance.GameOver();
    }

    public float GetCurrentHp() => currentHp;
}
