using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public List<string> stackableInventory = new List<string>();
    public CollectibleObject itemInHand;
    [SerializeField]
    private GameObject hand;
    [SerializeField]
    private float throwStrength = 3;

    public void AddStackableItemToInventory(CollectibleObject item)
    {
        stackableInventory.Add(item.GetItemName());
        Debug.Log($"Stackable item added. Total count: {stackableInventory.Count}");
        item.gameObject.SetActive(false); // Désactive l'objet après la collecte
    }

    public void AddItemInHand(CollectibleObject item)
    {
        if (itemInHand != null && itemInHand != item)
        {
            // Si un autre objet est en main, retirez-le
            itemInHand.transform.parent = null;
            itemInHand.rb.isKinematic = false;
            itemInHand = null;
        }
        else if (itemInHand != null && itemInHand == item)
        {
            // Si c'est le même objet, le lancer
            itemInHand.rb.isKinematic = false;
            itemInHand.rb.AddForce(new Vector3(0, throwStrength, 0) + transform.forward * throwStrength, ForceMode.Impulse);
            itemInHand.transform.parent = null;
            itemInHand = null;
            return;
        }

        // Ajoutez le nouvel objet à la main
        item.transform.parent = hand.transform;
        itemInHand = item;
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.Euler(180, 0, 0); // Rotation appropriée
    }
}