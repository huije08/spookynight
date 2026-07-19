using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AugmentType
{
    MultiShot,
    ChargeShot,
    DamageUp,
    FireRateUp,
    Piercing,
    MaxHpUp,
    Lifesteal,
    Shield,
}

[System.Serializable]
public class AugmentLevel
{
    public string description;
    public float value;
}

[System.Serializable]
public class Augment
{
    public string augmentName;
    public AugmentType type;
    public Sprite icon;
    public AugmentLevel[] levels;

    [HideInInspector] public int currentLevel = 0;

    public int MaxLevel => levels.Length;
    public bool IsMaxLevel => currentLevel >= MaxLevel;
    public bool IsOwned => currentLevel > 0;

    public string GetNextDesc()
    {
        if (IsMaxLevel) return "최대 레벨";
        return levels[currentLevel].description;
    }
}

public class AugmentManager : MonoBehaviour
{
    public static AugmentManager Instance;

    [Header("증강 아이콘")]
    public Sprite multiShotIcon;
    public Sprite chargeShotIcon;
    public Sprite damageUpIcon;
    public Sprite fireRateIcon;
    public Sprite piercingIcon;
    public Sprite maxHpIcon;
    public Sprite lifestealIcon;
    public Sprite shieldIcon;

    [Header("리롤 설정")]
    public int rerollCost = 15;             // 리롤 비용 (호박)

    private List<Augment> allAugments = new List<Augment>();
    private List<Augment> currentChoices = new List<Augment>();

