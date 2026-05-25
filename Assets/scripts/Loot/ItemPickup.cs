using UnityEngine;



public class ItemPickup : MonoBehaviour
{

    public ItemData itemType;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Coś weszło w trigger: " + other.name); // To pokaże w konsoli, co dokładnie dotknęło przedmiotu

        if (other.CompareTag("Player"))
        {

            if (PlayerInventory.Instance != null && itemType != null)
            {
                PlayerInventory.Instance.AddItem(itemType);
                
                Destroy(gameObject);
            }
        }
    }
}