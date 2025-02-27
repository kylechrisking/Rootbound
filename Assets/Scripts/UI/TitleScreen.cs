using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleScreen : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject optionsPanel;
    
    [Header("Title Animation")]
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float pulseAmount = 0.1f;
    [SerializeField] private TextMeshProUGUI titleText;

    private void Start()
    {
        SetupButtons();
        optionsPanel.SetActive(false);
    }

    private void Update()
    {
        // Simple pulse animation for title
        if (titleText != null)
        {
            float pulse = 1f + (Mathf.Sin(Time.time * pulseSpeed) * pulseAmount);
            titleText.transform.localScale = Vector3.one * pulse;
        }
    }

    private void SetupButtons()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
        
        if (optionsButton != null)
            optionsButton.onClick.AddListener(ToggleOptions);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    private void StartGame()
    {
        SceneController.Instance.LoadScene("GameScene");
    }

    private void ToggleOptions()
    {
        optionsPanel.SetActive(!optionsPanel.activeSelf);
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
} 