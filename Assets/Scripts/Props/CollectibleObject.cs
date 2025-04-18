using UnityEngine;

public class CollectibleObject : MonoBehaviour
{
    private InteractionManager interactionManager;
    private GameObject player;
    [SerializeField]
    public string itemName;
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
            interactionManager = InteractionManager.instance;
            if (interactionManager != null)
            {
                interactionManager.Sub(Collect, transform);
                player = other.gameObject.transform.parent.gameObject;
            }
            else
            {
                Debug.LogError("InteractionManager instance is null in CollectibleObject.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("InteractionArea"))
        {
            if (interactionManager != null)
            {
                interactionManager.Unsub(Collect, transform);
            }
            player = null;
        }
    }

    public void Collect()
    {

        if (player == null)
        {
            Debug.LogError("Le joueur n'est pas défini dans Collect(). Échec de la collecte pour : " + itemName);
            return;
        }

        ThirdPersonController playerController = player.GetComponent<ThirdPersonController>();

        // Bloque seulement la collecte si le joueur est en ragdoll
        if (playerController != null && playerController.isRagdoll)
        {
            return; // Annule la collecte si en ragdoll
        }

        InventorySystem inventory = player.GetComponent<InventorySystem>();

        if (inventory != null)
        {
            rb.isKinematic = true; // Rendre l'objet cinématique après la collecte

            if (isStackable)
            {
                inventory.AddStackableItemToInventory(this);
                gameObject.SetActive(false); // Désactive l'objet empilable
            }
            else
            {
                inventory.AddItemInHand(this);
            }

            // Déclenche la poursuite de l'alien uniquement si l'objet est le diamant
            if (itemName == "Diamant")
            {
                AlienController alien = FindObjectOfType<AlienController>();
                if (alien != null && !alien.IsKnockedOut())
                {
                    alien.StartPursuing();
                }
            }
        }
        else
        {
            Debug.LogError("InventorySystem non trouvé sur le joueur.");
        }
    }

}
