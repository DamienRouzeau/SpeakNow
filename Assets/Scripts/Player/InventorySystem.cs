using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    private static InventorySystem Instance { get; set; }
    public static InventorySystem instance => Instance;

    
    public List<string> stackableInventory = new List<string>();
    public CollectibleObject itemInHand;
    [SerializeField]
    private GameObject hand;
    [SerializeField]
    private Transform throwPosition;
    [SerializeField]
    private float throwStrength = 3f;
    
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    
    
    public void AddStackableItemToInventory(CollectibleObject item)
    {
        stackableInventory.Add(item.GetItemName());
        item.gameObject.SetActive(false); // Désactive l'objet après la collecte
    }

    public void AddItemInHand(CollectibleObject item)
    {
        if (itemInHand != null && itemInHand != item)
        {
            itemInHand.transform.parent = null;
            itemInHand.rb.isKinematic = false;
            itemInHand.GetComponent<Collider>().enabled = true; // Réactiver le Collider
            itemInHand = null;
        }
        else if (itemInHand != null && itemInHand == item)
        {
            itemInHand.rb.isKinematic = false;
            itemInHand.GetComponent<Collider>().enabled = true; // Réactiver le Collider
            itemInHand.rb.AddForce(new Vector3(0, throwStrength, 0) + transform.forward * throwStrength, ForceMode.Impulse);
            itemInHand.transform.parent = null;
            itemInHand = null;
            return;
        }

        item.GetComponent<Collider>().enabled = false; // Désactiver le Collider lorsqu'il est en main
        item.transform.parent = hand.transform;
        itemInHand = item;
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.Euler(180, 0, 0);

        InteractionManager interaction = InteractionManager.instance;
        //interaction.interactibleObjects.Unsub(interaction.);
    }

    public void RemoveItemInHand()
    {
        if (itemInHand != null)
        {
            itemInHand.rb.isKinematic = false;
            itemInHand.rb.useGravity = true;
            itemInHand.transform.parent = null;
            itemInHand.transform.position = throwPosition.position;
            itemInHand.GetComponent<Collider>().enabled = true; // Réactiver le Collider
            itemInHand.rb.AddForce(new Vector3(0, throwStrength, 0) + transform.forward * throwStrength, ForceMode.Impulse);

            itemInHand = null;
        }
    }

    public void DestroyItemInHand()
    {
        if (itemInHand != null)
        {
            itemInHand.transform.parent = null;
            itemInHand.rb.isKinematic = false;
            Destroy(itemInHand.gameObject);
            itemInHand = null;
        }
    }
}
