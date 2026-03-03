using UnityEngine;

public class StartupMenuController : MonoBehaviour
{
    public static bool openCreditsImmediatelyHack = false;
    public static bool skipMusicHack = false;

    [SerializeField] MainMenuController mainMenu;
    [SerializeField] OptionsMenuController optionsMenu;
    [SerializeField] CreditsMenuController creditsMenu;
    [SerializeField] SoundController music;

    bool shouldPlayMusic = false;

    void Awake()
    {
        Save.LoadFile();

        creditsMenu.stopTimeEnding = Save.Instance.didTimeStopEnding;
        creditsMenu.normalEnding = Save.Instance.didNormalEnding;

        mainMenu.gameObject.SetActive(true);
        optionsMenu.gameObject.SetActive(false);
        creditsMenu.gameObject.SetActive(false);

        if (openCreditsImmediatelyHack)
        {
            openCreditsImmediatelyHack = false;
            mainMenu.gameObject.SetActive(false);
            creditsMenu.gameObject.SetActive(true);
        }

        shouldPlayMusic = Save.Instance.didNormalEnding && !skipMusicHack;
        skipMusicHack = false;
    }

    void Update()
    {
        music.SetPlaying(shouldPlayMusic);
    }
}
