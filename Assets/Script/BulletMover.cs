using UnityEngine;

public class BulletMover : MonoBehaviour
{
    public float initialSpeedFirst20Seconds = 3f;

    [Header("Speed Scaling")]
    public int speedUpFromScore = 20;
    public float speedIncreasePerSecond = 0.2f;

    [Header("Scale Randomizer")]
    public int randomizeFromScore = 10;
    public float preThresholdScale = 0.22f;
    public float minRandomScale = 0.12f;
    public float maxRandomScale = 0.3f;

    private MinigamePlayer player;

    void Start()
    {
        player = Object.FindFirstObjectByType<MinigamePlayer>();

        bool shouldRandomize = player != null && player.CurrentScore >= randomizeFromScore;

        float low = Mathf.Min(minRandomScale, maxRandomScale);
        float high = Mathf.Max(minRandomScale, maxRandomScale);

        float scale = shouldRandomize ? Random.Range(low, high) : preThresholdScale;
        scale = Mathf.Clamp(scale, 0.1f, 0.4f); // hard safety limit
        transform.localScale = Vector3.one * scale;

        // Cleanup: Destroys bullet when it's far off screen
        Destroy(gameObject, 7f);
    }

    void Update()
    {
        if (player == null) player = Object.FindFirstObjectByType<MinigamePlayer>();

        float currentSpeed = initialSpeedFirst20Seconds;
        if (player != null)
        {
            float extraSeconds = Mathf.Max(0f, player.CurrentScore - speedUpFromScore);
            currentSpeed = initialSpeedFirst20Seconds + (extraSeconds * speedIncreasePerSecond);
        }

        // Move forward based on the rotation we give it when spawning
        transform.Translate(Vector2.right * currentSpeed * Time.deltaTime);
    }
}