using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameMaster : MonoBehaviour
{
    public static GameMaster Instance;

    [Header("Monster Settings")]
    public GameObject basicMonsterPrefab;
    public GameObject smallMonsterPrefab;
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

    private float basicTimer = 0f;
    private float smallTimer = 0f;
    public bool isSpawnMonsters = true;

    private List<GameObject> activeMonsters = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public void EndGame()
    {
        isSpawnMonsters = false;
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

            sunLight.intensity = isDay ? 1f : 0.1f;
        }

        if (cycleTimer >= currentLimit)
        {
            isDay = !isDay;
            cycleTimer = 0f;
            Debug.Log(isDay ? "Wsta³ dzieñ - bezpiecznie!" : "Zapad³a noc - uwa¿aj!");
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

    public void SpawnMonster(GameObject monsterPrefab, string name,  int multiplier)
    {
        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;

        float randomDistance = UnityEngine.Random.Range(minDistance, maxDistance);

        Vector2 finalOffset = randomDirection * randomDistance;

       
        //TODO: Improve clowd spawing system.
        for (int i = 0; i < multiplier; i++)
        {
             Vector3 spawnPosition = new Vector3(
                 centerPoint.position.x + finalOffset.x + 2*i,
                 spawnHeight,
                 centerPoint.position.z + finalOffset.y + 2 * i
             );
            GameObject clone = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
            clone.name = name;
            clone.GetComponent<Monster>().targets = targets;

            activeMonsters.Add(clone);
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


    void Update()
    {
        HandleTimeCycle();

        if (isSpawnMonsters && !isDay) 
        {
            doOnDelay(ref basicTimer, 3f, () => SpawnMonster(basicMonsterPrefab, "basicMonstar", 1));
            doOnDelay(ref smallTimer, 6f, () => SpawnMonster(smallMonsterPrefab, "smallMonstar", 5));
        }

        if (isDay && activeMonsters.Count > 0)
        {
            ClearAllMonsters();
        }

    }
}
