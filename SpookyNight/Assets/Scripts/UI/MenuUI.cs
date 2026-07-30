using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    public Button startButton;
    public Button exitButton;
    public Button settingButton;

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
    }

    void OnSettingClicked()
    {
        
    }
}
