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
    private bool hasDiamond = false;
    private Animator animator;
    public Transform handTransform;
    private InteractionManager interactionManager;

    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private CapsuleCollider capsuleCollider;
    private NavMeshAgent agent;

    void Start()
    {
        Debug.Log("AlienController Start() appelé.");
        
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
        Debug.Log("ToggleRagdoll appelé avec état : " + state);
        if (animator != null) animator.enabled = !state;

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb != null) rb.isKinematic = !state;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col != null && col != capsuleCollider && col.gameObject.tag != "InteractionArea")
            {
                col.enabled = state;
            }
        }

        agent.enabled = !state;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InteractionArea") && InteractionManager.instance != null)
        {
            InteractionManager.instance.Sub(KnockOut, transform);
            Debug.Log("Le joueur est proche de l'alien - abonné pour l'interaction.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("InteractionArea") && interactionManager != null)
        {
            interactionManager.Unsub(KnockOut, transform);
            Debug.Log("Le joueur s'éloigne de l'alien - désabonné de l'interaction.");
        }
    }

    public void KnockOut()
    {
        if (!isKnockedOut)
        {
            Debug.Log("KnockOut appelé - tentative de mise KO.");
            Vector3 rayStart = player.position + Vector3.up * 1.5f;
            ThirdPersonController playerController = player.GetComponent<ThirdPersonController>();
            
            Vector3 targetPoint = transform.position + Vector3.up * 1.0f;
            Vector3 directionToAlien = (targetPoint - rayStart).normalized;
            float distanceToAlien = Vector3.Distance(rayStart, targetPoint);

            Debug.DrawRay(rayStart, directionToAlien * distanceToAlien, Color.red, 60.0f);
            if (playerController != null)
            {
                playerController.PunchAlien(); // Déclenche le coup de poing du joueur
            }
            
            if (Physics.Raycast(rayStart, directionToAlien, out RaycastHit hit, distanceToAlien))
            {
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    // Vérification d'angle supplémentaire pour s'assurer que le joueur regarde vraiment vers l'alien
                    Vector3 playerForward = player.forward;
                    float angle = Vector3.Angle(playerForward, directionToAlien);

                    // Si l'angle est inférieur à 45 degrés, alors l'alien est en face
                    if (angle < 45.0f)
                    {
                        // Déclencher le coup de poing uniquement si l'angle et le Raycast sont valides
                        if (playerController != null)
                        {
                            playerController.PunchAlien(); // Déclenche le coup de poing du joueur
                        }

                        Debug.Log("KnockOut appelé - Alien va passer en mode KO.");
                        isKnockedOut = true;
                        isPursuing = false;
                        isAttacking = false;
                        Invoke(nameof(ActivateRagdoll), 0.4f);
                    }
                    else
                    {
                        Debug.Log("Le joueur doit se tourner vers l'alien pour le frapper.");
                    }
                }
                else
                {
                    Debug.Log("Le joueur n'est pas directement face à l'alien.");
                }
            }
            else
            {
                Debug.Log("Le joueur n'est pas face à l'alien pour le frapper.");
            }
        }
    }

    private void ActivateRagdoll()
    {
        Debug.Log("ActivateRagdoll appelé.");
        ToggleRagdoll(true);
        agent.enabled = false;
        DropDiamond();
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
            Debug.Log("L'alien poursuit le joueur.");
            PursuePlayer();
            if (Vector3.Distance(transform.position, player.position) < attackRange && !isAttacking)
            {
                AttackPlayer();
            }
        }
    }

    void PursuePlayer()
    {
        if (!agent.enabled)
        {
            Debug.Log("Activation de l'agent NavMesh.");
            agent.enabled = true;
            agent.speed = pursuitSpeed;
        }

        Debug.Log("Mise à jour de la destination vers la position du joueur.");
        agent.SetDestination(player.position);

        animator.SetFloat("Speed", pursuitSpeed);
        animator.SetBool("isWalking", true);
    }

    void AttackPlayer()
    {
        if (isAttacking) return;

        Debug.Log("Alien attaque le joueur.");
        isAttacking = true;
        agent.isStopped = true;

        animator.SetTrigger("AttackTrigger");
        Invoke(nameof(ResetAttack), 1.0f);

        ThirdPersonController playerController = player.GetComponent<ThirdPersonController>();
        if (playerController != null)
        {
            playerController.EnterRagdoll();

            InventorySystem inventory = player.GetComponent<InventorySystem>();
            if (inventory != null && inventory.itemInHand != null && inventory.itemInHand.GetItemName() == "Diamant")
            {
                inventory.RemoveItemInHand();
                //Rigidbody diamondRb = diamond.GetComponent<Rigidbody>();
                //if (diamondRb != null) diamondRb.isKinematic = false;

                //inventory.itemInHand.transform.parent = null;
                //inventory.itemInHand = null;
                //Debug.Log("Le diamant a été retiré de la main du joueur.");
            }
        }

        isPursuing = false;
        StartCoroutine(CheckAndFetchDiamond());
    }

    private void ResetAttack()
    {
        Debug.Log("Réinitialisation de l'attaque.");
        isAttacking = false;
        agent.isStopped = false;
    }

    IEnumerator CheckAndFetchDiamond()
    {
        if (isKnockedOut) yield break;

        if (!hasDiamond && diamond.transform.parent == null)
        {
            Debug.Log("L'alien cherche le diamant.");
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
            Debug.Log("Le diamant est attaché à la main de l'alien.");
            diamond.transform.parent = handTransform;
            diamond.transform.localPosition = Vector3.zero;
            diamond.transform.localRotation = Quaternion.identity;

            Rigidbody diamondRb = diamond.GetComponent<Rigidbody>();
            if (diamondRb != null) diamondRb.isKinematic = true;

            Collider diamondCollider = diamond.GetComponent<Collider>();
            if (diamondCollider != null) diamondCollider.enabled = false;

            hasDiamond = true;
        }
    }

    IEnumerator ReturnToOriginalPosition()
    {
        if (isKnockedOut) yield break;

        Debug.Log("L'alien retourne à sa position d'origine.");
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
            Debug.Log("Le diamant est lâché.");
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

            hasDiamond = false;
        }
    }

    public void StartPursuing()
    {
        if (!isKnockedOut)
        {
            Debug.Log("L'alien commence à poursuivre.");
            isPursuing = true;
        }
    }

    public bool IsKnockedOut()
    {
        return isKnockedOut;
    }
}
