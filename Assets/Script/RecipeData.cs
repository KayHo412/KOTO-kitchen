using UnityEngine;
using System.Collections.Generic;

// This allows you to right-click in your folders to create a new dish!
[CreateAssetMenu(fileName = "New Recipe", menuName = "KitchenGame/Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("General Info")]
    public string dishName;
    public Sprite finalDishSprite; // Large image of the finished meal

    [Header("Ingredients Needed")]
    public List<IngredientRequirement> ingredients;

    [Header("Cooking Settings")]
    public float requiredStirAmount = 100f;
    public Color soupColor = Color.yellow; // Changes the pot liquid color
}

[System.Serializable]
public class IngredientRequirement
{
    public string ingredientName;
    public Sprite icon;           // The BIG icon for the HUD
    public int amountRequired = 1;

    [Header("Physical Models")]
    public GameObject rawPrefab;    // The WHOLE potato (spawns in the Tray)
    public string requiredTag;      // The tag the pot looks for (e.g., "CutPotato")
}