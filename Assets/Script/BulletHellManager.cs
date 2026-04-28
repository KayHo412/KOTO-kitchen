using UnityEngine;

public class BulletHellManager : MonoBehaviour
{
    public GameObject[] bulletVariants; // Drag Bullet_Carrot, Bullet_Pot variants here
    public float spawnRate = 0.8f;
    private float timer;
    private bool canSpawn = true;

    private void OnEnable()
    {
        MinigamePlayer.OnPlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        MinigamePlayer.OnPlayerDied -= HandlePlayerDied;
    }

    void Update()
    {
        if (!canSpawn) return;

        timer += Time.deltaTime;
        if (timer >= spawnRate)
        {
            SpawnPattern();
            timer = 0;
            // Make it harder over time
            spawnRate = Mathf.Max(0.2f, spawnRate - 0.005f);
        }
    }

    void SpawnPattern()
    {
        // Spawn from any direction (360°) around the play area, aimed inward.
        float screenX = 12f; // Adjust based on your camera size
        float screenY = 7f;

        Vector2 dirFromCenter = Random.insideUnitCircle.normalized;
        if (dirFromCenter.sqrMagnitude < 0.001f) dirFromCenter = Vector2.right;

        Vector2 travelDir = -dirFromCenter; // toward center
        Vector3 spawnPos = new Vector3(dirFromCenter.x * screenX, dirFromCenter.y * screenY, 0f);
        float angle = Mathf.Atan2(travelDir.y, travelDir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        GameObject prefab = bulletVariants[Random.Range(0, bulletVariants.Length)];
        Vector3 spawnPosFixed = new Vector3(spawnPos.x, spawnPos.y, 0);
        Instantiate(prefab, spawnPosFixed, rotation);
    }

    private void HandlePlayerDied()
    {
        canSpawn = false;

        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject bullet in bullets)
        {
            Destroy(bullet);
        }
    }
}