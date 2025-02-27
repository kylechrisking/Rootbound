using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsPanel : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    
    [Header("Display Settings")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    
    [Header("Buttons")]
    [SerializeField] private Button applyButton;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        SetupUI();
        LoadCurrentSettings();
    }

    private void SetupUI()
    {
        // Setup listeners
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        
        applyButton.onClick.AddListener(SaveSettings);
        closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    private void LoadCurrentSettings()
    {
        var settings = GameSettings.Instance.currentSettings;
        
        masterVolumeSlider.value = settings.masterVolume;
        musicVolumeSlider.value = settings.musicVolume;
        sfxVolumeSlider.value = settings.sfxVolume;
        
        fullscreenToggle.isOn = settings.fullscreen;
        qualityDropdown.value = settings.qualityLevel;
    }

    private void OnMasterVolumeChanged(float value)
    {
        GameSettings.Instance.currentSettings.masterVolume = value;
    }

    private void OnMusicVolumeChanged(float value)
    {
        GameSettings.Instance.currentSettings.musicVolume = value;
    }

    private void OnSFXVolumeChanged(float value)
    {
        GameSettings.Instance.currentSettings.sfxVolume = value;
    }

    private void OnFullscreenChanged(bool isFullscreen)
    {
        GameSettings.Instance.currentSettings.fullscreen = isFullscreen;
    }

    private void OnQualityChanged(int qualityIndex)
    {
        GameSettings.Instance.currentSettings.qualityLevel = qualityIndex;
    }

    private void SaveSettings()
    {
        GameSettings.Instance.SaveSettings();
    }
} 