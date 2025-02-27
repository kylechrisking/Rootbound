using UnityEngine;

public class SkillUnlockTrigger : TutorialTriggerBase
{
    [SerializeField] private string skillId;
    private SkillManager skillManager;

    protected override void Start()
    {
        base.Start();
        skillManager = FindObjectOfType<SkillManager>();
        skillManager.OnSkillUnlocked += OnSkillUnlocked;
    }

    private void OnSkillUnlocked(SkillData skill)
    {
        if (skill.id == skillId)
        {
            TriggerTutorial();
        }
    }

    private void OnDestroy()
    {
        if (skillManager != null)
        {
            skillManager.OnSkillUnlocked -= OnSkillUnlocked;
        }
    }
} 