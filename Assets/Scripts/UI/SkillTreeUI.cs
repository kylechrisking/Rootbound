using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class SkillTreeUI : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private float horizontalSpacing = 200f;
    [SerializeField] private float verticalSpacing = 150f;
    
    private UIDocument document;
    private VisualElement skillTreeContainer;
    private Label skillPointsLabel;
    private Dictionary<string, SkillNodeUI> skillNodes = new Dictionary<string, SkillNodeUI>();

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        if (document == null)
        {
            Debug.LogError("No UIDocument found on SkillTreeUI!");
            return;
        }

        var root = document.rootVisualElement;
        
        // Get references to UI elements
        skillTreeContainer = root.Q<VisualElement>("skill-tree-container");
        skillPointsLabel = root.Q<Label>("skill-points-text");
    }

    private void Start()
    {
        InitializeSkillTree();
        UpdateSkillPoints();
    }

    private void InitializeSkillTree()
    {
        // Clear existing nodes
        skillTreeContainer.Clear();
        skillNodes.Clear();

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
        // Create skill node template
        TemplateContainer nodeTemplate = new TemplateContainer();
        nodeTemplate.name = $"skill-node-{skill.skillName}";
        nodeTemplate.AddToClassList("skill-node");
        
        var node = nodeTemplate.Q<SkillNodeUI>();
        if (node != null)
        {
            node.Initialize(skill, OnSkillNodeClicked);
            skillNodes.Add(skill.skillName, node);

            // Position the node
            int tier = GetSkillTier(skill);
            int position = GetSkillPositionInTier(skill, tier);
            
            nodeTemplate.style.left = tier * horizontalSpacing;
            nodeTemplate.style.top = position * verticalSpacing;
            
            skillTreeContainer.Add(nodeTemplate);
        }
    }

    private void CreateSkillConnections()
    {
        foreach (var node in skillNodes.Values)
        {
            foreach (string prereqName in node.SkillData.prerequisiteSkills)
            {
                if (skillNodes.TryGetValue(prereqName, out SkillNodeUI prereqNode))
                {
                    CreateConnectionLine(prereqNode, node);
                }
            }
        }
    }

    private void CreateConnectionLine(SkillNodeUI start, SkillNodeUI end)
    {
        var connection = new VisualElement();
        connection.AddToClassList("skill-connection");
        
        // Calculate connection position and rotation
        Vector2 startPos = start.worldBound.center;
        Vector2 endPos = end.worldBound.center;
        float angle = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x) * Mathf.Rad2Deg;
        float distance = Vector2.Distance(startPos, endPos);

        connection.style.width = distance;
        connection.style.rotate = new StyleRotate(new Rotate(angle));
        connection.style.left = startPos.x;
        connection.style.top = startPos.y;
        
        skillTreeContainer.Add(connection);
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
        if (skillPointsLabel != null)
        {
            skillPointsLabel.text = $"Skill Points: {SkillManager.Instance.GetCurrentSkillPoints()}";
        }
    }

    private int GetSkillTier(SkillData skill)
    {
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