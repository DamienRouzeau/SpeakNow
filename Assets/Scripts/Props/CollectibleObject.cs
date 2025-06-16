using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;


public class CollectibleObject : MonoBehaviour
{
    private InteractionManager interactionManager;
    private GameObject player;
    [SerializeField]
    public string itemName;
    [SerializeField]
    private bool isStackable = false;
    public Rigidbody rb;
    [SerializeField] private size objectSize = size.normal;
    public AnchorSystem currentAnchor;
    private void Start()
    {
        interactionManager = InteractionManager.instance;
    }
    
    public bool IsRecoverable()
    {
        return currentAnchor == null || currentAnchor.isRecoverable;
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
            player = other.gameObject.transform.parent.gameObject;
            if (player.GetComponent<ThirdPersonController>().GetSize() != objectSize)
            {
                player = null;
                return;
            }
            interactionManager = InteractionManager.instance;
            if (interactionManager != null)
            {
                interactionManager.Sub(Collect, transform);
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
                if (currentAnchor != null)
                {
                    currentAnchor.SetOccupied(false);
                    currentAnchor = null;
                }
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
    
    public void SetSize(size _size)
    {
        objectSize = _size;
    }

    #region Size
    public void GetBigger()
    {
        if(objectSize != size.big)
        {
            objectSize++;
        }
    }

    public void GetSmaller()
    {
        if (objectSize != size.little)
        {
            objectSize--;
        }
    }
    #endregion


    #region Getter
    public size GetSize() { return objectSize; }
    #endregion
}
