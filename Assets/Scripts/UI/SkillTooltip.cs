using UnityEngine;
using UnityEngine.UIElements;

public class SkillTooltip : MonoBehaviour
{
    private static SkillTooltip instance;
    private UIDocument document;
    private VisualElement tooltipContainer;
    private Label nameLabel;
    private Label descriptionLabel;
    private Label requirementsLabel;
    private Label costLabel;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        
        document = GetComponent<UIDocument>();
        if (document == null)
        {
            Debug.LogError("No UIDocument found on SkillTooltip!");
            return;
        }

        var root = document.rootVisualElement;
        tooltipContainer = root.Q<VisualElement>("tooltip-container");
        nameLabel = root.Q<Label>("skill-name");
        descriptionLabel = root.Q<Label>("skill-description");
        requirementsLabel = root.Q<Label>("skill-requirements");
        costLabel = root.Q<Label>("skill-cost");
        
        Hide();
    }

    public static void Show(SkillData skill, Vector2 position)
    {
        if (instance == null || instance.tooltipContainer == null) return;

        instance.nameLabel.text = skill.skillName;
        instance.descriptionLabel.text = skill.description;
        
        // Set requirements text
        string reqText = $"Level Required: {skill.levelRequired}";
        if (skill.prerequisiteSkills.Length > 0)
        {
            reqText += "\nRequired Skills: " + string.Join(", ", skill.prerequisiteSkills);
        }
        instance.requirementsLabel.text = reqText;
        
        instance.costLabel.text = $"Cost: {skill.skillPointCost} Skill Points";

        // Position the tooltip
        position += new Vector2(10, 10); // Offset from cursor
        instance.tooltipContainer.style.left = position.x;
        instance.tooltipContainer.style.top = position.y;
        
        instance.tooltipContainer.style.display = DisplayStyle.Flex;
    }

    public static void Hide()
    {
        if (instance == null || instance.tooltipContainer == null) return;
        instance.tooltipContainer.style.display = DisplayStyle.None;
    }
} 