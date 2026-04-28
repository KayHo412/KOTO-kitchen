using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance; // Allows other scripts to find this easily

    public SpriteRenderer backgroundRenderer;
    public Sprite stoveOffSprite;
    public Sprite stoveOnSprite;

    void Awake()
    {
        Instance = this;
    }

    public void SwitchToStoveOn()
    {
        backgroundRenderer.sprite = stoveOnSprite;
        Debug.Log("Stove is now HOT!");
    }
}