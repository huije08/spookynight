using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("상점 설정")]
    public int rerollCost = 10;

    private PlayerStats playerStats;
    private List<ShopItem> currentItems = new List<ShopItem>();

    public enum ShopItemType
    {
        HealSmall,
        HealLarge,
        DamageUp,
        FireRateUp,
        MaxHpUp,
    }

    [System.Serializable]
    public class ShopItem
    {
        public string itemName;
        public string description;
        public int cost;
        public ShopItemType type;
        public float value;
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
    }

    // ─── 상점 오픈 ────────────────────────────────────────────
    public void OpenShop()
    {
        Time.timeScale = 0f;
        GenerateShopItems();
        UIManager.Instance.ShowShopUI(currentItems);
        Debug.Log("상점 오픈!");
    }

    // ─── 상점 닫기 ────────────────────────────────────────────
    public void CloseShop()
    {
        Time.timeScale = 1f;
        currentItems.Clear();
        UIManager.Instance.HideShopUI();
        WaveManager.Instance.ResumeAfterShop();
    }

    // ─── 아이템 3개 생성 ──────────────────────────────────────
    void GenerateShopItems()
    {
        currentItems.Clear();
        List<ShopItem> pool = GetItemPool();

        while (currentItems.Count < 3 && pool.Count > 0)
        {
            int index = Random.Range(0, pool.Count);
            currentItems.Add(pool[index]);
            pool.RemoveAt(index);
        }
    }

    List<ShopItem> GetItemPool()
    {
        return new List<ShopItem>
        {
            new ShopItem { itemName = "응급 치료",   description = "HP +30 즉시 회복",       cost = 20, type = ShopItemType.HealSmall,  value = 30f  },
            new ShopItem { itemName = "완전 치료",   description = "HP +60 즉시 회복",       cost = 40, type = ShopItemType.HealLarge,  value = 60f  },
            new ShopItem { itemName = "레이저 강화", description = "데미지 +20",             cost = 30, type = ShopItemType.DamageUp,   value = 20f  },
            new ShopItem { itemName = "냉각 시스템", description = "발사속도 20% 증가",      cost = 30, type = ShopItemType.FireRateUp, value = 0.2f },
            new ShopItem { itemName = "장갑 강화",   description = "최대 HP +50",            cost = 35, type = ShopItemType.MaxHpUp,    value = 50f  },
        };
    }

    // ─── 구매 (UI에서 호출) ───────────────────────────────────
    public bool BuyItem(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= currentItems.Count) return false;
        if (currentItems[itemIndex] == null) return false;

        ShopItem item = currentItems[itemIndex];

        if (playerStats.pumpkins < item.cost)
        {
            Debug.Log("호박 부족!");
            return false;
        }

        playerStats.pumpkins -= item.cost;
        ApplyItem(item);
        currentItems[itemIndex] = null;

        UIManager.Instance.UpdateShopUI(currentItems);
        UIManager.Instance.UpdatePumpkin(playerStats.pumpkins);
        return true;
    }

    // ─── 리롤 ─────────────────────────────────────────────────
    public void Reroll()
    {
        if (playerStats.pumpkins < rerollCost)
        {
            Debug.Log("호박 부족!");
            return;
        }

        playerStats.pumpkins -= rerollCost;
        GenerateShopItems();
        UIManager.Instance.UpdateShopUI(currentItems);
        UIManager.Instance.UpdatePumpkin(playerStats.pumpkins);
    }

    // ─── 효과 적용 ────────────────────────────────────────────
    void ApplyItem(ShopItem item)
    {
        PlayerShooting shooting = FindObjectOfType<PlayerShooting>();

        switch (item.type)
        {
            case ShopItemType.HealSmall:
            case ShopItemType.HealLarge:
                playerStats.Heal(item.value);
                break;
            case ShopItemType.DamageUp:
                if (shooting != null) shooting.damage += item.value;
                break;
            case ShopItemType.FireRateUp:
                if (shooting != null)
                {
                    shooting.fireRate *= (1f - item.value);
                    shooting.fireRate  = Mathf.Max(0.05f, shooting.fireRate);
                }
                break;
            case ShopItemType.MaxHpUp:
                playerStats.maxHp += item.value;
                playerStats.Heal(item.value);
                break;
        }

        Debug.Log($"{item.itemName} 구매 완료! 남은 호박: {playerStats.pumpkins}개");
    }
}
