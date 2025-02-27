using UnityEngine;
using System.IO;
using System;

public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SaveManager>();
            }
            return _instance;
        }
    }

    private string SavePath => Path.Combine(Application.persistentDataPath, "rootbound.save");
    private GameData currentGameData;
    public bool HasSaveData => File.Exists(SavePath);

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            currentGameData = new GameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        // Update save data from game state
        UpdateGameData();

        // Serialize and save
        string jsonData = JsonUtility.ToJson(currentGameData, true);
        File.WriteAllText(SavePath, jsonData);
        Debug.Log($"Game saved to: {SavePath}");
    }

    public bool LoadGame()
    {
        if (!HasSaveData) return false;

        try
        {
            string jsonData = File.ReadAllText(SavePath);
            currentGameData = JsonUtility.FromJson<GameData>(jsonData);
            ApplyGameData();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading save file: {e.Message}");
            return false;
        }
    }

    private void UpdateGameData()
    {
        // Get current game state
        PlayerStats playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats != null)
        {
            currentGameData.playerGrowthLevel = playerStats.GetGrowthLevel();
            currentGameData.resourceLevels[0] = playerStats.GetWaterLevel();
            currentGameData.resourceLevels[1] = playerStats.GetNutrientLevel();
            currentGameData.resourceLevels[2] = playerStats.GetSunlightLevel();
            currentGameData.playerPosition = playerStats.transform.position;
        }

        currentGameData.lastSaveTime = DateTime.Now;
        // Add more game state data here
    }

    private void ApplyGameData()
    {
        // Apply loaded data to game state
        PlayerStats playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.SetGrowthLevel(currentGameData.playerGrowthLevel);
            playerStats.SetResourceLevels(currentGameData.resourceLevels);
            playerStats.transform.position = currentGameData.playerPosition;
        }
        // Apply more game state data here
    }

    public GameData GetCurrentGameData()
    {
        return currentGameData;
    }

    public void DeleteSaveData()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            currentGameData = new GameData();
            Debug.Log("Save data deleted");
        }
    }
} 