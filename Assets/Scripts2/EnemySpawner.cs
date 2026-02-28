using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject redSlimePrefab;
    public GameObject blueSlimePrefab;
    public GameObject greenSlimePrefab;

    [Header("Testing / Spawning Toggles")]
    public bool spawnRed = true;
    public bool spawnBlue = true;
    public bool spawnGreen = true;

    [Header("Difficulty Tuning (Base Spawn Rates)")]
    [Tooltip("Numerator for spawn interval: Interval = Base Rate / Day")]
    public float baseRateGreen = 3f;
    public float baseRateBlue = 4f;
    public float baseRateRed = 6f;

    [Header("Dynamic Difficulty (Days 3 & 4)")]
    [Tooltip("If active enemies drop to or below this number, spawns speed up.")]
    public int lowEnemyThreshold = 4;
    [Tooltip("Multiplier for the numerator when low on enemies. (< 1 makes it faster)")]
    public float fastSpawnMultiplier = 0.5f;

    [Tooltip("If active enemies reach or exceed this number, spawns slow down.")]
    public int highEnemyThreshold = 12;
    [Tooltip("Multiplier for the numerator when overwhelmed. (> 1 makes it slower)")]
    public float slowSpawnMultiplier = 2.0f;

    [Header("Difficulty Testing")]
    [Tooltip("Keep checked to test specific days. Uncheck when the final game is ready to read from PlayerPrefs.")]
    public bool useTestDay = true;
    [Range(1, 4)] public int testDay = 1;

    [Header("Spawn Boundaries")]
    public float minSpawnDistance = 10f;
    public float maxSpawnDistance = 16f;

    // Internal Difficulty Variables
    private int currentDay;
    private float intervalGreen;
    private float intervalBlue;
    private float intervalRed;

    private bool isSpawnerActive = false;
    private GameObject firstSlime;
    private Transform playerTarget;

    // Independent Timers for each Slime Type
    private float nextSpawnTimeGreen;
    private float nextSpawnTimeBlue;
    private float nextSpawnTimeRed;

    // Tracking the pacing state so we only log when it changes, avoiding console spam
    private enum PacingState { Normal, Fast, Slow }
    private PacingState currentPacing = PacingState.Normal;

    void Start()
    {
        // 1. Determine the Current Day (Reads from other scene via PlayerPrefs)
        if (useTestDay)
        {
            currentDay = testDay;
            Debug.Log($"System: Using Test Day {currentDay} from Inspector.");
        }
        else
        {
            currentDay = PlayerPrefs.GetInt("CurrentDay", 1);
        }

        // Clamp to ensure it never exceeds your 4-day maximum
        currentDay = Mathf.Clamp(currentDay, 1, 4);

        // 2. Initial calculate individual intervals based on the Day
        CalculateIntervals(1.0f); // 1.0f is the normal multiplier

        // 3. Find the Player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }

        Debug.Log($"System: Spawner initialized for Day {currentDay}. Base Intervals - Green: {intervalGreen}s, Blue: {intervalBlue}s, Red: {intervalRed}s.");

        // 4. Drop the first slime immediately
        SpawnFirstSlime();
    }

    void CalculateIntervals(float multiplier)
    {
        intervalGreen = (baseRateGreen * multiplier) / currentDay;
        intervalBlue = (baseRateBlue * multiplier) / currentDay;
        intervalRed = (baseRateRed * multiplier) / currentDay;
    }

    void SpawnFirstSlime()
    {
        Vector2 spawnPos = GetRandomSpawnPosition();
        GameObject prefabToSpawn = null;

        if (currentDay == 1)
        {
            if (greenSlimePrefab != null && spawnGreen)
            {
                prefabToSpawn = greenSlimePrefab;
            }
            else
            {
                prefabToSpawn = GetRandomAllowedEnemy();
            }
        }
        else
        {
            prefabToSpawn = GetRandomAllowedEnemy();
        }

        if (prefabToSpawn != null)
        {
            firstSlime = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            Debug.Log($"System: First slime ({prefabToSpawn.name}) dropped. Spawner entering standby mode.");
        }
        else
        {
            Debug.LogWarning("Spawner: Failed to find an allowed prefab for the first slime!");
            ActivateSpawner();
        }
    }

    void Update()
    {
        if (playerTarget == null) return;

        // STANDBY MODE: Wait for the player to kill the first slime
        if (!isSpawnerActive)
        {
            if (firstSlime == null || !firstSlime.activeInHierarchy)
            {
                Debug.Log($"System: First slime defeated! Activating main swarm.");
                ActivateSpawner();
            }
            return;
        }

        // --- DYNAMIC DIFFICULTY CHECK ---
        AdjustPacingBasedOnEnemyCount();

        // ACTIVE MODE: Independent spawning checks gated by the Current Day
        if (spawnGreen && greenSlimePrefab != null && Time.time >= nextSpawnTimeGreen)
        {
            SpawnSpecificEnemy(greenSlimePrefab);
            nextSpawnTimeGreen = Time.time + intervalGreen;
        }

        if (currentDay >= 2 && spawnBlue && blueSlimePrefab != null && Time.time >= nextSpawnTimeBlue)
        {
            SpawnSpecificEnemy(blueSlimePrefab);
            nextSpawnTimeBlue = Time.time + intervalBlue;
        }

        if (currentDay >= 3 && spawnRed && redSlimePrefab != null && Time.time >= nextSpawnTimeRed)
        {
            SpawnSpecificEnemy(redSlimePrefab);
            nextSpawnTimeRed = Time.time + intervalRed;
        }
    }

    void AdjustPacingBasedOnEnemyCount()
    {
        // Only apply Dynamic Pacing on Day 3 and 4
        if (currentDay < 3) return;

        // Count how many enemies are currently active in the scene
        GameObject[] activeEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        int enemyCount = activeEnemies != null ? activeEnemies.Length : 0;

        PacingState newState = PacingState.Normal;
        float currentMultiplier = 1.0f;

        // Determine if we need to shift gears
        if (enemyCount <= lowEnemyThreshold)
        {
            newState = PacingState.Fast;
            currentMultiplier = fastSpawnMultiplier;
        }
        else if (enemyCount >= highEnemyThreshold)
        {
            newState = PacingState.Slow;
            currentMultiplier = slowSpawnMultiplier;
        }

        // If our state just changed, log it and recalculate the intervals
        if (newState != currentPacing)
        {
            currentPacing = newState;
            CalculateIntervals(currentMultiplier);
            Debug.Log($"System: Dynamic Pacing shifted to [{currentPacing}]. Active Enemies: {enemyCount}. Multiplier applied: {currentMultiplier}");
        }
    }

    void ActivateSpawner()
    {
        isSpawnerActive = true;

        float currentTime = Time.time;
        nextSpawnTimeGreen = currentTime + intervalGreen;
        nextSpawnTimeBlue = currentTime + intervalBlue;
        nextSpawnTimeRed = currentTime + intervalRed;

        Debug.Log($"System: Spawner activated. Day {currentDay} swarm inbound!");
    }

    Vector2 GetRandomSpawnPosition()
    {
        if (playerTarget == null) return Vector2.zero;

        float randomAngle = Random.Range(0f, 360f);
        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);

        Vector2 spawnDirection = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
        return (Vector2)playerTarget.position + (spawnDirection * randomDistance);
    }

    void SpawnSpecificEnemy(GameObject prefab)
    {
        Vector2 spawnPos = GetRandomSpawnPosition();
        Instantiate(prefab, spawnPos, Quaternion.identity);
        Debug.Log($"Spawner: Dropped {prefab.name} at {spawnPos}");
    }

    GameObject GetRandomAllowedEnemy()
    {
        List<GameObject> allowedEnemies = new List<GameObject>();

        if (spawnGreen && greenSlimePrefab != null) allowedEnemies.Add(greenSlimePrefab);
        if (currentDay >= 2 && spawnBlue && blueSlimePrefab != null) allowedEnemies.Add(blueSlimePrefab);
        if (currentDay >= 3 && spawnRed && redSlimePrefab != null) allowedEnemies.Add(redSlimePrefab);

        if (allowedEnemies.Count == 0) return null;

        return allowedEnemies[Random.Range(0, allowedEnemies.Count)];
    }
}