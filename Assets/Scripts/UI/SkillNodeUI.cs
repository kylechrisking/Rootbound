using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public enum SkillNodeState
{
    Locked,
    Available,
    Unlocked
}

public class SkillNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private TextMeshProUGUI skillNameText;
    
    [Header("Colors")]
    [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color unlockedColor = Color.green;

    public SkillData SkillData { get; private set; }
    private System.Action<SkillNodeUI> onClickCallback;
    private SkillNodeState currentState;

    public void Initialize(SkillData skillData, System.Action<SkillNodeUI> onClick)
    {
        SkillData = skillData;
        onClickCallback = onClick;

        // Set up visual elements
        iconImage.sprite = skillData.icon;
        skillNameText.text = skillData.skillName;
        
        SetState(SkillNodeState.Locked);
    }

    public void SetState(SkillNodeState state)
    {
        currentState = state;
        
        switch (state)
        {
            case SkillNodeState.Locked:
                frameImage.color = lockedColor;
                iconImage.color = lockedColor;
                lockIcon.SetActive(true);
                break;

            case SkillNodeState.Available:
                frameImage.color = availableColor;
                iconImage.color = availableColor;
                lockIcon.SetActive(false);
                break;

            case SkillNodeState.Unlocked:
                frameImage.color = unlockedColor;
                iconImage.color = Color.white;
                lockIcon.SetActive(false);
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Show tooltip
        SkillTooltip.Show(SkillData, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Hide tooltip
        SkillTooltip.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentState == SkillNodeState.Available)
        {
            onClickCallback?.Invoke(this);
        }
    }
} 