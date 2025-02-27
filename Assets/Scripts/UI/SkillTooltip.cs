using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkillTooltip : MonoBehaviour
{
    private static SkillTooltip instance;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI requirementsText;
    [SerializeField] private TextMeshProUGUI costText;
    
    [Header("Layout Settings")]
    [SerializeField] private Vector2 offset = new Vector2(20f, 20f);
    [SerializeField] private float padding = 20f;
    
    [Header("Colors")]
    [SerializeField] private Color requirementMetColor = Color.green;
    [SerializeField] private Color requirementNotMetColor = Color.red;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        instance = this;
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        Hide();
    }

    public static void Show(SkillData skill, Vector3 position)
    {
        if (instance == null) return;
        instance.ShowTooltip(skill, position);
    }

    public static void Hide()
    {
        if (instance == null) return;
        instance.HideTooltip();
    }

    private void ShowTooltip(SkillData skill, Vector3 position)
    {
        // Set position
        transform.position = position + (Vector3)offset;

        // Set name and description
        skillNameText.text = skill.skillName;
        descriptionText.text = skill.description;

        // Set requirements
        string requirements = $"Level Required: {skill.levelRequired}";
        if (skill.prerequisiteSkills.Length > 0)
        {
            requirements += "\nRequired Skills:";
            foreach (string prereq in skill.prerequisiteSkills)
            {
                bool isUnlocked = SkillManager.Instance.IsSkillUnlocked(prereq);
                requirements += $"\n• <color={(isUnlocked ? requirementMetColor : requirementNotMetColor)}>{prereq}</color>";
            }
        }
        requirementsText.text = requirements;

        // Set cost
        costText.text = $"Cost: {skill.skillPointCost} Skill Points";

        // Show tooltip
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Ensure tooltip stays within screen bounds
        ClampToScreen();
    }

    private void HideTooltip()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    private void ClampToScreen()
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector3[] canvasCorners = new Vector3[4];
        canvasRect.GetWorldCorners(canvasCorners);

        float minX = canvasCorners[0].x + padding;
        float maxX = canvasCorners[2].x - padding;
        float minY = canvasCorners[0].y + padding;
        float maxY = canvasCorners[2].y - padding;

        Vector3 position = transform.position;

        if (corners[0].x < minX) position.x += minX - corners[0].x;
        if (corners[2].x > maxX) position.x -= corners[2].x - maxX;
        if (corners[0].y < minY) position.y += minY - corners[0].y;
        if (corners[2].y > maxY) position.y -= corners[2].y - maxY;

        transform.position = position;
    }
} 