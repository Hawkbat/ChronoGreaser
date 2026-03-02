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
        mainMenu.gameObject.SetActive(true);
        optionsMenu.gameObject.SetActive(false);
        creditsMenu.gameObject.SetActive(false);

        if (openCreditsImmediatelyHack)
        {
            openCreditsImmediatelyHack = false;
            mainMenu.gameObject.SetActive(false);
            creditsMenu.gameObject.SetActive(true);
        }

        Save.LoadFile();

        shouldPlayMusic = Save.Instance.didNormalEnding && !skipMusicHack;
        skipMusicHack = false;
    }

    void Update()
    {
        music.SetPlaying(shouldPlayMusic);
    }
}
