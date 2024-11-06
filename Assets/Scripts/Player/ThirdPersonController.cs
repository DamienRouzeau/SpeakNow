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

    // Callback pour l'interaction avec les portes et le diamant
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
            Debug.LogError("CharacterController is not assigned.");

        inputActions = new PlayerInputActions();
        inputActions.PlayerControls.SetCallbacks(this);

        if (animator == null)
            Debug.LogError("Animator is not assigned in ThirdPersonController.");
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

    // Fonction pour activer le mode ragdoll lors d'une attaque
    public void EnterRagdoll()
    {
        if (!isRagdoll)
        {
            isRagdoll = true;
            characterController.enabled = false; // Désactive CharacterController pendant le ragdoll
            animator.enabled = false;
            StartCoroutine(ExitRagdoll());
        }
    }

    private IEnumerator ExitRagdoll()
    {
        yield return new WaitForSeconds(3f); // Temps en ragdoll
        isRagdoll = false;
        characterController.enabled = true; // Réactive le CharacterController après le ragdoll
        animator.enabled = true;
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
