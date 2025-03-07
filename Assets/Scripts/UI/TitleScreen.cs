using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    private UIDocument document;
    private Button startButton;
    private Button optionsButton;
    private Button quitButton;
    private VisualElement optionsPanel;
    private Label titleText;
    private VisualElement playerIcon;
    private Label usernameText;
    private Label levelText;
    private ProgressBar xpProgress;
    private Label versionText;
    private Label buildText;
    
    [Header("Title Animation")]
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float pulseAmount = 0.1f;

    private const string VERSION = "0.1.0";
    private const string BUILD = "Alpha";

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        if (document == null)
        {
            Debug.LogError("No UIDocument found on TitleScreen!");
            return;
        }

        var root = document.rootVisualElement;
        
        // Get references to UI elements
        startButton = root.Q<Button>("Play");
        optionsButton = root.Q<Button>("Options");
        quitButton = root.Q<Button>("Quit");
        optionsPanel = root.Q<VisualElement>("options-panel");
        titleText = root.Q<Label>("title-text");
        playerIcon = root.Q<VisualElement>("player-icon");
        usernameText = root.Q<Label>("username-text");
        levelText = root.Q<Label>("level-text");
        xpProgress = root.Q<ProgressBar>("xp-progress");
        versionText = root.Q<Label>("version-text");
        buildText = root.Q<Label>("build-text");

        SetupButtons();
        if (optionsPanel != null)
            optionsPanel.style.display = DisplayStyle.None;

        // Set up button handlers
        root.Q<Button>("play-button").clicked += StartGame;
        root.Q<Button>("options-button").clicked += ShowOptions;
        root.Q<Button>("quit-button").clicked += QuitGame;

        // Set version info
        versionText.text = $"Version {VERSION}";
        buildText.text = BUILD;

        // Load and display player profile
        LoadPlayerProfile();
    }

    private void Update()
    {
        // Simple pulse animation for title
        if (titleText != null)
        {
            float pulse = 1f + (Mathf.Sin(Time.time * pulseSpeed) * pulseAmount);
            titleText.style.scale = new StyleScale(new Scale(Vector3.one * pulse));
        }
    }

    private void SetupButtons()
    {
        if (startButton != null)
            startButton.clicked += StartGame;
        
        if (optionsButton != null)
            optionsButton.clicked += ToggleOptions;
        
        if (quitButton != null)
            quitButton.clicked += QuitGame;
    }

    private void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void ToggleOptions()
    {
        if (optionsPanel != null)
            optionsPanel.style.display = optionsPanel.style.display.value == DisplayStyle.None ? 
                DisplayStyle.Flex : DisplayStyle.None;
    }

    private void ShowOptions()
    {
        // Create options panel
        var optionsPanel = new OptionsPanel();
        document.rootVisualElement.Add(optionsPanel);
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void LoadPlayerProfile()
    {
        string profileJson = PlayerPrefs.GetString("PlayerProfile", "");
        PlayerProfile profile;

        if (string.IsNullOrEmpty(profileJson))
        {
            // Create new profile if none exists
            profile = PlayerProfile.CreateNew("Player");
            PlayerPrefs.SetString("PlayerProfile", profile.ToJson());
            PlayerPrefs.Save();
        }
        else
        {
            profile = PlayerProfile.FromJson(profileJson);
        }

        // Update UI with profile info
        UpdateProfileUI(profile);
    }

    private void UpdateProfileUI(PlayerProfile profile)
    {
        usernameText.text = profile.username;
        levelText.text = $"Level {profile.level}";
        xpProgress.value = profile.GetLevelProgress() * 100;

        // Set player icon (assuming we have a list of icon sprites)
        if (ResourceManager.TryGetPlayerIcon(profile.playerIconIndex, out Sprite icon))
        {
            playerIcon.style.backgroundImage = new StyleBackground(icon);
        }
    }
} 