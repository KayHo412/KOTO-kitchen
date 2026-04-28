using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public static TutorialController Instance { get; private set; }

    [System.Serializable]
    public struct TutorialStep
    {
        public string instructionKey; // Optional localization key (recommended)
        public string instruction;
        public GameObject targetObject;
    }

    public TutorialStep[] steps;
    private int currentStepIndex = 0;
    private bool isTutorialActive = false;
    private int lastAdvancedFrame = -1;
    private const float StepBubbleTimeoutSeconds = 15f;

    private const string Step1Key = "Tutorial.Step1";
    private const string Step6Key = "Tutorial.Step6";
    private const string Step7Key = "Tutorial.Step7";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        StartTutorialImmediate();
    }

    void OnEnable()
    {
        Localization.OnLanguageChanged += HandleLanguageChanged;
    }

    void OnDisable()
    {
        Localization.OnLanguageChanged -= HandleLanguageChanged;
    }

    void StartTutorialImmediate()
    {
        isTutorialActive = true;

        if (currentStepIndex >= steps.Length)
        {
            FinishTutorial();
            return;
        }

        ShowCurrentStep();
    }

    void ShowCurrentStep()
    {
        CancelInvoke("HideTutorialBubbleDueToInactivity");

        if (currentStepIndex < steps.Length)
        {
            var step = steps[currentStepIndex];
            string localizedInstruction = ResolveStepInstruction(step);
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowTutorialGuide(localizedInstruction, step.targetObject);
            }
            Invoke("HideTutorialBubbleDueToInactivity", StepBubbleTimeoutSeconds);

            if (currentStepIndex == steps.Length - 1)
            {
                CancelInvoke("FinishTutorial");
                Invoke("FinishTutorial", 8f);
            }
        }
        else
        {
            FinishTutorial();
        }
    }

    private string ResolveStepInstruction(TutorialStep step)
    {
        // Preferred path: explicit key in inspector
        if (!string.IsNullOrWhiteSpace(step.instructionKey))
        {
            string localizedByKey = Localization.Get(step.instructionKey);
            if (localizedByKey != step.instructionKey) return localizedByKey;
        }

        // Backward-compatible fallback: keep old text as-is
        // Also supports using instruction as a key if user chooses.
        if (!string.IsNullOrWhiteSpace(step.instruction))
        {
            string localizedByInstruction = Localization.Get(step.instruction);
            return localizedByInstruction;
        }

        return string.Empty;
    }

    private void HandleLanguageChanged()
    {
        if (!isTutorialActive) return;
        if (currentStepIndex < 0 || currentStepIndex >= steps.Length) return;

        // Re-show current step text in the newly selected language.
        ShowCurrentStep();
    }

    public GameObject GetCurrentTarget()
    {
        if (currentStepIndex < 0 || currentStepIndex >= steps.Length) return null;
        return steps[currentStepIndex].targetObject;
    }

    public int GetCurrentStepIndex()
    {
        return currentStepIndex;
    }

    public string GetCurrentInstructionKey()
    {
        if (currentStepIndex < 0 || currentStepIndex >= steps.Length) return string.Empty;
        return steps[currentStepIndex].instructionKey;
    }

    // UI buttons use this overload because they cannot easily pass a GameObject.
    public void ProceedNextStep()
    {
        if (!isTutorialActive) return;
        if (Time.frameCount == lastAdvancedFrame) return;

        lastAdvancedFrame = Time.frameCount;
        currentStepIndex++;
        ShowCurrentStep();
    }

    public void ProceedNextStep(GameObject triggeredBy)
    {
        if (currentStepIndex >= steps.Length) return;
        if (triggeredBy == null) return;

        if (!CanAdvanceByTrigger(triggeredBy)) return;
        AdvanceStep();
    }

    private bool CanAdvanceByTrigger(GameObject triggeredBy)
    {
        TutorialStep step = steps[currentStepIndex];
        string key = GetEffectiveStepKey(step);

        // Key-based rules are primary and deterministic.
        if (string.Equals(key, Step1Key, System.StringComparison.OrdinalIgnoreCase))
            return IsPotObject(triggeredBy);

        if (string.Equals(key, Step6Key, System.StringComparison.OrdinalIgnoreCase) ||
            string.Equals(key, Step7Key, System.StringComparison.OrdinalIgnoreCase))
            return triggeredBy.CompareTag("CutIngredient");

        // Backward-compatible fallback for old inspector setups.
        return MatchByTargetFallback(step.targetObject, triggeredBy);
    }

    private string GetEffectiveStepKey(TutorialStep step)
    {
        if (!string.IsNullOrWhiteSpace(step.instructionKey))
            return NormalizeStepKey(step.instructionKey);

        // Backward compatibility: some scenes stored the key in `instruction` instead.
        if (!string.IsNullOrWhiteSpace(step.instruction))
            return NormalizeStepKey(step.instruction);

        return string.Empty;
    }

    private string NormalizeStepKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;

        string trimmed = key.Trim();

        // Tolerate common typo used in inspector values.
        if (string.Equals(trimmed, "tutorials.step1", System.StringComparison.OrdinalIgnoreCase))
            return Step1Key;

        return trimmed;
    }

    private bool MatchByTargetFallback(GameObject expectedTarget, GameObject triggeredBy)
    {
        if (expectedTarget == null || triggeredBy == null) return false;
        if (ReferenceEquals(expectedTarget, triggeredBy)) return true;

        string expectedName = NormalizeObjectName(expectedTarget.name);
        string triggeredName = NormalizeObjectName(triggeredBy.name);

        if (expectedName == triggeredName) return true;

        if (expectedName == "CuttingBoard" && triggeredBy.CompareTag("Ingredient")) return true;
        if (expectedName == "Knife" && triggeredName.Contains("Knife")) return true;
        if (IsPotObject(expectedTarget) && IsPotObject(triggeredBy)) return true;

        return false;
    }

    private bool IsPotObject(GameObject obj)
    {
        if (obj == null) return false;
        if (obj.TryGetComponent<DraggableTool>(out DraggableTool tool) && tool.isPot) return true;
        if (obj.GetComponent<PotManager>() != null) return true;
        return false;
    }

    private void AdvanceStep()
    {
        if (Time.frameCount == lastAdvancedFrame) return;
        lastAdvancedFrame = Time.frameCount;

        currentStepIndex++;

        if (!isTutorialActive) return;

        if (currentStepIndex >= steps.Length)
        {
            FinishTutorial();
            return;
        }

        ShowCurrentStep();
    }

    private string NormalizeObjectName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName)) return string.Empty;
        return objectName.Replace("(Clone)", string.Empty).Trim();
    }

    public void FinishTutorial()
    {
        isTutorialActive = false;
        CancelInvoke("HideTutorialBubbleDueToInactivity");
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.HideTutorialGuide();
        }
    }

    private void HideTutorialBubbleDueToInactivity()
    {
        if (!isTutorialActive) return;
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.HideTutorialGuide();
        }
    }
}