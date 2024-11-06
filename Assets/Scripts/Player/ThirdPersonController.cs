using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ThirdPersonController : MonoBehaviour, PlayerInputActions.IPlayerControlsActions
{
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float rotationSpeed = 720f;
    public float jumpForce = 7f;
    public Transform cameraTransform;
    [SerializeField]
    public Animator animator;
    public DoorController doorController; // Référence au DoorController pour gérer les portes
    public GameObject diamond; // Référence au diamant

    private CharacterController characterController;
    private PlayerInputActions inputActions;
    private Vector2 movementInput;
    private Vector2 lookInput;
    private bool isRunning;
    private bool isJumping = false;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isRagdoll = false; // Pour gérer l'état ragdoll

    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (characterController == null)
            Debug.LogError("CharacterController is not assigned.");
        if (animator == null)
            Debug.LogError("Animator is not assigned in ThirdPersonController.");

        inputActions = new PlayerInputActions();
        inputActions.PlayerControls.SetCallbacks(this);

        // Obtenir tous les Rigidbody et Collider pour activer/désactiver le mode ragdoll
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        // Désactiver le mode ragdoll au démarrage
        ToggleRagdoll(false);
    }

    void OnEnable()
    {
        inputActions.PlayerControls.Enable();
    }

    void OnDisable()
    {
        inputActions.PlayerControls.Disable();
    }

    void Update()
    {
        // Si en ragdoll, ignorer le mouvement
        if (isRagdoll) return;

        // Vérifier si le personnage est au sol
        isGrounded = characterController.isGrounded;
        animator.SetBool("isJumping", isJumping);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Applique une petite force pour coller au sol
        }

        // Gestion du mouvement
        if (!isJumping)
        {
            Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);
            move = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0) * move;

            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            characterController.Move(move * currentSpeed * Time.deltaTime);

            // Mise à jour de la vitesse dans l'Animator
            animator.SetFloat("Speed", move.magnitude * currentSpeed);

            if (move.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        // Applique la gravité
        velocity.y += Physics.gravity.y * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        // Gérer la rotation de la caméra
        RotateCamera();
    }

    void RotateCamera()
    {
        float rotationX = lookInput.x * rotationSpeed * Time.deltaTime;
        float rotationY = lookInput.y * rotationSpeed * Time.deltaTime;

        // Appliquer la rotation verticale uniquement à la caméra
        cameraTransform.Rotate(-rotationY, 0, 0);
        cameraTransform.Rotate(0, rotationX, 0, Space.World); // Rotation sur l'axe Y pour regarder autour
    }

    private void ToggleRagdoll(bool state)
    {
        // Désactive l'Animator et le CharacterController si on passe en mode ragdoll
        if (animator != null) animator.enabled = !state;
        if (characterController != null) characterController.enabled = !state;

        // Active ou désactive les Rigidbody et Collider des membres pour le mode ragdoll
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb != null) rb.isKinematic = !state;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col != null && col != characterController) col.enabled = state;
        }

        isRagdoll = state;
    }

    public void EnterRagdoll()
    {
        if (!isRagdoll)
        {
            ToggleRagdoll(true);
            StartCoroutine(ExitRagdoll());
        }
    }

    private IEnumerator ExitRagdoll()
    {
        yield return new WaitForSeconds(10f); // Temps en mode ragdoll
        ToggleRagdoll(false);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            isJumping = true;
            animator.SetBool("isJumping", true);
            StartCoroutine(ApplyJumpForce(0.7f));
        }
    }

    private IEnumerator ApplyJumpForce(float delay)
    {
        yield return new WaitForSeconds(delay);
        velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
        isJumping = false;
        animator.SetBool("isJumping", false);
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.performed;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (InteractionManager.instance != null)
            {
                InteractionManager.instance.Interact();
            }
            else
            {
                Debug.LogWarning("InteractionManager instance is missing.");
            }
        }

        if (context.performed && doorController != null)
        {
            doorController.ToggleDoor();
        }
        else if (context.performed && diamond != null && !isRagdoll)
        {
            AlienController alien = FindObjectOfType<AlienController>();
            if (alien != null && !alien.IsKnockedOut())
            {
                alien.StartPursuing();
            }
            else if (diamond != null)
            {
                diamond.transform.parent = transform;
                diamond.transform.localPosition = Vector3.zero;
            }
        }
        else if (doorController == null)
        {
            Debug.LogWarning("DoorController is not assigned in ThirdPersonController.");
        }
    }
}
