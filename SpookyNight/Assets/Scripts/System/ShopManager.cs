using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    private PlayerStats playerStats;
    private List<ShopItem> shopItems = new List<ShopItem>();

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
        public bool isSoldOut = false;
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        InitShopItems();
    }

    // 상점 아이템 5개 고정
    void InitShopItems()
    {
        shopItems = new List<ShopItem>
        {
            new ShopItem { itemName = "응급 치료",   description = "HP +30 즉시 회복",  cost = 20, type = ShopItemType.HealSmall,  value = 30f  },
            new ShopItem { itemName = "완전 치료",   description = "HP +60 즉시 회복",  cost = 40, type = ShopItemType.HealLarge,  value = 60f  },
            new ShopItem { itemName = "레이저 강화", description = "데미지 +20",        cost = 30, type = ShopItemType.DamageUp,   value = 20f  },
            new ShopItem { itemName = "냉각 시스템", description = "발사속도 20% 증가", cost = 30, type = ShopItemType.FireRateUp, value = 0.2f },
            new ShopItem { itemName = "장갑 강화",   description = "최대 HP +50",       cost = 35, type = ShopItemType.MaxHpUp,    value = 50f  },
        };
    }

    // 상점 오픈
    public void OpenShop()
    {
        Time.timeScale = 0f;

        // 상점 열릴 때마다 품절 초기화
        foreach (var item in shopItems)
            item.isSoldOut = false;

        UIManager.Instance.ShowShopUI(shopItems);
        Debug.Log("상점 오픈!");
    }

    // 상점 닫기
    public void CloseShop()
    {
        Time.timeScale = 1f;
        UIManager.Instance.HideShopUI();
        WaveManager.Instance.ResumeAfterShop();
    }

    // 구매 (UI 버튼에서 호출)
    public bool BuyItem(int index)
    {
        if (index < 0 || index >= shopItems.Count) return false;

        ShopItem item = shopItems[index];

        if (item.isSoldOut)
        {
            Debug.Log("품절된 아이템!");
            return false;
        }

        if (playerStats.pumpkins < item.cost)
        {
            Debug.Log("호박 부족!");
            return false;
        }

        playerStats.pumpkins -= item.cost;
        ApplyItem(item);
        item.isSoldOut = true;

        UIManager.Instance.UpdateShopUI(shopItems);
        UIManager.Instance.UpdatePumpkin(playerStats.pumpkins);
        return true;
    }

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
                    shooting.fireRate = Mathf.Max(0.05f, shooting.fireRate);
                }
                break;
            case ShopItemType.MaxHpUp:
                playerStats.maxHp += item.value;
                playerStats.Heal(item.value);
                break;
        }

        Debug.Log($"{item.itemName} 구매! 남은 호박: {playerStats.pumpkins}개");
    }
}