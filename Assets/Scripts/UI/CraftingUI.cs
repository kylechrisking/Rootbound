using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class CraftingUI : MonoBehaviour
{
    private UIDocument document;
    private VisualElement root;
    private Label stationName;
    private Button closeButton;
    private ScrollView recipeList;
    private VisualElement recipeDetails;
    private SliderInt craftAmount;
    private Button craftButton;
    private ScrollView craftingQueue;

    private string currentCategory = "All";
    private Recipe selectedRecipe;
    private Dictionary<string, VisualElement> queueEntries = new Dictionary<string, VisualElement>();

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        if (document == null)
        {
            Debug.LogError("No UIDocument found on CraftingUI!");
            return;
        }

        root = document.rootVisualElement.Q<VisualElement>("crafting-panel");
        SetupUI();
    }

    private void OnEnable()
    {
        CraftingEvents.OnCraftingProgressUpdated += UpdateCraftingProgress;
    }

    private void OnDisable()
    {
        CraftingEvents.OnCraftingProgressUpdated -= UpdateCraftingProgress;
    }

    private void SetupUI()
    {
        // Get references
        stationName = root.Q<Label>("station-name");
        closeButton = root.Q<Button>("close-button");
        recipeList = root.Q<ScrollView>("recipe-list");
        recipeDetails = root.Q<VisualElement>("recipe-details");
        craftAmount = root.Q<SliderInt>("craft-amount");
        craftButton = root.Q<Button>("craft-button");
        craftingQueue = root.Q<ScrollView>("crafting-queue");

        // Setup event handlers
        closeButton.clicked += Hide;
        craftButton.clicked += StartCrafting;
        
        // Setup craft amount slider
        craftAmount.lowValue = 1;
        craftAmount.highValue = 99;
        craftAmount.value = 1;
        
        // Initially hide recipe details
        recipeDetails.style.display = DisplayStyle.None;
    }

    public void Show(string stationType)
    {
        root.style.display = DisplayStyle.Flex;
        stationName.text = $"{stationType} Station";
        
        // Load recipes for this station type
        LoadRecipes(stationType);
        
        // Reset UI state
        selectedRecipe = null;
        recipeDetails.style.display = DisplayStyle.None;
        currentCategory = "All";
        UpdateCategoryButtons();
    }

    public void Hide()
    {
        root.style.display = DisplayStyle.None;
    }

    private void LoadRecipes(string stationType)
    {
        recipeList.Clear();
        
        // Get all recipes for this station
        var recipes = Resources.LoadAll<Recipe>("Recipes")
            .Where(r => r.craftingStationType == stationType)
            .OrderBy(r => r.sortOrder);

        // Create category buttons
        var categories = recipes.SelectMany(r => r.categories)
            .Distinct()
            .OrderBy(c => c);

        var categoryList = root.Q<VisualElement>("category-list");
        categoryList.Clear();
        
        // Add "All" category
        var allButton = new Button(() => SelectCategory("All")) { text = "All" };
        allButton.AddToClassList("category-button");
        allButton.AddToClassList("selected");
        categoryList.Add(allButton);

        // Add other categories
        foreach (var category in categories)
        {
            var button = new Button(() => SelectCategory(category)) { text = category };
            button.AddToClassList("category-button");
            categoryList.Add(button);
        }

        // Add recipe entries
        foreach (var recipe in recipes)
        {
            var entry = CreateRecipeEntry(recipe);
            recipeList.Add(entry);
        }
    }

    private VisualElement CreateRecipeEntry(Recipe recipe)
    {
        var entry = new VisualElement();
        entry.AddToClassList("recipe-entry");
        
        var icon = new VisualElement();
        icon.AddToClassList("recipe-icon");
        icon.style.backgroundImage = new StyleBackground(recipe.icon);
        
        var info = new VisualElement();
        info.AddToClassList("recipe-info");
        
        var name = new Label(recipe.recipeName);
        name.AddToClassList("recipe-name");
        
        info.Add(name);
        entry.Add(icon);
        entry.Add(info);

        // Add click handler
        entry.RegisterCallback<ClickEvent>(evt => SelectRecipe(recipe, entry));

        return entry;
    }

    private void SelectCategory(string category)
    {
        currentCategory = category;
        UpdateCategoryButtons();
        
        // Update recipe visibility
        foreach (var element in recipeList.Children())
        {
            var entry = element as VisualElement;
            var recipe = entry.userData as Recipe;
            
            bool show = category == "All" || recipe.categories.Contains(category);
            entry.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void UpdateCategoryButtons()
    {
        var categoryList = root.Q<VisualElement>("category-list");
        foreach (var button in categoryList.Children())
        {
            if (button is Button categoryButton)
            {
                if (categoryButton.text == currentCategory)
                    categoryButton.AddToClassList("selected");
                else
                    categoryButton.RemoveFromClassList("selected");
            }
        }
    }

    private void SelectRecipe(Recipe recipe, VisualElement entry)
    {
        // Update selection visual
        recipeList.Query<VisualElement>(null, "recipe-entry")
            .ForEach(e => e.RemoveFromClassList("selected"));
        entry.AddToClassList("selected");

        selectedRecipe = recipe;
        UpdateRecipeDetails();
    }

    private void UpdateRecipeDetails()
    {
        if (selectedRecipe == null)
        {
            recipeDetails.style.display = DisplayStyle.None;
            return;
        }

        recipeDetails.style.display = DisplayStyle.Flex;

        // Update recipe info
        recipeDetails.Q<VisualElement>("recipe-icon").style.backgroundImage = 
            new StyleBackground(selectedRecipe.icon);
        recipeDetails.Q<Label>("recipe-name").text = selectedRecipe.recipeName;
        recipeDetails.Q<Label>("recipe-description").text = selectedRecipe.description;

        // Update ingredients
        var ingredientsList = recipeDetails.Q<VisualElement>("ingredients-list");
        ingredientsList.Clear();
        
        foreach (var ingredient in selectedRecipe.ingredients)
        {
            var entry = CreateIngredientEntry(ingredient);
            ingredientsList.Add(entry);
        }

        // Update results
        var resultsList = recipeDetails.Q<VisualElement>("results-list");
        resultsList.Clear();
        
        foreach (var result in selectedRecipe.results)
        {
            var entry = CreateResultEntry(result);
            resultsList.Add(entry);
        }

        // Update craft button state
        UpdateCraftButton();
    }

    private VisualElement CreateIngredientEntry(ResourceRequirement ingredient)
    {
        var entry = new VisualElement();
        entry.AddToClassList("ingredient-entry");

        var resourceData = ResourceManager.Instance.GetResourceData(ingredient.resourceType);
        
        var icon = new VisualElement();
        icon.AddToClassList("ingredient-icon");
        icon.style.backgroundImage = new StyleBackground(resourceData.icon);

        var name = new Label(resourceData.displayName);
        name.AddToClassList("ingredient-name");

        var amount = new Label($"{ingredient.amount}");
        amount.AddToClassList("ingredient-amount");
        
        // Check if player has enough
        bool hasEnough = PlayerInventory.Instance.HasResource(ingredient.resourceType, ingredient.amount);
        amount.AddToClassList(hasEnough ? "sufficient" : "insufficient");

        entry.Add(icon);
        entry.Add(name);
        entry.Add(amount);

        return entry;
    }

    private VisualElement CreateResultEntry(CraftingResult result)
    {
        var entry = new VisualElement();
        entry.AddToClassList("result-entry");

        Sprite icon = null;
        string name = "";

        if (result.item is Tool tool)
        {
            icon = tool.icon;
            name = tool.toolName;
        }
        else if (result.item is Item item)
        {
            icon = item.icon;
            name = item.itemName;
        }

        var iconElement = new VisualElement();
        iconElement.AddToClassList("result-icon");
        iconElement.style.backgroundImage = new StyleBackground(icon);

        var nameLabel = new Label(name);
        nameLabel.AddToClassList("result-name");

        var amountLabel = new Label(result.amount > 1 ? $"x{result.amount}" : "");
        amountLabel.AddToClassList("result-amount");

        entry.Add(iconElement);
        entry.Add(nameLabel);
        entry.Add(amountLabel);

        return entry;
    }

    private void UpdateCraftButton()
    {
        if (selectedRecipe == null)
        {
            craftButton.SetEnabled(false);
            return;
        }

        bool canCraft = CraftingManager.Instance.CanCraftRecipe(selectedRecipe);
        craftButton.SetEnabled(canCraft);
    }

    private void StartCrafting()
    {
        if (selectedRecipe == null) return;

        int amount = craftAmount.value;
        for (int i = 0; i < amount; i++)
        {
            CraftingManager.Instance.StartCrafting(selectedRecipe);
        }

        // Update UI
        UpdateRecipeDetails();
    }

    private void UpdateCraftingProgress(string recipeName, float progress)
    {
        if (!queueEntries.TryGetValue(recipeName, out var entry))
        {
            entry = CreateQueueEntry(recipeName);
            queueEntries[recipeName] = entry;
            craftingQueue.Add(entry);
        }

        var progressBar = entry.Q<ProgressBar>();
        progressBar.value = progress * 100;

        if (progress >= 1f)
        {
            entry.schedule.Execute(() => {
                entry.RemoveFromHierarchy();
                queueEntries.Remove(recipeName);
            }).StartingIn(1000); // Remove after 1 second
        }
    }

    private VisualElement CreateQueueEntry(string recipeName)
    {
        var entry = new VisualElement();
        entry.AddToClassList("queue-entry");

        var name = new Label(recipeName);
        name.AddToClassList("queue-name");

        var progress = new ProgressBar();
        progress.AddToClassList("progress-bar");
        progress.lowValue = 0;
        progress.highValue = 100;

        entry.Add(name);
        entry.Add(progress);

        return entry;
    }
} 