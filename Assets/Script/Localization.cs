using System;
using System.Collections.Generic;
using UnityEngine;

public static class Localization
{
    public enum Language
    {
        English,
        Finnish
    }

    private const string LanguagePrefKey = "GameLanguage";

    private static readonly Dictionary<Language, Dictionary<string, string>> table = new()
    {
        {
            Language.English, new Dictionary<string, string>
            {
                { "Score", "Score" },
                { "ReadyToStir", "STIR THE SOUP USING YOUR FINGER" },
                { "DeathTitle", "Game Over" },
                { "DishReady", "is Ready!" },
                { "Pause", "Pause" },
                { "Resume", "Resume" },
                { "PotDry", "The pot is dry! Pour water first." },
                { "WaterCold", "Water is cold! Wait for the steam." },
                { "SliceFoodFirst", "Slice the food first!" },
                { "BoardOccupied", "Cutting board is occupied!" },
                { "VegetableSliced", "VEGETABLE SLICED!" },
                { "DishFinished", "You have successfully made Lohikeitto!" },
                { "Done", "Done" },
                { "BackToMenu", "Menu"},
                { "Restart", "Restart" },
                { "WrongIngredientNotice", "Wrong ingredient! Expected {0}, but got {1}." },

                // Tutorial step keys (set these in TutorialController step.instructionKey)
                { "Tutorial.Step1", "Drag the pot to the stove." },
                { "Tutorial.Step2", "Pour water into the pot." },
                { "Tutorial.Step3", "Turn on the stove and wait until the water starts boiling." },
                { "Tutorial.Step4", "Place an ingredient on the cutting board." },
                { "Tutorial.Step5", "Chop the ingredient with the knife." },
                { "Tutorial.Step6", "Drag the cut ingredient into the pot." },
                { "Tutorial.Step7", "Repeat until all ingredients are added." },
                { "Tutorial.Step8", "Stir to finish the dish." }
            }
        },
        {
            Language.Finnish, new Dictionary<string, string>
            {
                { "Score", "Pisteet" },
                { "ReadyToStir", "SEKOITA KEITTO SORMELLASI" },
                { "DeathTitle", "Peli p\u00E4\u00E4ttyi" },
                { "DishReady", "on valmis!" },
                { "Pause", "Tauko" },
                { "Resume", "Jatka" },
                { "PotDry", "Kattila on kuiva! Kaada ensin vett\u00E4." },
                { "WaterCold", "Vesi on kylm\u00E4\u00E4! Odota h\u00F6yry\u00E4." },
                { "SliceFoodFirst", "Pilko ruoka ensin!" },
                { "BoardOccupied", "Leikkuulauta on varattu!" },
                { "VegetableSliced", "VIHANNES PILKOTTU!" },
                { "DishFinished", "Onnistuit tekem\u00E4\u00E4n Lohikeittoa!" },
                { "Done", "Valmis" },
                { "BackToMenu", "Valikko" },
                { "Restart", "Uudestaan" },
                { "WrongIngredientNotice", "V\u00E4\u00E4r\u00E4 ainesosa! Odotettiin {0}, mutta saatiin {1}." },

                // Tutorial step keys (set these in TutorialController step.instructionKey)
                { "Tutorial.Step1", "Ved\u00E4 kattila liedelle." },
                { "Tutorial.Step2", "Kaada vett\u00E4 kattilaan." },
                { "Tutorial.Step3", "Laita liesi p\u00E4\u00E4lle ja odota, ett\u00E4 vesi alkaa kiehua." },
                { "Tutorial.Step4", "Aseta ainesosa leikkuulaudalle." },
                { "Tutorial.Step5", "Pilko ainesosa veitsell\u00E4." },
                { "Tutorial.Step6", "Ved\u00E4 pilkottu ainesosa kattilaan." },
                { "Tutorial.Step7", "Toista, kunnes kaikki ainesosat on lis\u00E4tty." },
                { "Tutorial.Step8", "Sekoita viimeistell\u00E4ksesi annoksen." }
            }
        }
    };

    private static Language currentLanguage = LoadSavedLanguage();

    public static event Action OnLanguageChanged;

    public static Language CurrentLanguage => currentLanguage;

    public static string Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;

        if (table.TryGetValue(currentLanguage, out var langTable) && langTable.TryGetValue(key, out var value))
            return value;

        if (table.TryGetValue(Language.English, out var fallbackTable) && fallbackTable.TryGetValue(key, out var fallback))
            return fallback;

        return key;
    }

    public static void SetLanguage(Language language)
    {
        if (currentLanguage == language) return;

        currentLanguage = language;
        PlayerPrefs.SetString(LanguagePrefKey, language == Language.Finnish ? "fi" : "en");
        PlayerPrefs.Save();
        OnLanguageChanged?.Invoke();
    }

    public static void SetLanguage(string languageCode)
    {
        if (string.Equals(languageCode, "fi", StringComparison.OrdinalIgnoreCase))
            SetLanguage(Language.Finnish);
        else
            SetLanguage(Language.English);
    }

    private static Language LoadSavedLanguage()
    {
        string code = PlayerPrefs.GetString(LanguagePrefKey, "en");
        return string.Equals(code, "fi", StringComparison.OrdinalIgnoreCase)
            ? Language.Finnish
            : Language.English;
    }
}
