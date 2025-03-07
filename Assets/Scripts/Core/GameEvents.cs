using UnityEngine;
using System;

public static class GameEvents
{
    // Player Events
    public static Action<int> OnPlayerLevelUp;
    public static Action<ResourceType, float> OnResourceCollected;
    public static Action<string> OnAchievementUnlocked;
    public static Action<string> OnQuestCompleted;
    public static Action<string> OnCritterDiscovered;
    public static Action<int> OnIslandUnlocked;
    
    // Resource Events
    public static Action<ResourceType, float> OnResourceAdded;
    public static Action<ResourceType, float> OnResourceDepleted;
    public static Action<Vector2> OnResourceSpawned;
    
    // Game State Events
    public static Action OnGameSaved;
    public static Action OnGameLoaded;
    public static Action OnDayNightCycleChanged;
    public static Action<bool> OnGamePaused;
    
    // UI Events
    public static Action<string> OnShowTooltip;
    public static Action OnHideTooltip;
    public static Action<string> OnShowPopupMessage;
    
    public static void Reset()
    {
        // Clear all event subscriptions when transitioning between scenes
        OnPlayerLevelUp = null;
        OnResourceCollected = null;
        OnAchievementUnlocked = null;
        OnQuestCompleted = null;
        OnCritterDiscovered = null;
        OnIslandUnlocked = null;
        OnResourceAdded = null;
        OnResourceDepleted = null;
        OnResourceSpawned = null;
        OnGameSaved = null;
        OnGameLoaded = null;
        OnDayNightCycleChanged = null;
        OnGamePaused = null;
        OnShowTooltip = null;
        OnHideTooltip = null;
        OnShowPopupMessage = null;
    }
} 