    private PlayerStats playerStats;
    private PlayerShooting playerShooting;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        playerShooting = FindObjectOfType<PlayerShooting>();
        InitAugments();
    }

    void InitAugments()
    {
        allAugments = new List<Augment>
        {
            new Augment {
                augmentName = "점사",
                type = AugmentType.MultiShot,
                icon = multiShotIcon,
                levels = new AugmentLevel[]
                {
                    new AugmentLevel { description = "한 번 클릭에 2발 점사", value = 2 },
                    new AugmentLevel { description = "한 번 클릭에 3발 점사", value = 3 },
                    new AugmentLevel { description = "한 번 클릭에 5발 점사", value = 5 },
                }
            },
            new Augment {
                augmentName = "차지샷",
                type = AugmentType.ChargeShot,
                icon = chargeShotIcon,
                levels = new AugmentLevel[]
                {
                    new AugmentLevel { description = "차지샷 해금 (데미지 2배)", value = 2f },
                    new AugmentLevel { description = "차지샷 데미지 3배",        value = 3f },
                    new AugmentLevel { description = "차지샷 데미지 5배",        value = 5f },
                }
            },
            new Augment {
                augmentName = "과부하",
                type = AugmentType.DamageUp,
                icon = damageUpIcon,
                levels = new AugmentLevel[]
                {
                    new AugmentLevel { description = "데미지 +15", value = 15f },
                    new AugmentLevel { description = "데미지 +25", value = 25f },
                    new AugmentLevel { description = "데미지 +40", value = 40f },
                }
            },
            new Augment {
                augmentName = "급속 냉각",
                type = AugmentType.FireRateUp,
                icon = fireRateIcon,
                levels = new AugmentLevel[]
                {
                    new AugmentLevel { description = "발사속도 20% 증가", value = 0.2f },
                    new AugmentLevel { description = "발사속도 30% 증가", value = 0.3f },
                    new AugmentLevel { description = "발사속도 40% 증가", value = 0.4f },
                }
            },
            new Augment {
                augmentName = "관통 레이저",
                type = AugmentType.Piercing,
                icon = piercingIcon,
                levels = new AugmentLevel[]
                {
                    new AugmentLevel { description = "레이저 관통",          value = 0f },
                    new AugmentLevel { description = "관통 + 범위 확대",      value = 1f },
                    new AugmentLevel { description = "관통 + 범위 대폭 확대", value = 2f },
                }
            },
            new Augment {
                augmentName = "강화 장갑",
                type = AugmentType.MaxHpUp,
                icon = maxHpIcon,
                levels = new AugmentLevel[]
                {
                    new AugmentLevel { description = "최대 HP +30", value = 30f },
                    new AugmentLevel { description = "최대 HP +50", value = 50f },
                    new AugmentLevel { description = "최대 HP +80", value = 80f },
                }
            },
            new Augment {
                augmentName = "흡혈",
                type = AugmentType.Lifesteal,
                icon = lifestealIcon,
                levels = new AugmentLevel[]
                {
                    new AugmentLevel { description = "데미지 10% 흡혈", value = 0.1f },
                    new AugmentLevel { description = "데미지 20% 흡혈", value = 0.1f },
                    new AugmentLevel { description = "데미지 30% 흡혈", value = 0.1f },
                }
            },
            new Augment {
                augmentName = "에너지 실드",
                type = AugmentType.Shield,
                icon = shieldIcon,
                levels = new AugmentLevel[]
                {
                    new AugmentLevel { description = "15% 확률 피격 무효", value = 0.15f },
                    new AugmentLevel { description = "25% 확률 피격 무효", value = 0.10f },
                    new AugmentLevel { description = "40% 확률 피격 무효", value = 0.15f },
                }
            },
        };
    }

    // 증강 UI 열기
    public void OpenAugmentUI()
    {
        currentChoices = GetRandomAugments();
        Time.timeScale = 0f;
        UIManager.Instance.ShowAugmentUI(currentChoices);
    }

    // 리롤 (UI 리롤 버튼에서 호출)
    public void Reroll()
    {
        if (playerStats.pumpkins < rerollCost)
        {
            Debug.Log("호박 부족!");
            return;
        }

        playerStats.pumpkins -= rerollCost;
        UIManager.Instance.UpdatePumpkin(playerStats.pumpkins);

        currentChoices = GetRandomAugments();
        UIManager.Instance.ShowAugmentUI(currentChoices);
        Debug.Log($"리롤! 남은 호박: {playerStats.pumpkins}개");
    }

    List<Augment> GetRandomAugments()
    {
        return allAugments
            .Where(a => !a.IsMaxLevel)
            .OrderBy(_ => Random.value)
            .Take(3)
            .ToList();
    }

    // 카드 선택
    public void SelectAugment(int index)
    {
        if (index < 0 || index >= currentChoices.Count) return;
        ApplyAugment(currentChoices[index]);
        Time.timeScale = 1f;
        UIManager.Instance.HideAugmentUI();
    }

    void ApplyAugment(Augment augment)
    {
        float value = augment.levels[augment.currentLevel].value;

        switch (augment.type)
        {
            case AugmentType.MultiShot:
                playerShooting.hasMultiShot = true;
                playerShooting.bulletCount = (int)value;
                playerShooting.burstInterval = 0.08f;
                break;
            case AugmentType.ChargeShot:
                playerShooting.hasChargeShot = true;
                playerShooting.chargeMultiplier = value;
                break;
            case AugmentType.DamageUp:
                playerShooting.damage += value;
                break;
            case AugmentType.FireRateUp:
                playerShooting.fireRate *= (1f - value);
                playerShooting.fireRate = Mathf.Max(0.05f, playerShooting.fireRate);
                break;
            case AugmentType.Piercing:
                playerShooting.hasPiercing = true;
                playerShooting.piercingRadius = 0.4f + value * 0.3f;
                break;
            case AugmentType.MaxHpUp:
                playerStats.maxHp += value;
                playerStats.Heal(value);
                break;
            case AugmentType.Lifesteal:
                playerStats.lifesteal += value;
                break;
            case AugmentType.Shield:
                playerStats.shieldChance += value;
                break;
        }

        augment.currentLevel++;
        Debug.Log($"{augment.augmentName} Lv{augment.currentLevel} 적용!");
    }
}