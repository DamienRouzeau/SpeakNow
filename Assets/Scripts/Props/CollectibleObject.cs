using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleObject : MonoBehaviour
{
    private InteractionManager interactionManager;
    private GameObject player;
    public string itemName;
    [SerializeField]
    private bool isStackable = false;
    public Rigidbody rb;

    private void Start()
    {
        interactionManager = InteractionManager.instance;
    }

    public void FixedUpdate()
    {
        if (transform.position.y < -10)
        {
            rb.velocity = Vector3.zero;
            transform.position = new Vector3(transform.position.x, 2, transform.position.z);
        }
    }

    public string GetItemName()
    {
        return itemName;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InteractionArea") && rb.isKinematic == false)
        {
            interactionManager.Sub(Collect, this.gameObject);
            interactionManager.HighlightClosest();
            player = other.gameObject.transform.parent.gameObject;
        }
    }




    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("InteractionArea"))
        {
            interactionManager.Unsub(Collect, this.gameObject);
            player = null;
        }
    }

    public void Collect()
    {
        interactionManager.Unsub(Collect, this.gameObject);

        if (player != null)
        {
            InventorySystem inventory = player.GetComponent<InventorySystem>();
            rb.isKinematic = true;
            if (isStackable)
            {
                inventory.AddStackableItemToInventory(this);
            }
            else
            {
                inventory.AddItemInHand(this);
                rb.useGravity = true;
            }
        }
    }
}
