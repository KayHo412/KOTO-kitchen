using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class HUDManager : MonoBehaviour
{
    public RecipeData currentLevelRecipe;

    [Header("Main HUD UI (Original)")]
    public TextMeshProUGUI dishNameText;
    public Image ingredientIconDisplay;
    public TextMeshProUGUI countText;

    [Header("Checklist UI (New)")]
    public GameObject recipePanel;
    public Transform listContainer;
    public GameObject ingredientRowPrefab;

    [Header("Order Ticket UI")]
    public GameObject orderTicketPanel;

    [Header("Result Window (Original)")]
    public GameObject resultWindow;
    public Image resultWindowImageUI;
    public TextMeshProUGUI finalNameText;

    [Header("Result Window Audio")]
    public AudioSource resultWindowAudioSource;
    public AudioClip resultWindowOpenClip;

    private int currentIngredientIndex = 0;
    private int currentAmountCollected = 0;
    private Dictionary<string, IngredientRowUI> activeRows = new Dictionary<string, IngredientRowUI>();

    private string NormalizeName(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("(Clone)", string.Empty).Trim();
    }

    void Start()
    {
        if (resultWindow != null) resultWindow.SetActive(false);
        if (orderTicketPanel != null) orderTicketPanel.SetActive(false);
        InitializeRecipeList(); // Sets up the checkboxes
        UpdateUI();             // Sets up your original HUD
    }

    public void ShowOrderTicket()
    {
        if (orderTicketPanel != null) orderTicketPanel.SetActive(true);
    }

    public void HideOrderTicket()
    {
        if (orderTicketPanel != null) orderTicketPanel.SetActive(false);
    }

    public void InitializeRecipeList()
    {
        if (currentLevelRecipe == null || listContainer == null || ingredientRowPrefab == null) return;

        foreach (Transform child in listContainer) Destroy(child.gameObject);
        activeRows.Clear();

        foreach (var req in currentLevelRecipe.ingredients)
        {
            GameObject newRow = Instantiate(ingredientRowPrefab, listContainer);
            IngredientRowUI rowScript = newRow.GetComponent<IngredientRowUI>();
            if (rowScript == null) continue;

            rowScript.SetupRow(req.ingredientName, req.amountRequired);

            // CRITICAL: use ingredientName as key (not tag) to avoid duplicate-key issues.
            activeRows[NormalizeName(req.ingredientName)] = rowScript;
        }
    }

    public void UpdateUI()
    {
        if (currentLevelRecipe == null) return;
        dishNameText.text = currentLevelRecipe.dishName;

        if (currentIngredientIndex < currentLevelRecipe.ingredients.Count)
        {
            var req = currentLevelRecipe.ingredients[currentIngredientIndex];
            if (req.icon != null)
            {
                ingredientIconDisplay.sprite = req.icon;
                ingredientIconDisplay.gameObject.SetActive(true);
            }
            int remaining = req.amountRequired - currentAmountCollected;
            countText.text = "x" + remaining;
        }
        else
        {
            ingredientIconDisplay.gameObject.SetActive(false);
            countText.text = Localization.Get("ReadyToStir");
        }
    }

    public bool RegisterIngredientAdded(string addedObjectName, DraggableTool tool)
    {
        if (IsRecipeComplete()) return false;
        if (tool != null && tool.isCounted) return false;
        if (string.IsNullOrEmpty(addedObjectName)) return false;

        string cleanAddedName = NormalizeName(addedObjectName);

        // 1) Match against the currently required ingredient by NAME contains check.
        var currentReq = currentLevelRecipe.ingredients[currentIngredientIndex];
        string currentReqName = NormalizeName(currentReq.ingredientName);
        bool matchesCurrentRequired = cleanAddedName.IndexOf(currentReqName, System.StringComparison.OrdinalIgnoreCase) >= 0;

        if (!matchesCurrentRequired) return false;

        // 2) Update the checklist row for that ingredient.
        if (activeRows.TryGetValue(currentReqName, out IngredientRowUI row))
        {
            row.DecreaseAmount();
        }

        // 3) Update original linear HUD index/count logic.
        if (tool != null) tool.isCounted = true;
        currentAmountCollected++;

        if (currentAmountCollected >= currentReq.amountRequired)
        {
            currentIngredientIndex++;
            currentAmountCollected = 0;
        }

        UpdateUI();
        return true;
    }

    public void NotifyWrongIngredient(string addedObjectName)
    {
        if (IsRecipeComplete()) return;
        if (string.IsNullOrWhiteSpace(addedObjectName)) return;
        if (currentLevelRecipe == null || currentIngredientIndex >= currentLevelRecipe.ingredients.Count) return;

        string cleanAddedName = NormalizeName(addedObjectName);
        var currentReq = currentLevelRecipe.ingredients[currentIngredientIndex];
        string currentReqName = NormalizeName(currentReq.ingredientName);

        if (string.IsNullOrWhiteSpace(cleanAddedName) || cleanAddedName.IndexOf(currentReqName, System.StringComparison.OrdinalIgnoreCase) >= 0)
            return;

        if (NotificationManager.Instance != null)
        {
            string message = string.Format(Localization.Get("WrongIngredientNotice"), currentReq.ingredientName, cleanAddedName);
            NotificationManager.Instance.ShowNotification(message);
        }
    }

    public bool IsRecipeComplete() => currentIngredientIndex >= currentLevelRecipe.ingredients.Count;

    public void ShowFinalResult()
    {
        if (resultWindow != null) resultWindow.SetActive(true);
        if (finalNameText != null) finalNameText.text = currentLevelRecipe.dishName + " " + Localization.Get("DishReady");
        if (resultWindowImageUI != null) resultWindowImageUI.sprite = currentLevelRecipe.finalDishSprite;

        if (resultWindowAudioSource != null && resultWindowOpenClip != null)
        {
            resultWindowAudioSource.PlayOneShot(resultWindowOpenClip);
        }
    }

    public void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    public void GoToMainMenu() => SceneManager.LoadScene("StartingScreen");
}