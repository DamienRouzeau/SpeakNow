using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AlienController : MonoBehaviour
{
    public Transform originalPosition;
    public GameObject diamond;
    public float pursuitSpeed = 5f;
    public float returnSpeed = 2f;
    public float attackRange = 2f;
    public Transform player;
    private bool isKnockedOut = false;
    private bool isPursuing = false;
    private bool isAttacking = false;
    private bool hasDiamond = false; // Nouveau booléen pour savoir si l'alien a le diamant
    private Animator animator;
    public Transform handTransform;
    private InteractionManager interactionManager;

    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private CapsuleCollider capsuleCollider;
    private NavMeshAgent agent;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false; // Désactiver l'agent au démarrage
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        if (player == null || diamond == null || originalPosition == null || handTransform == null)
            Debug.LogError("Assurez-vous d'assigner toutes les références dans l'inspecteur.");

        ToggleRagdoll(false); // Désactive le mode ragdoll au démarrage
        interactionManager = InteractionManager.instance;
    }

    private void ToggleRagdoll(bool state)
    {
        if (animator != null) animator.enabled = !state;

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb != null)
            {
                rb.isKinematic = !state;
            }
        }

        var mainRigidbody = GetComponent<Rigidbody>();
        if (mainRigidbody != null)
        {
            mainRigidbody.isKinematic = !state;
            mainRigidbody.useGravity = state;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col != null && col != capsuleCollider && col.gameObject.tag != "InteractionArea")
            {
                col.enabled = state;
            }
        }
    }

    void OnDestroy()
    {
        if (interactionManager != null)
        {
            interactionManager.Unsub(KnockOut);
            Debug.Log("Alien désabonné de l'interaction.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InteractionArea") && InteractionManager.instance != null)
        {
            InteractionManager.instance.Sub(KnockOut, 2); // Priorité haute pour l'alien
            Debug.Log("Le joueur est proche de l'alien - abonné pour l'interaction.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("InteractionArea") && interactionManager != null)
        {
            interactionManager.Unsub(KnockOut);
            Debug.Log("Le joueur s'éloigne de l'alien - désabonné de l'interaction.");
        }
    }

    public void KnockOut()
    {
        if (!isKnockedOut)
        {
            Debug.Log("KnockOut appelé - Alien va passer en mode KO.");
            isKnockedOut = true;
            isPursuing = false;
            isAttacking = false;
            ToggleRagdoll(true); // Passe en mode ragdoll
            agent.enabled = false; // Désactiver le NavMeshAgent en mode ragdoll
            DropDiamond(); // Détache le diamant et le rend collectable
        }
    }

    void Update()
    {
        if (isKnockedOut) return;

        ThirdPersonController playerController = player.GetComponent<ThirdPersonController>();
        if (playerController != null && playerController.isRagdoll)
        {
            if (isPursuing)
            {
                isPursuing = false;
                StartCoroutine(CheckAndFetchDiamond());
            }
            return;
        }

        if (isPursuing && !isKnockedOut)
        {
            PursuePlayer();
            if (Vector3.Distance(transform.position, player.position) < attackRange && !isAttacking)
            {
                AttackPlayer();
            }
        }
        
        if (agent.enabled)
        {
            UpdatePositionAndRotation();
        }
    }

    void PursuePlayer()
    {
        if (!agent.enabled)
        {
            agent.enabled = true;
            agent.speed = pursuitSpeed;
        }
        
        agent.SetDestination(player.position);
        
        animator.SetFloat("Speed", pursuitSpeed);
        animator.SetBool("isWalking", true);
    }

    void AttackPlayer()
    {
        if (isAttacking) return;

        isAttacking = true;
        agent.isStopped = true; // Arrêter le NavMeshAgent pendant l'attaque

        // Déclencher l'animation d'attaque
        animator.SetTrigger("AttackTrigger");

        // Délai pour la fin de l'animation d'attaque avant de réactiver le mouvement
        Invoke(nameof(ResetAttack), 1.0f); // Ajustez la durée selon la longueur de l'animation

        // Interagir avec le joueur
        ThirdPersonController playerController = player.GetComponent<ThirdPersonController>();
        if (playerController != null)
        {
            playerController.EnterRagdoll();

            InventorySystem inventory = player.GetComponent<InventorySystem>();
            if (inventory != null && inventory.itemInHand != null && inventory.itemInHand.GetItemName() == "Diamant")
            {
                Rigidbody diamondRb = diamond.GetComponent<Rigidbody>();
                if (diamondRb != null) diamondRb.isKinematic = false;

                inventory.itemInHand.transform.parent = null;
                inventory.itemInHand = null;
                Debug.Log("Le diamant a été retiré de la main du joueur.");
            }
        }

        isPursuing = false;
        StartCoroutine(CheckAndFetchDiamond());
    }

    // Réinitialise l'état d'attaque pour permettre de nouvelles attaques
    private void ResetAttack()
    {
        isAttacking = false;
        agent.isStopped = false; // Réactiver le NavMeshAgent pour le mouvement
    }

    IEnumerator CheckAndFetchDiamond()
    {
        if (isKnockedOut) yield break; // Ne rien faire si l'alien est KO

        if (!hasDiamond && diamond.transform.parent == null) // Alien ne possède pas encore le diamant
        {
            agent.enabled = true;
            agent.speed = returnSpeed;
            agent.SetDestination(diamond.transform.position);

            while (!isKnockedOut && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance))
            {
                animator.SetFloat("Speed", returnSpeed);
                animator.SetBool("isWalking", true);
                yield return null;
            }

            if (!isKnockedOut)
            {
                AttachDiamondToHand();
            }
        }

        if (!isKnockedOut)
        {
            StartCoroutine(ReturnToOriginalPosition());
        }
    }
    
    private void AttachDiamondToHand()
    {
        if (handTransform != null)
        {
            diamond.transform.parent = handTransform;
            diamond.transform.localPosition = Vector3.zero;
            diamond.transform.localRotation = Quaternion.identity;

            Rigidbody diamondRb = diamond.GetComponent<Rigidbody>();
            if (diamondRb != null) diamondRb.isKinematic = true;

            Collider diamondCollider = diamond.GetComponent<Collider>();
            if (diamondCollider != null) diamondCollider.enabled = false;

            hasDiamond = true; // Marquer que l'alien possède le diamant
            Debug.Log("Le diamant est maintenant attaché solidement à la main de l'alien.");
        }
    }

    IEnumerator ReturnToOriginalPosition()
    {
        if (isKnockedOut) yield break; // Ne rien faire si l'alien est KO

        agent.enabled = true;
        agent.speed = returnSpeed;
        agent.SetDestination(originalPosition.position);

        while (!isKnockedOut && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance))
        {
            animator.SetFloat("Speed", returnSpeed);
            animator.SetBool("isWalking", true);
            yield return null;
        }

        if (!isKnockedOut)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("isWalking", false);
        
            DropDiamond();
        }
    }

    private void DropDiamond()
    {
        if (diamond != null)
        {
            diamond.transform.parent = null;

            Rigidbody diamondRb = diamond.GetComponent<Rigidbody>();
            if (diamondRb != null)
            {
                diamondRb.isKinematic = false;
                diamondRb.AddForce(transform.forward * 3f + Vector3.up, ForceMode.Impulse);
            }

            Collider diamondCollider = diamond.GetComponent<Collider>();
            if (diamondCollider != null)
            {
                diamondCollider.enabled = true;
            }

            CollectibleObject collectible = diamond.GetComponent<CollectibleObject>();
            if (collectible != null)
            {
                collectible.enabled = true;
            }

            hasDiamond = false; // L'alien ne possède plus le diamant
            Debug.Log("Le diamant a été lâché et est désormais un collectible.");
        }
    }

    public void StartPursuing()
    {
        if (!isKnockedOut)
        {
            isPursuing = true;
        }
    }

    public bool IsKnockedOut()
    {
        return isKnockedOut;
    }

    private void UpdatePositionAndRotation()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 2f, LayerMask.GetMask("Default")))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);

            Vector3 forward = agent.velocity.normalized;
            if (forward.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(forward);
            }
        }
    }
}
