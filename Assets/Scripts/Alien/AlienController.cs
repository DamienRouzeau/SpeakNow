using UnityEngine;
using System.Collections;

public class AlienController : MonoBehaviour
{
    public Transform originalPosition;
    public GameObject diamond;
    public float pursuitSpeed = 5f;
    public float attackRange = 2f;
    public Transform player;
    private bool isKnockedOut = false;
    private bool isPursuing = false;
    private Animator animator;

    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("Animator is not assigned in AlienController.");

        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        if (player == null)
            Debug.LogError("Player reference is not assigned in AlienController.");
        if (diamond == null)
            Debug.LogError("Diamond reference is not assigned in AlienController.");
        if (originalPosition == null)
            Debug.LogError("Original position is not assigned in AlienController.");

        ToggleRagdoll(false);
    }

    void ToggleRagdoll(bool state)
    {
        if (animator != null)
            animator.enabled = !state;
    
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb != null) rb.isKinematic = !state;
        }
        foreach (Collider col in ragdollColliders)
        {
            if (col != null) col.enabled = state;
        }
    }

    public void KnockOut()
    {
        if (!isKnockedOut)
        {
            isKnockedOut = true;
            isPursuing = false;
            ToggleRagdoll(true);
            StartCoroutine(WakeUp());
        }
    }

    IEnumerator WakeUp()
    {
        yield return new WaitForSeconds(5f); // Temps d'inconscience
        ToggleRagdoll(false);
        isKnockedOut = false;
    }

    void Update()
    {
        if (isPursuing && !isKnockedOut)
        {
            PursuePlayer();
            if (Vector3.Distance(transform.position, player.position) < attackRange)
            {
                AttackPlayer();
            }
        }
    }

    void PursuePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Assure que l'alien ne soulève pas le joueur en l'air
        transform.position += direction * pursuitSpeed * Time.deltaTime;
        animator.SetBool("isWalking", true);
    }

    void AttackPlayer()
    {
        ThirdPersonController playerController = player.GetComponent<ThirdPersonController>();
        if (playerController != null)
        {
            playerController.EnterRagdoll(); // Met le joueur en ragdoll
        }
        
        diamond.transform.parent = transform; // Récupère le diamant
        diamond.transform.localPosition = Vector3.zero;
        isPursuing = false;
        StartCoroutine(ReturnToOriginalPosition());
    }

    IEnumerator ReturnToOriginalPosition()
    {
        yield return new WaitForSeconds(2f);
        while (Vector3.Distance(transform.position, originalPosition.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, originalPosition.position, pursuitSpeed * Time.deltaTime);
            yield return null;
        }
        animator.SetBool("isWalking", false);
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
}
