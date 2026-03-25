using UnityEngine;
using System;
using System.Collections;


public class GameMaster : MonoBehaviour
{

    public GameObject basicMonsterPrefab;
    public GameObject smallMonsterPrefab;
    public GameObject[] targets = new GameObject[2];


    public Transform centerPoint;
    public float spawnRadius = 10f;

    public float spawnHeight = 0.2f;

    public float minDistance = 15f; 
    public float maxDistance = 30f;

    private float basicTimer = 0f;
    private float smallTimer = 0f;


    public bool isDay = false;


    void doOnDelay(ref float timer, float delay, System.Action fn)
    {
        timer += Time.deltaTime;
        if (timer >= delay)
        {
            fn();
            timer = 0f;
        }
    }

    public void SpawnMonster(GameObject monsterPrefab, string name,  int multiplier)
    {
        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;

        float randomDistance = UnityEngine.Random.Range(minDistance, maxDistance);

        Vector2 finalOffset = randomDirection * randomDistance;

       
        //TODO: Improve spawing system.
        //TODO: Stop spawning when game ended/ no centerPoint
        //TODO: Spawn objects in water
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
        }
    }


    void Update()
    {
        doOnDelay(ref basicTimer, 3f, () => SpawnMonster(basicMonsterPrefab, "basicMonstar", 1));
        doOnDelay(ref smallTimer, 6f, () => SpawnMonster(smallMonsterPrefab, "smallMonstar", 5));
    }
}
