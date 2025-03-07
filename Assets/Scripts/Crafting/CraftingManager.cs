using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }

    [Header("Crafting Settings")]
    [SerializeField] private Recipe[] availableRecipes;
    [SerializeField] private int maxSimultaneousCrafts = 5;
    [SerializeField] private float craftingSpeedMultiplier = 1f;
    
    private Dictionary<string, Recipe> recipeMap;
    private List<CraftingOperation> activeCrafts;
    private HashSet<string> unlockedRecipes;
    private Dictionary<string, CraftingStation> craftingStations;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeCrafting();
    }

    private void InitializeCrafting()
    {
        recipeMap = new Dictionary<string, Recipe>();
        activeCrafts = new List<CraftingOperation>();
        unlockedRecipes = new HashSet<string>();
        craftingStations = new Dictionary<string, CraftingStation>();

        foreach (Recipe recipe in availableRecipes)
        {
            recipeMap[recipe.recipeName] = recipe;
        }

        // Load unlocked recipes from save data
        LoadUnlockedRecipes();
    }

    public bool CanCraftRecipe(Recipe recipe, bool checkStation = true)
    {
        if (!unlockedRecipes.Contains(recipe.recipeName))
            return false;

        if (activeCrafts.Count >= maxSimultaneousCrafts)
            return false;

        PlayerProfile profile = GameManager.Instance.PlayerProfile;
        if (profile.level < recipe.requiredLevel)
            return false;

        if (checkStation && recipe.requiresCraftingStation)
        {
            if (!HasAccessToCraftingStation(recipe.craftingStationType))
                return false;
        }

        // Check ingredients
        foreach (var ingredient in recipe.ingredients)
        {
            if (!PlayerInventory.Instance.HasResource(ingredient.resourceType, ingredient.amount))
                return false;
        }

        return true;
    }

    public void StartCrafting(Recipe recipe, System.Action<bool> onComplete = null)
    {
        if (!CanCraftRecipe(recipe))
        {
            onComplete?.Invoke(false);
            return;
        }

        // Consume ingredients
        foreach (var ingredient in recipe.ingredients)
        {
            PlayerInventory.Instance.RemoveResource(ingredient.resourceType, ingredient.amount);
        }

        // Start crafting operation
        CraftingOperation operation = new CraftingOperation
        {
            recipe = recipe,
            progress = 0f,
            onComplete = onComplete
        };

        activeCrafts.Add(operation);
        StartCoroutine(CraftingRoutine(operation));

        // Play effects
        if (recipe.craftingSound != null)
        {
            AudioManager.Instance.PlaySFX(recipe.craftingSound);
        }

        if (recipe.craftingEffectPrefab != null)
        {
            // Spawn effect at crafting station if available, otherwise at player
            Vector3 effectPosition = GetCraftingEffectPosition(recipe.craftingStationType);
            Instantiate(recipe.craftingEffectPrefab, effectPosition, Quaternion.identity);
        }
    }

    private IEnumerator CraftingRoutine(CraftingOperation operation)
    {
        float craftingTime = operation.recipe.craftingTime / craftingSpeedMultiplier;
        float timer = 0f;

        while (timer < craftingTime)
        {
            timer += Time.deltaTime;
            operation.progress = timer / craftingTime;
            
            // Update UI progress if needed
            GameEvents.OnCraftingProgressUpdated?.Invoke(operation.recipe.recipeName, operation.progress);
            
            yield return null;
        }

        CompleteCrafting(operation);
    }

    private void CompleteCrafting(CraftingOperation operation)
    {
        // Add crafted items to inventory
        foreach (var result in operation.recipe.results)
        {
            int amount = result.amount;
            
            // Check for bonus items
            if (result.bonusChance > 0 && Random.value < result.bonusChance)
            {
                amount += result.bonusAmount;
            }

            if (result.item is Tool tool)
            {
                PlayerInventory.Instance.AddTool(tool);
            }
            else if (result.item is Item item)
            {
                PlayerInventory.Instance.AddItem(item, amount);
            }
        }

        // Grant experience
        GameManager.Instance.PlayerProfile.AddExperience(operation.recipe.experienceGained);

        // Play completion effects
        if (operation.recipe.completionSound != null)
        {
            AudioManager.Instance.PlaySFX(operation.recipe.completionSound);
        }

        // Show notification
        NotificationSystem.ShowGeneral(
            "Crafting Complete",
            $"Finished crafting {operation.recipe.recipeName}",
            operation.recipe.icon
        );

        // Remove from active crafts
        activeCrafts.Remove(operation);
        
        // Trigger completion callback
        operation.onComplete?.Invoke(true);
    }

    public void UnlockRecipe(string recipeName)
    {
        if (recipeMap.ContainsKey(recipeName) && !unlockedRecipes.Contains(recipeName))
        {
            unlockedRecipes.Add(recipeName);
            SaveUnlockedRecipes();
            
            // Show notification
            Recipe recipe = recipeMap[recipeName];
            NotificationSystem.ShowGeneral(
                "New Recipe Unlocked!",
                $"You can now craft: {recipe.recipeName}",
                recipe.icon
            );
        }
    }

    public void RegisterCraftingStation(string stationType, CraftingStation station)
    {
        craftingStations[stationType] = station;
    }

    public void UnregisterCraftingStation(string stationType)
    {
        craftingStations.Remove(stationType);
    }

    private bool HasAccessToCraftingStation(string stationType)
    {
        return craftingStations.ContainsKey(stationType) && 
               craftingStations[stationType].IsAccessible();
    }

    private Vector3 GetCraftingEffectPosition(string stationType)
    {
        if (craftingStations.TryGetValue(stationType, out CraftingStation station))
        {
            return station.GetEffectPosition();
        }
        return PlayerController.Instance.transform.position;
    }

    private void LoadUnlockedRecipes()
    {
        string json = PlayerPrefs.GetString("UnlockedRecipes", "");
        if (string.IsNullOrEmpty(json)) return;

        string[] recipes = JsonUtility.FromJson<UnlockedRecipesData>(json).recipes;
        unlockedRecipes = new HashSet<string>(recipes);
    }

    private void SaveUnlockedRecipes()
    {
        var data = new UnlockedRecipesData
        {
            recipes = new string[unlockedRecipes.Count]
        };
        unlockedRecipes.CopyTo(data.recipes);
        
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("UnlockedRecipes", json);
        PlayerPrefs.Save();
    }

    [System.Serializable]
    private class UnlockedRecipesData
    {
        public string[] recipes;
    }

    private class CraftingOperation
    {
        public Recipe recipe;
        public float progress;
        public System.Action<bool> onComplete;
    }
}

// Event for crafting progress updates
public static class CraftingEvents
{
    public static System.Action<string, float> OnCraftingProgressUpdated;
} 