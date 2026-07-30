using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    public Slider loadingBar;
    public TMP_Text loadingText;
    public TMP_Text tipText;
    public TMP_Text percentText;
    bool readyToEnter = false;

    string[] tips =
    {
        "플래시를 켜면 적을 멈출수있습니다",
        "항상 뒤를 조심하세요",
        "시간이 지날수록 강한적들이 등장합니다",
        "체력이 낮을 땐 무리하지 마세요",
        "세번째 웨이브마다 상점이 등장합니다",
        "십자가를 사용하면 체력을 모두 회복합니다",
        "촛불을 주변에선 공격력이 증가합니다",
        "적이 많을땐 소금을 던져보세요"
    };

    void Start()
    {
        // 랜덤 팁 설정
        tipText.text = "TIPs: " + tips[Random.Range(0, tips.Length)];

        StartCoroutine(LoadAsync());
        StartCoroutine(LoadingDotAnimation());
    }

    IEnumerator LoadAsync()
    {
        string sceneName = PlayerPrefs.GetString("NextScene");

        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        while (async.progress < 0.9f)
        {
            float progress = async.progress;

            // 로딩바
            loadingBar.value = progress;

            // 퍼센트 표시
            percentText.text = Mathf.RoundToInt(progress * 100f) + "%";

            yield return null;
        }

        // 100% 처리
        loadingBar.value = 1f;
        percentText.text = "100%";

        // 완료 상태
        readyToEnter = true;
        loadingText.text = "PRESS ANY KEY";
    }

    IEnumerator LoadingDotAnimation()
    {
        int dotCount = 1;

        while (!readyToEnter)
        {
            string dots = new string('.', dotCount);
            loadingText.text = "Now Loading" + dots;

            dotCount++;
            if (dotCount > 3) dotCount = 1;

            yield return new WaitForSeconds(0.4f);
        }
    }

    void Update()
    {
        if (readyToEnter && Input.anyKeyDown)
        {
            SceneManager.LoadScene(PlayerPrefs.GetString("NextScene"));
        }
    }
}