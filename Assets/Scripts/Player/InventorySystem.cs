using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public List<string> stackableInventory;
    public CollectibleObject itemInHand;
    [SerializeField]
    private GameObject hand;
    [SerializeField]
    private float throwStrenght = 3;

    public void AddStackableItemToInventory(CollectibleObject item)
    {
        stackableInventory.Add(item.GetItemName());
        Debug.Log(stackableInventory.Count);
        item.gameObject.SetActive(false);
    }

    public void RemoveStackableItemFromInventory(CollectibleObject item)
    {
        stackableInventory.Remove(item.GetItemName());
        Debug.Log(stackableInventory.Count);
    }

    public void AddItemInHand(CollectibleObject item)
    {
        if (itemInHand != null && itemInHand != item)
        {
            itemInHand.transform.parent = null;
            itemInHand.rb.isKinematic = false;
            itemInHand = null;
        }
        else if (itemInHand != null && itemInHand == item)
        {
            itemInHand.rb.isKinematic = false;
            itemInHand.rb.AddForce(new Vector3(0, throwStrenght, 0) + transform.forward * throwStrenght, ForceMode.Impulse);
            itemInHand.transform.parent = null;
            itemInHand = null;
            return;
        }
        item.transform.parent = hand.transform;
        itemInHand = item;
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = new Quaternion(180,0,0,0);
    }
}
