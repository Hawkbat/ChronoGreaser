using UnityEngine;

public class StartupMenuController : MonoBehaviour
{
    public static bool openCreditsImmediatelyHack = false;
    public static bool skipMusicHack = false;

    [SerializeField] MainMenuController mainMenu;
    [SerializeField] OptionsMenuController optionsMenu;
    [SerializeField] CreditsMenuController creditsMenu;
    [SerializeField] SoundController music;
    [SerializeField] ParticleSystem stars;

    bool shouldPlayMusic = false;
    ParticleSystem.Particle[] particles;

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

        particles = new ParticleSystem.Particle[stars.main.maxParticles];
    }

    void Update()
    {
        music.SetPlaying(shouldPlayMusic);

        /*
        var particleCount = stars.GetParticles(particles);
        if (TimeLoop.IsRewinding)
        {
            for (int i = 0; i < particleCount; i++)
            {
                var p = particles[i];
                var initialLifetime = p.startLifetime;
                var startTimeOffset = -TimeLoop.CurrentTime;
                particles[i] = p;
            }
        }
        stars.SetParticles(particles, particleCount);
        */
    }
}
