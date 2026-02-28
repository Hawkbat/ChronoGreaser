using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuController : MonoBehaviour
{
    [SerializeField] MainMenuController mainMenu;
    [SerializeField] Slider mainVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider sfxVolumeSlider;
    [SerializeField] Slider lookSensitivityXSlider;
    [SerializeField] Slider lookSensitivityYSlider;
    [SerializeField] Button backButton;

    void OnEnable()
    {
        Save.LoadFile();
        mainVolumeSlider.value = Save.Instance.masterVolume;
        musicVolumeSlider.value = Save.Instance.musicVolume;
        sfxVolumeSlider.value = Save.Instance.sfxVolume;
        lookSensitivityXSlider.value = Save.Instance.cameraSensitivityX;
        lookSensitivityYSlider.value = Save.Instance.cameraSensitivityY;
        mainVolumeSlider.onValueChanged.AddListener(OnMainVolumeSliderChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeSliderChanged);
        lookSensitivityXSlider.onValueChanged.AddListener(OnLookSensitivityXSliderChanged);
        lookSensitivityYSlider.onValueChanged.AddListener(OnLookSensitivityYSliderChanged);
        backButton.onClick.AddListener(OnBackButtonPressed);
    }

    void OnDisable()
    {
        mainVolumeSlider.onValueChanged.RemoveListener(OnMainVolumeSliderChanged);
        musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeSliderChanged);
        sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeSliderChanged);
        lookSensitivityXSlider.onValueChanged.RemoveListener(OnLookSensitivityXSliderChanged);
        lookSensitivityYSlider.onValueChanged.RemoveListener(OnLookSensitivityYSliderChanged);
        backButton.onClick.RemoveListener(OnBackButtonPressed);
    }

    void OnMainVolumeSliderChanged(float value)
    {
        Save.Instance.masterVolume = value;
        Save.SaveFile();
    }

    void OnMusicVolumeSliderChanged(float value)
    {
        Save.Instance.musicVolume = value;
        Save.SaveFile();
    }

    void OnSFXVolumeSliderChanged(float value)
    {
        Save.Instance.sfxVolume = value;
        Save.SaveFile();
    }

    void OnLookSensitivityXSliderChanged(float value)
    {
        Save.Instance.cameraSensitivityX = value;
        Save.SaveFile();
    }

    void OnLookSensitivityYSliderChanged(float value)
    {
        Save.Instance.cameraSensitivityY = value;
        Save.SaveFile();
    }

    void OnBackButtonPressed()
    {
        mainMenu.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
