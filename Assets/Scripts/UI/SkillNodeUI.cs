using UnityEngine;
using UnityEngine.UIElements;

public enum SkillNodeState
{
    Locked,
    Available,
    Unlocked
}

public class SkillNodeUI : MonoBehaviour
{
    private UIDocument document;
    private VisualElement iconElement;
    private VisualElement frameElement;
    private VisualElement lockIcon;
    private Label skillNameLabel;
    
    public SkillData SkillData { get; private set; }
    private System.Action<SkillNodeUI> onClickCallback;
    private SkillNodeState currentState;

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        if (document == null)
        {
            Debug.LogError("No UIDocument found on SkillNodeUI!");
            return;
        }

        var root = document.rootVisualElement;
        
        // Get references to UI elements
        iconElement = root.Q<VisualElement>("skill-icon");
        frameElement = root.Q<VisualElement>("skill-frame");
        lockIcon = root.Q<VisualElement>("lock-icon");
        skillNameLabel = root.Q<Label>("skill-name");

        // Set up click handler
        root.RegisterCallback<ClickEvent>(OnNodeClicked);
        root.RegisterCallback<MouseEnterEvent>(OnNodeMouseEnter);
        root.RegisterCallback<MouseLeaveEvent>(OnNodeMouseLeave);
    }

    public void Initialize(SkillData skillData, System.Action<SkillNodeUI> onClick)
    {
        SkillData = skillData;
        onClickCallback = onClick;

        // Set up visual elements
        if (iconElement != null)
        {
            iconElement.style.backgroundImage = new StyleBackground(skillData.icon);
        }
        
        if (skillNameLabel != null)
        {
            skillNameLabel.text = skillData.skillName;
        }
        
        SetState(SkillNodeState.Locked);
    }

    public void SetState(SkillNodeState state)
    {
        currentState = state;
        
        // Remove all state classes first
        frameElement?.RemoveFromClassList("locked");
        frameElement?.RemoveFromClassList("available");
        frameElement?.RemoveFromClassList("unlocked");
        
        iconElement?.RemoveFromClassList("locked");
        iconElement?.RemoveFromClassList("available");
        iconElement?.RemoveFromClassList("unlocked");

        string stateClass = state.ToString().ToLower();
        
        // Add appropriate state class
        frameElement?.AddToClassList(stateClass);
        iconElement?.AddToClassList(stateClass);
        
        if (lockIcon != null)
        {
            lockIcon.style.display = state == SkillNodeState.Locked ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void OnNodeMouseEnter(MouseEnterEvent evt)
    {
        // Show tooltip
        SkillTooltip.Show(SkillData, transform.position);
    }

    private void OnNodeMouseLeave(MouseLeaveEvent evt)
    {
        // Hide tooltip
        SkillTooltip.Hide();
    }

    private void OnNodeClicked(ClickEvent evt)
    {
        if (currentState == SkillNodeState.Available)
        {
            onClickCallback?.Invoke(this);
        }
    }
} 