using UnityEngine;

public class StoveSwitch : MonoBehaviour
{
    // Drag your "Off" background sprite here
    public Sprite stoveOffSprite;
    // Drag your "On" background sprite here
    public Sprite stoveOnSprite;

    private SpriteRenderer backgroundRenderer;
    private bool isStoveOn = false;

    void Awake()
    {
        backgroundRenderer = GetComponent<SpriteRenderer>();
        // Ensure it starts in the Off state
        backgroundRenderer.sprite = stoveOffSprite;
    }

    public void ToggleStove()
    {
        isStoveOn = !isStoveOn;

        if (isStoveOn)
        {
            backgroundRenderer.sprite = stoveOnSprite;
            Debug.Log("Stove turned ON");
        }
        else
        {
            backgroundRenderer.sprite = stoveOffSprite;
            Debug.Log("Stove turned OFF");
        }
    }
}