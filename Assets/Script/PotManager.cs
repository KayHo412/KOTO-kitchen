using UnityEngine;

public class PotManager : MonoBehaviour
{
    [Header("References")]
    public HUDManager hud;
    public GameObject stirWindow;

    [Header("Audio")]
    public AudioSource boilingAudioSource;

    private bool isWaterReady = false;
    private bool hasWater = false;
    private bool recipeFinished = false;

    void Start()
    {
        if (stirWindow != null) stirWindow.SetActive(false);

        if (boilingAudioSource != null) boilingAudioSource.Stop();
    }

    public bool GetWaterStatus() => hasWater;

    public void SetWaterReady(bool ready)
    {
        isWaterReady = ready;
        UpdateBoilingSound();
    }

    public void SetWaterStatus(bool filled)
    {
        hasWater = filled;
        UpdateBoilingSound();

        SteamSystem steam = Object.FindFirstObjectByType<SteamSystem>();
        if (steam != null) steam.NotifyWaterAdded();
    }

    // New: Handle Cream specifically after animation finishes
    public void NotifyCreamAdded(GameObject creamBottle)
    {
        if (hasWater && isWaterReady && !recipeFinished)
        {
            bool added = hud != null && hud.RegisterIngredientAdded(creamBottle.gameObject.name, creamBottle.GetComponent<DraggableTool>());
            if (added)
            {
                Destroy(creamBottle);

                if (TutorialController.Instance != null)
                    TutorialController.Instance.ProceedNextStep(creamBottle);

                if (hud.IsRecipeComplete())
                {
                    recipeFinished = true;
                    UpdateBoilingSound();

                    if (hud != null) hud.HideOrderTicket();
                    if (stirWindow != null) stirWindow.SetActive(true);
                }
            }
            else
            {
                if (hud != null)
                    hud.NotifyWrongIngredient(creamBottle.gameObject.name);

                Destroy(creamBottle);
            }
        }
        else
        {
            // Error handling if they pour cream too early
            if (NotificationManager.Instance != null)
            {
                if (!hasWater) NotificationManager.Instance.ShowNotification(Localization.Get("PotDry"));
                else if (!isWaterReady) NotificationManager.Instance.ShowNotification(Localization.Get("WaterCold"));
            }
            Destroy(creamBottle);
        }
    }

    private void UpdateBoilingSound()
    {
        if (boilingAudioSource == null) return;

        if (hasWater && isWaterReady && !recipeFinished)
        {
            if (!boilingAudioSource.isPlaying) boilingAudioSource.Play();
        }
        else
        {
            if (boilingAudioSource.isPlaying) boilingAudioSource.Stop();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Handle standard cut ingredients via trigger
        if (other.CompareTag("CutIngredient") && !recipeFinished)
        {
            if (hasWater && isWaterReady)
            {
                bool added = hud != null && hud.RegisterIngredientAdded(other.gameObject.name, other.GetComponent<DraggableTool>());
                if (added)
                {
                    if (TutorialController.Instance != null)
                    {
                        TutorialController tutorial = TutorialController.Instance;
                        string stepKey = tutorial.GetCurrentInstructionKey();
                        int stepIndex = tutorial.GetCurrentStepIndex();

                        bool isIngredientToPotStep =
                            stepKey == "Tutorial.Step6" ||
                            stepKey == "Tutorial.Step7" ||
                            stepIndex == 5 ||
                            stepIndex == 6;

                        if (isIngredientToPotStep)
                            tutorial.ProceedNextStep(other.gameObject);
                    }

                    Destroy(other.gameObject);

                    if (hud.IsRecipeComplete())
                    {
                        recipeFinished = true;
                        UpdateBoilingSound();

                        if (hud != null) hud.HideOrderTicket();
                        if (stirWindow != null) stirWindow.SetActive(true);
                    }
                }
                else
                {
                    if (hud != null)
                    {
                        hud.NotifyWrongIngredient(other.gameObject.name);
                    }
                    else if (NotificationManager.Instance != null)
                    {
                        NotificationManager.Instance.ShowNotification(Localization.Get("WrongIngredientNotice"));
                    }

                    Destroy(other.gameObject);
                }
            }
            else
            {
                if (NotificationManager.Instance != null)
                {
                    if (!hasWater) NotificationManager.Instance.ShowNotification(Localization.Get("PotDry"));
                    else if (!isWaterReady) NotificationManager.Instance.ShowNotification(Localization.Get("WaterCold"));
                }
            }
        }
    }

}