using UnityEngine;

public class CollectibleObject : MonoBehaviour
{
    private InteractionManager interactionManager;
    private GameObject player;
    [SerializeField]
    private string itemName;
    [SerializeField]
    private bool isStackable = false;
    public Rigidbody rb;

    private void Start()
    {
        interactionManager = InteractionManager.instance;
    }

    private void FixedUpdate()
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
        if (other.CompareTag("InteractionArea"))
        {
            interactionManager.Sub(Collect);
            player = other.gameObject.transform.parent.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("InteractionArea"))
        {
            interactionManager.Unsub(Collect);
            player = null;
        }
    }

    public void Collect()
    {
        if (player != null)
        {
            InventorySystem inventory = player.GetComponent<InventorySystem>();
            ThirdPersonController playerController = player.GetComponent<ThirdPersonController>();

            // Vérifiez si le joueur n'est pas en ragdoll
            if (inventory != null && !playerController.isRagdoll)
            {
                rb.isKinematic = true; // Rendre le collectible cinématique après la collecte

                if (isStackable)
                {
                    inventory.AddStackableItemToInventory(this);
                }
                else
                {
                    inventory.AddItemInHand(this);
                }

                // Désactiver l'objet après la collecte
                gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("Cannot collect the item while in ragdoll.");
            }
        }
    }
}
