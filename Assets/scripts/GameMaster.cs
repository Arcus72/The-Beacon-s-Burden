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

    [Header("Time Settings")]
    public Light sunLight; 
    public float dayDuration = 180f;  
    public float nightDuration = 360f; 
    private float cycleTimer = 0f;
    public bool isDay = true;

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
            Debug.Log(isDay ? "Wsta� dzie� - bezpiecznie!" : "Zapad�a noc - uwa�aj!");
        }
    }

    void doOnDelay(ref float timer, float delay, System.Action fn)
    {
        timer += Time.deltaTime;
        if (timer >= delay)
        {
            fn();
            timer = 0f;
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

        int multiplier = UnityEngine.Random.Range(1, monsterScript.spawnMultiplayer + 1);

        int columns = Mathf.CeilToInt(Mathf.Sqrt(multiplier));
        float spacing = 2f;

        for (int i = 0; i < multiplier; i++)
        {
            int row = i / columns;
            int col = i % columns;

            float xOffset = (col - (columns - 1) / 2f) * spacing;
            float zOffset = (row - (columns - 1) / 2f) * spacing;

            // Ustalamy punkt testowy wysoko w powietrzu (np. Y = 50), żeby promień leciał przez całą wysokość wyspy
            Vector3 rawSpawnPosition = new Vector3(
                packCenter.x + xOffset,
                50f,
                packCenter.z + zOffset
            );

            // Zwiększamy promień szukania (Extents) do 100 jednostek, żeby na pewno sięgnął gruntu z nieba
            if (NavMesh.SamplePosition(rawSpawnPosition, out NavMeshHit hit, 100f, NavMesh.AllAreas))
            {
                // Znaleziono siatkę! Rodzimy potwora idealnie na ziemi
                GameObject clone = Instantiate(monsterPrefab, hit.position, Quaternion.identity);
                clone.name = monsterPrefab.name;
                clone.GetComponent<BasicMonster>().targets = targets;

                activeMonsters.Add(clone);
            }
            else
            {
                // Jeśli punkt wylosował się w głębokiej wodzie daleko za wyspą, gdzie nie ma NavMesh:
                Debug.LogWarning($"Punkt spawnu [{rawSpawnPosition.x}, {rawSpawnPosition.z}] wypadł poza siatką NavMesh! Pomijam ten spawn.");
            }
        }
    }

    public void ClearAllMonsters()
    {
        foreach (GameObject monster in activeMonsters)
        {
            if (monster != null)
            {
                Destroy(monster);
            }
        }
        activeMonsters.Clear();
    }

    void SpawnAllMonsters(){
         foreach (var monsterPrefab in monsters) 
            {
                BasicMonster monsterScript = monsterPrefab.GetComponent<BasicMonster>();

                if (monsterScript != null) 
                    if (UnityEngine.Random.value <= monsterScript.spawningChance)
                        SpawnMonster(monsterPrefab, monsterScript);
                   
            }
    }

    void Update()
    {
        HandleTimeCycle();

        if (isSpawnMonsters && !isDay) 
        {
           spawnTimer += Time.deltaTime;
           if (spawnTimer >= 2f){
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
