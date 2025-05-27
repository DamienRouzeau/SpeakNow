using UnityEngine;

public class CameraFreeLook : MonoBehaviour
{
    [Header("Cible et sensibilité")]
    public Transform player;
    public float mouseSensitivity = 100f;

    [Header("Base Camera Settings")]
    public float baseDistance = 5f;
    public float baseMinDistance = 2f;
    public float baseCameraHeight = 3f;
    public float baseHeadHeight = 1.5f;
    public LayerMask collisionLayers;

    [Header("FOV Dynamique")]
    [SerializeField] private Camera mainCamera;
    public float baseFOV = 60f;
    public float maxFOV = 80f;
    public float minFOV = 45f;

    [HideInInspector] public bool cameraFrozen = false;

    private float rotationX = 0f;
    private float rotationY = 0f;

    public float minVerticalAngle = -15f;
    public float maxVerticalAngle = 55f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        float savedSensitivity = PlayerPrefs.GetFloat("sensitivity");
        if (savedSensitivity > 0.25f)
        {
            mouseSensitivity = savedSensitivity * 100f;
        }
        else
        {
            PlayerPrefs.SetFloat("sensitivity", 0.5f);
            mouseSensitivity = 50f;
        }
    }
    void LateUpdate()
    {
        if (!player) return;

        if (!cameraFrozen)
        {
            rotationX += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            rotationY = Mathf.Clamp(rotationY, minVerticalAngle, maxVerticalAngle);
        }

        Vector3 scale = player.localScale;
        float scaleFactor = scale.y / 0.15f;

        float dynamicDistance = baseDistance * scaleFactor;
        float dynamicHeight = baseCameraHeight * Mathf.Pow(scaleFactor, 1.15f);
        float dynamicMinDistance = baseMinDistance * scaleFactor;
        float dynamicHeadHeight = baseHeadHeight * scaleFactor;

        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);
        Vector3 offset = rotation * new Vector3(0, dynamicHeight, -dynamicDistance);

        Vector3 targetPosition = player.position + offset;

        RaycastHit hit;
        Vector3 rayOrigin = player.position + Vector3.up * dynamicHeadHeight;
        Vector3 rayDirection = targetPosition - rayOrigin;

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, dynamicDistance, collisionLayers))
        {
            float adjustedDistance = Mathf.Max(dynamicMinDistance, hit.distance - 0.2f);
            targetPosition = player.position + (hit.point - player.position).normalized * adjustedDistance;
        }

        if (mainCamera != null)
        {
            float t = Mathf.InverseLerp(0.07f, 0.3f, scale.y);
            float newFOV = Mathf.Lerp(minFOV, maxFOV, t);
            mainCamera.fieldOfView = newFOV;
        }

        transform.position = targetPosition;

        Vector3 headPosition = player.position + Vector3.up * dynamicHeadHeight;
        transform.LookAt(headPosition);
    }

}
