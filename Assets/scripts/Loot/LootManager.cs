using UnityEngine;
using System.Collections.Generic;

public class LootManager : MonoBehaviour
{
    public List<GameObject> loots = new List<GameObject>();
    
    public static LootManager Instance;

    void Awake() 
    { 
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    
   public GameObject WylosujObiekt() 
    {
        if (loots == null || loots.Count == 0) 
        {
            Debug.LogWarning("Lista dropów jest pusta!");
            return null;
        }

        int losowyIndeks = Random.Range(0, loots.Count);
        return loots[losowyIndeks];
    }

    public void SpawnLoot(Vector3 position, float dropChance) 
    {
        if (UnityEngine.Random.value > dropChance) 
            return; 

        GameObject itemToSpawn = WylosujObiekt();
        if (itemToSpawn != null) 
            Instantiate(itemToSpawn, position, Quaternion.identity); 
    }
}
