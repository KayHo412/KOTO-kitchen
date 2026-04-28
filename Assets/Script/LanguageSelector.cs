using UnityEngine;

public class LanguageSelector : MonoBehaviour
{
    public void SetEnglish()
    {
        Localization.SetLanguage(Localization.Language.English);
    }

    public void SetFinnish()
    {
        Localization.SetLanguage(Localization.Language.Finnish);
    }

    public void ToggleEnglishFinnish()
    {
        if (Localization.CurrentLanguage == Localization.Language.English)
            Localization.SetLanguage(Localization.Language.Finnish);
        else
            Localization.SetLanguage(Localization.Language.English);
    }
}
