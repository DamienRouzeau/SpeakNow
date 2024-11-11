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
    public DoorController doorController;

    private CharacterController characterController;
    private PlayerInputActions inputActions;
    private Vector2 movementInput;
    private Vector2 lookInput;
    private bool isRunning;
    private bool isJumping = false;
    private Vector3 velocity;
    private bool isGrounded;
    public bool isRagdoll = false;
    [SerializeField]
    private ParticleSystem walkParticle;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private InventorySystem inventorySystem;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        inventorySystem = GetComponent<InventorySystem>(); // Récupère l'inventaire du joueur

        if (characterController == null)
            Debug.LogError("CharacterController is not assigned.");
        if (animator == null)
            Debug.LogError("Animator is not assigned in ThirdPersonController.");
        if (inventorySystem == null)
            Debug.LogError("InventorySystem is not assigned in ThirdPersonController.");

        inputActions = new PlayerInputActions();
        inputActions.PlayerControls.SetCallbacks(this);

        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

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
        if (isRagdoll) return;

        isGrounded = characterController.isGrounded;
        animator.SetBool("isJumping", isJumping);
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (!isJumping)
        {
            Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);
            move = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0) * move;

            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            characterController.Move(move * currentSpeed * Time.deltaTime);

            animator.SetFloat("Speed", move.magnitude * currentSpeed);

            if (move.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        Vector3 checkMove = new Vector3(movementInput.x, 0, movementInput.y);
        float checkSpeed = isRunning ? runSpeed : walkSpeed;
        if (!walkParticle.isEmitting && checkMove.magnitude * checkSpeed > 2.1f && isGrounded)
        {
            walkParticle.Play();
        }
        else if (checkMove.magnitude * checkSpeed < 2.1 || !isGrounded)
        {
            walkParticle.Stop();
        }

        velocity.y += Physics.gravity.y * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        RotateCamera();
    }

    void RotateCamera()
    {
        float rotationX = lookInput.x * rotationSpeed * Time.deltaTime;
        float rotationY = lookInput.y * rotationSpeed * Time.deltaTime;

        cameraTransform.Rotate(-rotationY, 0, 0);
        cameraTransform.Rotate(0, rotationX, 0, Space.World);
    }
    
    public void PunchAlien()
    {
        if (animator != null)
        {
            animator.SetTrigger("AttackTrigger");
        }
    }

    private void ToggleRagdoll(bool state)
    {
        if (animator != null) animator.enabled = !state;
        if (characterController != null) characterController.enabled = !state;

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb != null) rb.isKinematic = !state;
        }

        foreach (Collider col in ragdollColliders)
        {
            if (col != null && col != characterController && col.gameObject.tag != "InteractionArea")
            {
                col.enabled = state;
            }
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
        yield return new WaitForSeconds(3f);
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
            animator.SetTrigger("Jumping");
            velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
            //StartCoroutine(ApplyJumpForce(0.7f));
        }
    }

    /*private IEnumerator ApplyJumpForce(float delay)
    {
        yield return new WaitForSeconds(delay);
        velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
        isJumping = false;
        animator.SetBool("isJumping", false);
    }*/

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

            // Vérifie si l'objet en main est un "maïs" avant d'ouvrir la porte
            if (doorController != null && inventorySystem.itemInHand != null && inventorySystem.itemInHand.GetItemName() == "Maïs")
            {
                doorController.ToggleDoor();
            }
        }
    }
}
