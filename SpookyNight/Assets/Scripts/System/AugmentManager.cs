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
public class Augment
{
    public string augmentName;
    public string description;
    public AugmentType type;
    public float value;
}

public class AugmentManager : MonoBehaviour
{
    public static AugmentManager Instance;

    private List<Augment> allAugments = new List<Augment>();
    private List<AugmentType> ownedAugments = new List<AugmentType>();

    private PlayerStats playerStats;
    private PlayerShooting playerShooting;

    // 현재 제시된 증강 3개
    private List<Augment> currentChoices = new List<Augment>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        playerStats    = FindObjectOfType<PlayerStats>();
        playerShooting = FindObjectOfType<PlayerShooting>();
        InitAugments();
    }

    void InitAugments()
    {
        allAugments = new List<Augment>
        {
            new Augment { augmentName = "멀티샷",    description = "레이저를 3발 동시에 발사",      type = AugmentType.MultiShot,  value = 3    },
            new Augment { augmentName = "차지샷",    description = "우클릭으로 강력한 차지샷 발사",  type = AugmentType.ChargeShot, value = 3f   },
            new Augment { augmentName = "과부하",    description = "데미지 +25",                    type = AugmentType.DamageUp,   value = 25f  },
            new Augment { augmentName = "급속 냉각", description = "발사 속도 30% 증가",            type = AugmentType.FireRateUp, value = 0.3f },
            new Augment { augmentName = "관통 레이저",description = "레이저가 적을 관통",           type = AugmentType.Piercing,   value = 0    },
            new Augment { augmentName = "강화 장갑", description = "최대 HP +50",                  type = AugmentType.MaxHpUp,    value = 50f  },
            new Augment { augmentName = "흡혈",      description = "데미지의 15%를 HP로 회복",      type = AugmentType.Lifesteal,  value = 0.15f},
            new Augment { augmentName = "에너지 실드",description = "피격 시 20% 확률로 무효화",    type = AugmentType.Shield,     value = 0.2f },
        };
    }

    // ─── 증강 UI 열기 ─────────────────────────────────────────
    public void OpenAugmentUI()
    {
        currentChoices = GetRandomAugments();
        Time.timeScale = 0f;
        UIManager.Instance.ShowAugmentUI(currentChoices);
    }

    // ─── 랜덤 3개 뽑기 ───────────────────────────────────────
    List<Augment> GetRandomAugments()
    {
        return allAugments
            .Where(a => !IsOneTime(a.type) || !ownedAugments.Contains(a.type))
            .OrderBy(_ => Random.value)
            .Take(3)
            .ToList();
    }

    // ─── 증강 선택 (UI에서 호출) ──────────────────────────────
    public void SelectAugment(int index)
    {
        if (index < 0 || index >= currentChoices.Count) return;
        ApplyAugment(currentChoices[index]);
        Time.timeScale = 1f;
        UIManager.Instance.HideAugmentUI();
    }

    // ─── 증강 적용 ────────────────────────────────────────────
    void ApplyAugment(Augment augment)
    {
        ownedAugments.Add(augment.type);

        switch (augment.type)
        {
            case AugmentType.MultiShot:
                playerShooting.hasMultiShot = true;
                playerShooting.bulletCount  = (int)augment.value;
                break;
            case AugmentType.ChargeShot:
                playerShooting.hasChargeShot    = true;
                playerShooting.chargeMultiplier = augment.value;
                break;
            case AugmentType.DamageUp:
                playerShooting.damage += augment.value;
                break;
            case AugmentType.FireRateUp:
                playerShooting.fireRate *= (1f - augment.value);
                playerShooting.fireRate  = Mathf.Max(0.05f, playerShooting.fireRate);
                break;
            case AugmentType.Piercing:
                playerShooting.hasPiercing = true;
                break;
            case AugmentType.MaxHpUp:
                playerStats.maxHp += augment.value;
                playerStats.Heal(augment.value);
                break;
            case AugmentType.Lifesteal:
                playerStats.lifesteal += augment.value;
                break;
            case AugmentType.Shield:
                playerStats.shieldChance += augment.value;
                break;
        }

        Debug.Log($"증강 적용: {augment.augmentName}");
    }

    bool IsOneTime(AugmentType type)
    {
        return type == AugmentType.MultiShot  ||
               type == AugmentType.ChargeShot ||
               type == AugmentType.Piercing;
    }
}
