using UnityEngine;

public class GameSettings : MonoBehaviour
{
    private static GameSettings _instance;
    public static GameSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameSettings>();
            }
            return _instance;
        }
    }

    [System.Serializable]
    public class Settings
    {
        public float masterVolume = 1f;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        public bool fullscreen = true;
        public int qualityLevel = 2; // 0: Low, 1: Medium, 2: High
    }

    public Settings currentSettings = new Settings();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveSettings()
    {
        string settingsJson = JsonUtility.ToJson(currentSettings);
        PlayerPrefs.SetString("GameSettings", settingsJson);
        PlayerPrefs.Save();
        
        // Apply settings
        ApplySettings();
    }

    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey("GameSettings"))
        {
            string settingsJson = PlayerPrefs.GetString("GameSettings");
            currentSettings = JsonUtility.FromJson<Settings>(settingsJson);
        }
        
        ApplySettings();
    }

    private void ApplySettings()
    {
        // Apply audio settings
        AudioManager.Instance?.SetMasterVolume(currentSettings.masterVolume);
        
        // Apply display settings
        Screen.fullScreen = currentSettings.fullscreen;
        QualitySettings.SetQualityLevel(currentSettings.qualityLevel);
    }
} 