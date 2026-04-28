using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour
{
    public string localizationKey;

    private TextMeshProUGUI tmpText;
    private Text legacyText;

    void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        legacyText = GetComponent<Text>();
    }

    void OnEnable()
    {
        Localization.OnLanguageChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        Localization.OnLanguageChanged -= Refresh;
    }

    public void Refresh()
    {
        string localizedValue = Localization.Get(localizationKey);

        if (tmpText != null) tmpText.text = localizedValue;
        if (legacyText != null) legacyText.text = localizedValue;
    }
}
