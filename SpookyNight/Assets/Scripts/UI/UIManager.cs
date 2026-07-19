using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI pumpkinText;
    public Slider hpSlider;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI waveAnnounceText;

    [Header("게임오버")]
    public GameObject gameOverPanel;

    [Header("게임 클리어")]
    public GameObject gameClearPanel;
    public TextMeshProUGUI clearTimeText;
    public TextMeshProUGUI clearPumpkinText;
    public TextMeshProUGUI clearKillText;

    [Header("증강 UI")]
    public GameObject augmentPanel;
    public Button[] augmentButtons;            // 버튼 3개
    public TextMeshProUGUI[] augmentNameTexts; // 버튼별 이름
    public TextMeshProUGUI[] augmentDescTexts; // 버튼별 설명
    public Image[] augmentIcons;               // 버튼 아이콘
    public Button rerollButton;

    [Header("상점 UI")]
    public GameObject shopPanel;
    public Button[] shopItemButtons;            // 아이템 슬롯 3개
    public TextMeshProUGUI[] shopItemNameTexts;
    public TextMeshProUGUI[] shopItemCostTexts;
    public Button shopCloseButton;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 증강 버튼
        for (int i = 0; i < augmentButtons.Length; i++)
        {
            int idx = i;
            augmentButtons[i].onClick.AddListener(
                () => AugmentManager.Instance.SelectAugment(idx));
        }

        // 증강 리롤 버튼 ← AugmentManager.Reroll() 호출
        rerollButton.onClick.AddListener(() => AugmentManager.Instance.Reroll());

        // 상점 버튼
        for (int i = 0; i < shopItemButtons.Length; i++)
        {
            int idx = i;
            shopItemButtons[i].onClick.AddListener(() => ShopManager.Instance.BuyItem(idx));
        }

        // 상점 닫기
        shopCloseButton.onClick.AddListener(() => ShopManager.Instance.CloseShop());

        // 패널 끄기
        augmentPanel.SetActive(false);
        shopPanel.SetActive(false);
    }

    // ─── HUD ──────────────────────────────────────────────────
    public void UpdateWave(int wave, int maxWave)
    {
        if (waveText != null)
            waveText.text = $"WAVE {wave} / {maxWave}";
    }

    public void UpdatePumpkin(int count)
    {
        if (pumpkinText != null)
            pumpkinText.text = $"{count}";
    }

    public void UpdateHp(float current, float max)
    {
        if (hpSlider != null) hpSlider.value = current / max;
        if (hpText   != null) hpText.text    = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    public void UpdateAmmo(int current, int max)
    {
        if (ammoText != null)
            ammoText.text = $"{current} / {max}";
    }

    // ─── 웨이브 알림 ──────────────────────────────────────────
    public void ShowWaveAnnounce(int wave)
    {
        StartCoroutine(WaveAnnounceCoroutine(wave));
    }

    IEnumerator WaveAnnounceCoroutine(int wave)
    {
        waveAnnounceText.gameObject.SetActive(true);
        waveAnnounceText.text = wave == 10 ? "BOSS WAVE" : $"WAVE {wave}";
        yield return new WaitForSeconds(2f);
        waveAnnounceText.gameObject.SetActive(false);
    }

    // ─── 게임오버 ─────────────────────────────────────────────
    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        StartCoroutine(WaitForAnyKey());
    }

    // ─── 게임 클리어 ──────────────────────────────────────────
    public void ShowGameClear()
    {
        gameClearPanel.SetActive(true);
        clearTimeText.text    = $"클리어 타임   {GameManager.Instance.GetFormattedTime()}";
        clearPumpkinText.text = $"수집한 호박   {GameManager.Instance.totalPumpkins} 개";
        clearKillText.text    = $"처치한 유령   {GameManager.Instance.totalKills} 마리";
        StartCoroutine(WaitForAnyKey());
    }

    IEnumerator WaitForAnyKey()
    {
        yield return new WaitForSecondsRealtime(1f);
        while (!Input.anyKeyDown)
            yield return null;
        Time.timeScale = 1f;
        GameManager.Instance.GoToMenu();
    }

    // ─── 증강 UI ──────────────────────────────────────────────
    public void ShowAugmentUI(List<Augment> choices)
    {
        augmentPanel.SetActive(true);

        for (int i = 0; i < augmentButtons.Length; i++)
        {
            if (i < choices.Count && choices[i] != null)
            {
                Augment a = choices[i];
                augmentButtons[i].gameObject.SetActive(true);

                // 이름 + 레벨 표시
                if (a.IsOwned)
                    augmentNameTexts[i].text = $"{a.augmentName}  Lv{a.currentLevel} → {a.currentLevel + 1}";
                else
                    augmentNameTexts[i].text = $"{a.augmentName}  NEW";

                // description → GetNextDesc()로 변경
                augmentDescTexts[i].text = a.GetNextDesc();

                // 아이콘
                if (augmentIcons != null && augmentIcons[i] != null)
                {
                    if (a.icon != null)
                    {
                        augmentIcons[i].sprite = a.icon;
                        augmentIcons[i].enabled = true;
                    }
                    else
                    {
                        augmentIcons[i].enabled = false;
                    }
                }
            }
            else
            {
                augmentButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void HideAugmentUI()
    {
        augmentPanel.SetActive(false);
    }

    // ─── 상점 UI ──────────────────────────────────────────────
    public void ShowShopUI(List<ShopManager.ShopItem> items)
    {
        shopPanel.SetActive(true);
        UpdateShopUI(items);
    }

    public void UpdateShopUI(List<ShopManager.ShopItem> items)
    {
        for (int i = 0; i < shopItemButtons.Length; i++)
        {
            if (i < items.Count && items[i] != null)
            {
                shopItemButtons[i].interactable  = true;
                shopItemNameTexts[i].text        = items[i].itemName;
                shopItemCostTexts[i].text        = $"{items[i].cost} 호박";
            }
            else
            {
                shopItemButtons[i].interactable  = false;
                shopItemNameTexts[i].text        = "sold out";
                shopItemCostTexts[i].text        = "-";
            }
        }
    }

    public void HideShopUI()
    {
        shopPanel.SetActive(false);
    }
}
