using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SkillTreeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject skillNodePrefab;
    [SerializeField] private RectTransform skillTreeContainer;
    [SerializeField] private TextMeshProUGUI skillPointsText;
    [SerializeField] private GameObject skillTooltip;
    [SerializeField] private LineRenderer skillConnectionPrefab;
    
    [Header("Layout Settings")]
    [SerializeField] private float horizontalSpacing = 200f;
    [SerializeField] private float verticalSpacing = 150f;
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color lockedColor = Color.gray;
    [SerializeField] private Color unlockedColor = Color.green;

    private Dictionary<string, SkillNodeUI> skillNodes = new Dictionary<string, SkillNodeUI>();
    private List<LineRenderer> connectionLines = new List<LineRenderer>();

    private void Start()
    {
        InitializeSkillTree();
        UpdateSkillPoints();
    }

    private void InitializeSkillTree()
    {
        // Clear existing nodes
        foreach (Transform child in skillTreeContainer)
        {
            Destroy(child.gameObject);
        }

        // Get all available skills from SkillManager
        SkillData[] skills = SkillManager.Instance.GetAvailableSkills();

        // Create nodes for each skill
        foreach (SkillData skill in skills)
        {
            CreateSkillNode(skill);
        }

        // Create connections between nodes
        CreateSkillConnections();

        // Update all node states
        UpdateAllNodeStates();
    }

    private void CreateSkillNode(SkillData skill)
    {
        GameObject nodeObj = Instantiate(skillNodePrefab, skillTreeContainer);
        SkillNodeUI node = nodeObj.GetComponent<SkillNodeUI>();
        
        node.Initialize(skill, OnSkillNodeClicked);
        skillNodes.Add(skill.skillName, node);

        // Position the node based on its tier and position in the tree
        // This is a simple layout - you might want to customize this
        int tier = GetSkillTier(skill);
        int position = GetSkillPositionInTier(skill, tier);
        
        nodeObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(
            tier * horizontalSpacing,
            position * verticalSpacing
        );
    }

    private void CreateSkillConnections()
    {
        foreach (var node in skillNodes.Values)
        {
            foreach (string prereqName in node.SkillData.prerequisiteSkills)
            {
                if (skillNodes.TryGetValue(prereqName, out SkillNodeUI prereqNode))
                {
                    CreateConnectionLine(prereqNode.transform.position, node.transform.position);
                }
            }
        }
    }

    private void CreateConnectionLine(Vector3 start, Vector3 end)
    {
        LineRenderer line = Instantiate(skillConnectionPrefab, skillTreeContainer);
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        connectionLines.Add(line);
    }

    private void UpdateAllNodeStates()
    {
        foreach (var node in skillNodes.Values)
        {
            UpdateNodeState(node);
        }
    }

    private void UpdateNodeState(SkillNodeUI node)
    {
        if (SkillManager.Instance.IsSkillUnlocked(node.SkillData.skillName))
        {
            node.SetState(SkillNodeState.Unlocked);
        }
        else if (SkillManager.Instance.CanUnlockSkill(node.SkillData.skillName))
        {
            node.SetState(SkillNodeState.Available);
        }
        else
        {
            node.SetState(SkillNodeState.Locked);
        }
    }

    private void OnSkillNodeClicked(SkillNodeUI node)
    {
        if (SkillManager.Instance.UnlockSkill(node.SkillData.skillName))
        {
            UpdateAllNodeStates();
            UpdateSkillPoints();
        }
    }

    private void UpdateSkillPoints()
    {
        skillPointsText.text = $"Skill Points: {SkillManager.Instance.GetCurrentSkillPoints()}";
    }

    private int GetSkillTier(SkillData skill)
    {
        // Simple tier calculation based on prerequisites
        // You might want to customize this based on your skill tree structure
        if (skill.prerequisiteSkills.Length == 0) return 0;
        
        int maxTier = 0;
        foreach (string prereq in skill.prerequisiteSkills)
        {
            if (skillNodes.TryGetValue(prereq, out SkillNodeUI prereqNode))
            {
                int prereqTier = GetSkillTier(prereqNode.SkillData);
                maxTier = Mathf.Max(maxTier, prereqTier + 1);
            }
        }
        return maxTier;
    }

    private int GetSkillPositionInTier(SkillData skill, int tier)
    {
        // Simple position calculation
        // You might want to customize this based on your skill tree layout
        int position = 0;
        foreach (var otherSkill in skillNodes.Values)
        {
            if (GetSkillTier(otherSkill.SkillData) == tier)
            {
                position++;
            }
        }
        return position;
    }
} 