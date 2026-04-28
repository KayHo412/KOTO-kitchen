using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class StirringMinigame : MonoBehaviour
{
    [Header("UI References")]
    public Image fillImage;
    public GameObject doneButton; // Drag your 'Done' Button here

    [Header("Audio")]
    public AudioSource windowAudioSource;   // Assign SFX source for window open sound
    public AudioClip stirWindowOpenClip;    // Plays once when stir window appears

    [Header("Settings")]
    public float stirRequirement = 100f;
    public float sensitivity = 20f;

    private float currentProgress = 0f;
    private bool isFinished = false;
    private HUDManager hud;
    private bool waitingForInputRelease = false;
    private bool hasPointerBaseline = false;
    private Vector2 lastPointerPosition = Vector2.zero;

    void Awake() => hud = Object.FindFirstObjectByType<HUDManager>();

    void OnEnable()
    {
        currentProgress = 0f;
        isFinished = false;
        waitingForInputRelease = true;
        hasPointerBaseline = false;
        if (fillImage != null) fillImage.fillAmount = 0f;
        if (doneButton != null) doneButton.SetActive(false); // Hide button at start

        if (windowAudioSource != null && stirWindowOpenClip != null)
        {
            windowAudioSource.PlayOneShot(stirWindowOpenClip);
        }
    }

    void Update()
    {
        if (isFinished) return; // Stop logic if bar is full

        bool pointerHeld = false;
        Vector2 currentPointerPosition = Vector2.zero;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            pointerHeld = true;
            currentPointerPosition = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            pointerHeld = true;
            currentPointerPosition = Mouse.current.position.ReadValue();
        }

        // Ignore the click/touch that opened the window, especially on web builds.
        // We only start counting after the player has released once.
        if (waitingForInputRelease)
        {
            if (!pointerHeld)
            {
                waitingForInputRelease = false;
                hasPointerBaseline = false;
            }
            return;
        }

        if (!pointerHeld)
        {
            hasPointerBaseline = false;
            return;
        }

        if (!hasPointerBaseline)
        {
            lastPointerPosition = currentPointerPosition;
            hasPointerBaseline = true;
            return;
        }

        float movementMagnitude = Vector2.Distance(currentPointerPosition, lastPointerPosition);
        lastPointerPosition = currentPointerPosition;

        if (movementMagnitude > 0.01f)
        {
            currentProgress += movementMagnitude * sensitivity;
            currentProgress = Mathf.Clamp(currentProgress, 0, stirRequirement);

            if (fillImage != null)
                fillImage.fillAmount = currentProgress / stirRequirement;

            if (currentProgress >= stirRequirement)
            {
                ShowDoneButton();
            }
        }
    }

    void ShowDoneButton()
    {
        isFinished = true;
        if (doneButton != null) doneButton.SetActive(true);
        Debug.Log("Stirring Complete! Show Done Button.");
    }

    // Called by the Done Button's OnClick event
    public void OnDoneClicked()
    {
        if (hud != null) hud.ShowFinalResult(); // Tell HUD to show the big win screen
        gameObject.SetActive(false);           // Close the stirring window
    }
}