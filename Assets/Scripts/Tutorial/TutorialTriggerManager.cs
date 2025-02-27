using UnityEngine;
using System.Collections.Generic;

public class TutorialTriggerManager : MonoBehaviour
{
    private static TutorialTriggerManager _instance;
    public static TutorialTriggerManager Instance => _instance;

    private Dictionary<string, List<TutorialTriggerBase>> contextTriggers = 
        new Dictionary<string, List<TutorialTriggerBase>>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            RegisterTriggers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void RegisterTriggers()
    {
        var triggers = FindObjectsOfType<TutorialTriggerBase>();
        foreach (var trigger in triggers)
        {
            RegisterTrigger(trigger);
        }
    }

    public void RegisterTrigger(TutorialTriggerBase trigger)
    {
        string contextId = trigger.GetType().Name;
        if (!contextTriggers.ContainsKey(contextId))
        {
            contextTriggers[contextId] = new List<TutorialTriggerBase>();
        }
        contextTriggers[contextId].Add(trigger);
    }

    public void EnableTriggersOfType<T>(bool enable) where T : TutorialTriggerBase
    {
        string contextId = typeof(T).Name;
        if (contextTriggers.TryGetValue(contextId, out var triggers))
        {
            foreach (var trigger in triggers)
            {
                trigger.enabled = enable;
            }
        }
    }
} 