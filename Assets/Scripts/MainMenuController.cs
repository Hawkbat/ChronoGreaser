using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] OptionsMenuController optionsMenu;
    [SerializeField] CreditsMenuController creditsMenu;
    [SerializeField] Button startButton;
    [SerializeField] Button optionsButton;
    [SerializeField] Button creditsButton;
    [SerializeField] Button quitButton;

    private void OnEnable()
    {
        startButton.onClick.AddListener(OnStartButtonPressed);
        optionsButton.onClick.AddListener(OnOptionsButtonPressed);
        creditsButton.onClick.AddListener(OnCreditsButtonPressed);
        quitButton.onClick.AddListener(OnQuitButtonPressed);
    }

    private void OnDisable()
    {
        startButton.onClick.RemoveListener(OnStartButtonPressed);
        optionsButton.onClick.RemoveListener(OnOptionsButtonPressed);
        creditsButton.onClick.RemoveListener(OnCreditsButtonPressed);
        quitButton.onClick.RemoveListener(OnQuitButtonPressed);
    }

    void OnStartButtonPressed()
    {
        SceneManager.LoadScene("Game");
    }

    void OnOptionsButtonPressed()
    {
        optionsMenu.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    void OnCreditsButtonPressed()
    {
        creditsMenu.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    void OnQuitButtonPressed()
    {
        Application.Quit();
    }
}
