using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public enum size
{
    little,
    normal,
    big,
}


public class ThirdPersonController : MonoBehaviour, PlayerInputActions.IPlayerControlsActions
{
    [Header("3C")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float rotationSpeed = 720f;
    public float jumpForce = 7f;
    public Transform cameraTransform;
    private CharacterController characterController;
    private PlayerInputActions inputActions;
    private Vector2 movementInput;
    private Vector2 lookInput;
    private bool isRunning;
    private bool isJumping = false;
    private Vector3 velocity;
    private bool isGrounded;
    [SerializeField] private Camera camera;

    [Header("Capacities")]
    [SerializeField] private List<string> capacities = new List<string>();
    private int capacityIndex;
    private bool littleBigCapacity = false;
    [SerializeField] private Vector3 littleSize = new Vector3(0.07f, 0.07f, 0.07f);
    [SerializeField] private Vector3 normalSize = new Vector3(0.15f, 0.15f, 0.15f);
    [SerializeField] private Vector3 bigSize = new Vector3(0.3f, 0.3f, 0.3f);
    private size size = size.normal;
    [SerializeField] private CameraProfil littleCam;
    [SerializeField] private CameraProfil normalCam;
    [SerializeField] private CameraProfil bigCam;
    [HideInInspector] public bool canRotateCamera = true;

    [SerializeField]
    public Animator animator;
    public DoorController doorController;
    public bool isRagdoll = false;
    [SerializeField]
    private ParticleSystem walkParticle;
    private Rigidbody[] ragdollRigidbodies;
    private Collider[] ragdollColliders;
    private InventorySystem inventorySystem;
    [SerializeField]
    private FeetDetectors feet;
    [SerializeField]
    private ParticleSystem particles;

    [Header("Audio")]
    [SerializeField]
    private AudioSource[] stepAudios;
    [SerializeField]
    private AudioSource jumpAudio;

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

        isGrounded = feet.GetGrounded();
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

        velocity.y += 2 * Physics.gravity.y * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        if (canRotateCamera)
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

    public void OnNoteBook(InputAction.CallbackContext context)
    {
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
    #region Capacities

    public void LittlePotion()
    {
        switch (size)
        {
            case size.little:
                return;
            case size.normal:
                particles.Play();
                transform.localScale = littleSize;
                cameraTransform.GetComponent<CameraFreeLook>().GetNewProfile(littleCam);
                if (inventorySystem.itemInHand != null)
                {
                    inventorySystem.itemInHand.GetSmaller();
                }
                InteractionManager.instance.ClearInteractibleObjectList();
                size = size.little;
                break;
            case size.big:
                particles.Play();
                transform.localScale = normalSize;
                cameraTransform.GetComponent<CameraFreeLook>().GetNewProfile(normalCam);
                if (inventorySystem.itemInHand != null)
                {
                    inventorySystem.itemInHand.GetSmaller();
                }
                InteractionManager.instance.ClearInteractibleObjectList();
                size = size.normal;
                break;
            default:
                Debug.LogWarning("Size not found");
                break;
        }
    }

    public void BigPotion()
    {
        switch (size)
        {
            case size.little:
                particles.Play();
                transform.localScale = normalSize;
                cameraTransform.GetComponent<CameraFreeLook>().GetNewProfile(normalCam);
                if (inventorySystem.itemInHand != null)
                {
                    inventorySystem.itemInHand.GetBigger();
                }
                InteractionManager.instance.ClearInteractibleObjectList();
                size = size.normal;
                break;
            case size.normal:
                particles.Play();
                transform.localScale = bigSize;
                cameraTransform.GetComponent<CameraFreeLook>().GetNewProfile(bigCam);
                if (inventorySystem.itemInHand != null)
                {
                    inventorySystem.itemInHand.GetBigger();
                }
                InteractionManager.instance.ClearInteractibleObjectList();
                size = size.big;
                break;
            case size.big:
                return;
            default:
                Debug.LogWarning("Size not found");
                break;
        }
    }
    private IEnumerator ActiveInteraction()
    {
        yield return new WaitForEndOfFrame();
        InteractionManager.instance.transform.localScale = Vector3.one;
    }

    #endregion

    private IEnumerator ExitRagdoll()
    {
        yield return new WaitForSeconds(3f);
        ToggleRagdoll(false);
        feet.ResetCounter();
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
            jumpAudio.volume = AudioManager.instance.GetVolume();
            jumpAudio.pitch = Random.Range(0.92f, 1.07f);
            jumpAudio.Play();
            isJumping = true;
            animator.SetTrigger("Jumping");
            velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
            isJumping = false;
        }
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

            // Vérifie si l'objet en main est un "maïs" avant d'ouvrir la porte
            if (doorController != null && inventorySystem.itemInHand != null && inventorySystem.itemInHand.GetItemName() == "Maïs")
            {
                doorController.ToggleDoor();
            }
            if (inventorySystem.itemInHand != null && inventorySystem.itemInHand.GetItemName() == "Potion")
            {
                capacities.Add("Potion");
                Debug.Log("Capacity 'potion' added");
                inventorySystem.DestroyItemInHand();
            }
        }
    }

    public void AddCapacity(string capacityName)
    {
        capacities.Add(capacityName);
    }

    public void OnScroll(InputAction.CallbackContext context)
    {
        if (capacities.Count < 1) return;
        Vector2 scrollIndex = context.ReadValue<Vector2>();
        if (scrollIndex.y > 0)
        {
            capacityIndex++;
            if (capacityIndex > capacities.Count - 1)
            {
                capacityIndex = 0;
            }
            else if (capacityIndex < 0)
            {
                capacityIndex = capacities.Count - 1;
            }
        }
        else if (scrollIndex.y < 0)
        {
            capacityIndex--;
        }
    }

    public void OnCapacity1(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (capacities.Count < 1) return;
            Debug.Log(capacities[capacityIndex]);
            switch (capacities[capacityIndex])
            {
                case "Potion": // Get smaller
                    LittlePotion();
                    break;
                default:
                    Debug.LogWarning("[Capacity] : Trying to use capacity without select an available capacity");
                    break;
            }
        }
    }

    public void OnCapacity2(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (capacities.Count < 1) return;
            Debug.Log(capacities[capacityIndex]);
            switch (capacities[capacityIndex])
            {
                case "Potion": // Get bigger
                    BigPotion();
                    break;
                default:
                    Debug.LogWarning("[Capacity] : Trying to use capacity without select an available capacity");
                    break;
            }
        }
    }

    public void PlayStepSound()
    {
        AudioSource step = stepAudios[Random.Range(0, stepAudios.Length)];
        step.pitch = Random.Range(0.85f, 1.15f);
        step.volume = AudioManager.instance.GetVolume();
        step.Play();
    }

    #region Getter
    public size GetSize() { return size; }
    #endregion
}
