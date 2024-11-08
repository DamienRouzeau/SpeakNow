using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using static UnityEngine.ParticleSystem;

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

    private CharacterController characterController;
    private PlayerInputActions inputActions;
    private Vector2 movementInput;
    private Vector2 lookInput;
    private bool isRunning;
    private bool isJumping = false;
    private Vector3 velocity;
    private bool isGrounded;
    [SerializeField]
    private ParticleSystem walkParticle;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();
        inputActions.PlayerControls.SetCallbacks(this);
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
        // Vérifier si le personnage est au sol
        isGrounded = characterController.isGrounded;

        // Mettre à jour le booléen isJumping dans l'Animator
        animator.SetBool("isJumping", isJumping);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Applique une petite force pour coller au sol
        }

        // Gestion du mouvement
        if (!isJumping)
        {
            Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);
            move = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0) * move; // Oriente le mouvement selon la caméra

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

        // Applique la gravité
        velocity.y += Physics.gravity.y * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        // Gérer le mouvement de la caméra (sans affecter la rotation du personnage)
        RotateCamera();
    }

    // Gérer la rotation de la caméra
    void RotateCamera()
    {
        float rotationX = lookInput.x * rotationSpeed * Time.deltaTime;
        float rotationY = lookInput.y * rotationSpeed * Time.deltaTime;

        // Appliquer la rotation verticale uniquement à la caméra (et non au personnage)
        cameraTransform.Rotate(-rotationY, 0, 0);
        cameraTransform.Rotate(0, rotationX, 0, Space.World); // Rotation sur l'axe Y pour regarder autour
    }

    // Callback pour le mouvement
    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    // Callback pour capturer l'input de la caméra
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    // Callback pour le saut
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            if (!isJumping) animator.SetTrigger("Jump");
            isJumping = true;
            //animator.SetBool("isJumping", true);
            velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
            isJumping = false;
            //animator.SetBool("isJumping", false);
            //StartCoroutine(ApplyJumpForce(0.7f));
        }
    }

    // Coroutine pour appliquer la force de saut après un délai
    private IEnumerator ApplyJumpForce(float delay)
    {
        yield return new WaitForSeconds(delay);
        velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
        isJumping = false;
        animator.SetBool("isJumping", false);
    }

    // Callback pour activer/désactiver la course
    public void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.performed;
        walkParticle.Play();
    }

    // Callback pour l'interaction avec les portes
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            InteractionManager.instance.Interact();
        }
    }
}
