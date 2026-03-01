using UnityEngine;
using UnityEngine.UI;

public class CreditsMenuController : MonoBehaviour
{
    [SerializeField] MainMenuController mainMenu;
    [SerializeField] Button backButton;

    void OnEnable()
    {
        backButton.onClick.AddListener(OnBackButtonPressed);
    }

    void OnDisable()
    {
        backButton.onClick.RemoveListener(OnBackButtonPressed);
    }

    void OnBackButtonPressed()
    {
        mainMenu.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
