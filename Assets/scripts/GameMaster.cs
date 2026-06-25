using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance;

    [Header("Monster Settings")]
    public List<GameObject> monsters = new List<GameObject>();
    public GameObject[] targets = new GameObject[2];

    public Transform centerPoint;
    public float spawnRadius = 10f;
    public float spawnHeight = 0.2f;

    public float minDistance = 15f;
    public float maxDistance = 30f;

    [Header("Time & Progression Settings")]
    public Light sunLight;
    public float dayDuration = 180f;
    public float nightDuration = 360f;
    private float cycleTimer = 0f;
    public bool isDay = true;

    [Tooltip("Aktualny dzień rozgrywki. Każda kolejna noc jest trudniejsza.")]
    public int currentDay = 1;

    [Header("Lighthouse Gleam")]
    public Transform lighthouseGleam;
    [SerializeField] private float rotationSpeed = 50f;
    private float myAngle = 0f;

    [Header("Monsters")]
    public bool isSpawnMonsters = true;
    private float spawnTimer = 0f;
    private List<GameObject> activeMonsters = new List<GameObject>();

    [Header("ItemShop")]
    public GameObject ItemShop;

    void Awake()
    {
        Instance = this;
        if (sunLight != null)
            sunLight.intensity = isDay ? 1f : 0f;
    }

    public void EndGame()
    {
        isSpawnMonsters = false;
        HudsMaster.Instance.showDeathScreen();
        ClearAllMonsters();
    }

    void HandleTimeCycle()
    {
        cycleTimer += Time.deltaTime;
        float currentLimit = isDay ? dayDuration : nightDuration;

        if (sunLight != null)
        {
            float rotationAngle = (cycleTimer / currentLimit) * 180f;
            if (isDay)
                sunLight.transform.rotation = Quaternion.Euler(rotationAngle, -90, 0);
            else
                sunLight.transform.rotation = Quaternion.Euler(rotationAngle + 180f, -90, 0);

            sunLight.intensity = isDay ? 1f : 0f;
        }

        if (cycleTimer >= currentLimit)
        {
            isDay = !isDay;
            cycleTimer = 0f;

            if (isDay)
            {
                currentDay++;
                ItemShop.SetActive(true);
                Debug.Log($"Wstał dzień {currentDay} - bezpiecznie!");
            }
            else
            {
                ItemShop.SetActive(false);
                Debug.Log($"Zapadła noc {currentDay} - uważaj! Poziom trudności wzrasta.");
            }
        }
    }

    public float GetTimeLeft()
    {
        float currentLimit = isDay ? dayDuration : nightDuration;
        return currentLimit - cycleTimer;
    }

    public void setDay()
    {
        isDay = true;
        cycleTimer = 0f;
        currentDay++;
        ItemShop.SetActive(true);
    }

    public void setNight()
    {
        isDay = false;
        cycleTimer = 0f;
        ItemShop.SetActive(false);
    }

    public void SpawnMonster(GameObject monsterPrefab, BasicMonster monsterScript)
    {
        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;
        float randomDistance = UnityEngine.Random.Range(minDistance, maxDistance);
        Vector2 finalOffset = randomDirection * randomDistance;

        Vector3 packCenter = new Vector3(
            centerPoint.position.x + finalOffset.x,
            centerPoint.position.y,
            centerPoint.position.z + finalOffset.y
        );

        int dayPackBonus = Mathf.FloorToInt((currentDay - 1) / 4f);
        int maxMultiplier = monsterScript.spawnMultiplayer + dayPackBonus;
        int multiplier = UnityEngine.Random.Range(1, maxMultiplier + 1);

        int columns = Mathf.CeilToInt(Mathf.Sqrt(multiplier));
        float spacing = 2f;

        for (int i = 0; i < multiplier; i++)
        {
            int row = i / columns;
            int col = i % columns;

            float xOffset = (col - (columns - 1) / 2f) * spacing;
            float zOffset = (row - (columns - 1) / 2f) * spacing;

            Vector3 rawSpawnPosition = new Vector3(
                packCenter.x + xOffset,
                50f,
                packCenter.z + zOffset
            );

            if (NavMesh.SamplePosition(rawSpawnPosition, out NavMeshHit hit, 100f, NavMesh.AllAreas))
            {
                GameObject clone = Instantiate(monsterPrefab, hit.position, Quaternion.identity);
                clone.name = monsterPrefab.name;

                NavMeshAgent monsterAgent = clone.GetComponent<NavMeshAgent>();
                if (monsterAgent != null)
                {
                    monsterAgent.enabled = false;
                }

                if (clone.transform.localScale.x < 0.2f)
                {
                    clone.transform.position = hit.position + new Vector3(0, 0.5f, 0);
                }
                else
                {
                    clone.transform.position = hit.position;
                }

                BasicMonster spawnedMonster = clone.GetComponent<BasicMonster>();
                if (spawnedMonster != null)
                {
                    spawnedMonster.targets = targets;
                    spawnedMonster.ApplyDifficultyScale(currentDay);
                }

                if (monsterAgent != null)
                {
                    monsterAgent.enabled = true;
                }

                activeMonsters.Add(clone);
            }
            else
            {
                Debug.LogWarning($"Punkt spawnu [{rawSpawnPosition.x}, {rawSpawnPosition.z}] wypadł poza siatką NavMesh! Pomijam.");
            }
        }
    }

    public void ClearAllMonsters()
    {
        foreach (GameObject monster in activeMonsters)
        {
            if (monster != null) Destroy(monster);
        }
        activeMonsters.Clear();
    }

    void SpawnAllMonsters()
    {
        foreach (var monsterPrefab in monsters)
        {
            BasicMonster monsterScript = monsterPrefab.GetComponent<BasicMonster>();
            if (monsterScript == null) continue;

            float calculatedChance = monsterScript.spawningChance;
            string nameLower = monsterPrefab.name.ToLower();

            // --- PROGRESJA BEZ SPADKÓW SZANSY ---
            if (currentDay <= 4)
            {
                // Faza 1: Głównie Zombie, pająki rzadko, tanki prawie wcale
                if (nameLower.Contains("zombie")) calculatedChance = 0.70f;
                else if (nameLower.Contains("spider")) calculatedChance = 0.15f;
                else if (nameLower.Contains("tank")) calculatedChance = 0.02f;
            }
            else if (currentDay <= 9)
            {
                // Faza 2: Zombie zostają wysoko, pająki mocno dołączają, tanki powolutku rosną
                if (nameLower.Contains("zombie")) calculatedChance = 0.70f;
                else if (nameLower.Contains("spider")) calculatedChance = 0.65f;
                else if (nameLower.Contains("tank")) calculatedChance = 0.10f;
            }
            else if (currentDay <= 14)
            {
                // Faza 3: Zombie i pająki wysoko, tanki leciutko zwiększają obecność
                if (nameLower.Contains("zombie")) calculatedChance = 0.70f;
                else if (nameLower.Contains("spider")) calculatedChance = 0.65f;
                else if (nameLower.Contains("tank")) calculatedChance = 0.30f;
            }
            else
            {
                // Faza 4: Dzień 15+ (Wszystko naraz na pełnej mocy)
                if (nameLower.Contains("zombie")) calculatedChance = 0.90f;
                else if (nameLower.Contains("spider")) calculatedChance = 0.90f;
                else if (nameLower.Contains("tank")) calculatedChance = 0.50f;
            }

            calculatedChance = Mathf.Clamp01(calculatedChance);

            if (UnityEngine.Random.value <= calculatedChance)
            {
                SpawnMonster(monsterPrefab, monsterScript);
            }
        }
    }

    void Update()
    {
        if (HudsMaster.Instance != null && HudsMaster.Instance.menuCanvas != null && HudsMaster.Instance.menuCanvas.activeInHierarchy)
        {
            return;
        }

        HandleTimeCycle();

        if (isSpawnMonsters && !isDay)
        {
            spawnTimer += Time.deltaTime;
            float currentSpawnDelay = Mathf.Max(0.7f, 5.0f - ((currentDay - 1) * 0.08f));

            if (spawnTimer >= currentSpawnDelay)
            {
                SpawnAllMonsters();
                spawnTimer = 0f;
            }
        }

        if (lighthouseGleam != null)
        {
            lighthouseGleam.gameObject.SetActive(!isDay);
            if (!isDay)
            {
                lighthouseGleam.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            }
        }

        if (isDay && activeMonsters.Count > 0)
        {
            ClearAllMonsters();
        }
    }
}