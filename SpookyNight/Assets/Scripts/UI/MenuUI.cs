using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    public Button startButton;
    public Button exitButton;

    void Start()
    {
        startButton.onClick.AddListener(OnStartClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    void OnStartClicked()
    {
        GameManager.Instance.StartGame();
    }

    void OnExitClicked()
    {
        Application.Quit();
        Debug.Log("게임 종료");
    }
}
