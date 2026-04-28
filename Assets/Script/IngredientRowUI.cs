using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientRowUI : MonoBehaviour
{
    public TextMeshProUGUI ingredientText;
    public Toggle checkBox;

    private int remaining;
    private string itemName;

    public bool IsFinished => remaining <= 0;

    public void SetupRow(string name, int amount)
    {
        itemName = name;
        remaining = amount;
        checkBox.isOn = false;
        UpdateUI();
    }

    public void DecreaseAmount()
    {
        if (remaining > 0)
        {
            remaining--;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (remaining > 0)
        {
            ingredientText.text = $"{itemName} x{remaining}";
        }
        else
        {
            // The Cross-out effect for Lohikeitto
            ingredientText.text = $"<s>{itemName}</s>";
            ingredientText.color = Color.gray;
            checkBox.isOn = true;
        }
    }
}