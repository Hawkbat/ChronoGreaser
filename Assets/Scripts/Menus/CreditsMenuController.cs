using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreditsMenuController : MonoBehaviour
{
    [SerializeField] MainMenuController mainMenu;
    [SerializeField] Button backButton;
    [SerializeField] TextMeshProUGUI stopTimeEndingText;
    [SerializeField] TextMeshProUGUI normalEndingText;

    public bool stopTimeEnding;
    public bool normalEnding;

    void OnEnable()
    {
        backButton.onClick.AddListener(OnBackButtonPressed);
        stopTimeEndingText.gameObject.SetActive(stopTimeEnding);
        normalEndingText.gameObject.SetActive(normalEnding);
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
