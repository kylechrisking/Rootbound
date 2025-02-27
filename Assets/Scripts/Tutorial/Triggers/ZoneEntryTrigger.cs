using UnityEngine;

public class ZoneEntryTrigger : TutorialTriggerBase
{
    [SerializeField] private string zoneId;
    [SerializeField] private Collider2D triggerArea;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerTutorial();
        }
    }
} 