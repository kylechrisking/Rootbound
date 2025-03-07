using UnityEngine;

[System.Serializable]
public class Achievement
{
    public string id;
    public string name;
    public string description;
    public Sprite icon;
    
    public bool isCompleted;
    public bool isLocked;
    public float progress;
    public float targetProgress = 1f;
    
    public System.Action<Achievement> onProgressUpdated;
    public System.Action<Achievement> onCompleted;

    public void UpdateProgress(float newProgress)
    {
        progress = Mathf.Clamp(newProgress, 0f, targetProgress);
        onProgressUpdated?.Invoke(this);

        if (progress >= targetProgress && !isCompleted)
        {
            Complete();
        }
    }

    public void Complete()
    {
        isCompleted = true;
        progress = targetProgress;
        onCompleted?.Invoke(this);
        
        // Show achievement notification
        NotificationSystem.ShowAchievement(
            "Achievement Unlocked!",
            name,
            icon
        );
    }

    public void Unlock()
    {
        isLocked = false;
        onProgressUpdated?.Invoke(this);
    }
} 