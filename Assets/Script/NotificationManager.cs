using UnityEngine;
using TMPro;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("General UI")]
    public GameObject notiPanel;
    public TextMeshProUGUI messageText;
    public AudioSource uiAudioSource;
    public AudioClip popUpSound;

    private string currentLocalizationKey;
    private bool currentMessageUsesLocalization;

    void Awake()
    {
        Instance = this;

        // This label is runtime-driven by NotificationManager. If a LocalizedText component
        // is attached in the scene (e.g. key = Tutorial.Step1), it can overwrite messages
        // and make Step1 appear in unrelated scenarios.
        if (messageText != null)
        {
            LocalizedText localizedText = messageText.GetComponent<LocalizedText>();
            if (localizedText != null)
            {
                localizedText.enabled = false;
            }
        }

        if (notiPanel != null) notiPanel.SetActive(false);
    }

    void OnEnable()
    {
        Localization.OnLanguageChanged += HandleLanguageChanged;
    }

    void OnDisable()
    {
        Localization.OnLanguageChanged -= HandleLanguageChanged;
    }

    public void ShowNotification(string message)
    {
        if (notiPanel == null) return;
        CancelInvoke("HideNotification");
        currentLocalizationKey = null;
        currentMessageUsesLocalization = false;
        messageText.text = message;
        notiPanel.SetActive(true);

        if (uiAudioSource != null && popUpSound != null)
            uiAudioSource.PlayOneShot(popUpSound);

        Invoke("HideNotification", 2f);
    }

    public void ShowNotificationKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        currentLocalizationKey = key;
        currentMessageUsesLocalization = true;
        ShowNotification(Localization.Get(key));

        // ShowNotification() resets flags; restore key tracking for language refresh.
        currentLocalizationKey = key;
        currentMessageUsesLocalization = true;
    }

    // Simplified: Only shows the text bubble, no arrow logic
    public void ShowTutorialGuide(string message, GameObject targetObject)
    {
        if (notiPanel != null && messageText != null)
        {
            currentLocalizationKey = null;
            currentMessageUsesLocalization = false;
            messageText.text = message;
            notiPanel.SetActive(true);
        }

        if (uiAudioSource != null && popUpSound != null)
            uiAudioSource.PlayOneShot(popUpSound);

        // targetObject is intentionally allowed to be null or a prefab asset.
        // The tutorial text bubble does not depend on it anymore.
    }

    public void ShowTutorialGuideKey(string key, GameObject targetObject)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        currentLocalizationKey = key;
        currentMessageUsesLocalization = true;
        ShowTutorialGuide(Localization.Get(key), targetObject);

        // ShowTutorialGuide() resets flags; restore key tracking for language refresh.
        currentLocalizationKey = key;
        currentMessageUsesLocalization = true;
    }

    public void HideTutorialGuide()
    {
        if (notiPanel != null) notiPanel.SetActive(false);
    }

    void HideNotification() => notiPanel.SetActive(false);

    private void HandleLanguageChanged()
    {
        if (!currentMessageUsesLocalization) return;
        if (string.IsNullOrWhiteSpace(currentLocalizationKey)) return;
        if (notiPanel == null || messageText == null || !notiPanel.activeInHierarchy) return;

        messageText.text = Localization.Get(currentLocalizationKey);
    }
}