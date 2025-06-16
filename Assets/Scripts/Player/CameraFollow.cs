using UnityEngine;

public class CameraFreeLook : MonoBehaviour
{
    /* ---------- Réglages généraux ---------- */
    [Header("Cible et sensibilité")]
    public Transform player;
    public float mouseSensitivity = 100f;

    [Header("Base Camera Settings")]
    public float baseDistance      = 5f;
    public float baseMinDistance   = 2f;
    public float baseCameraHeight  = 3f;
    public float baseHeadHeight    = 1.5f;
    [Tooltip("Layers pris en compte pour bloquer la caméra")]
    public LayerMask collisionLayers;

    [Header("FOV Dynamique")]
    [SerializeField] private Camera mainCamera;
    public float baseFOV = 60f;
    public float maxFOV  = 80f;
    public float minFOV  = 45f;

    [HideInInspector] public bool cameraFrozen = false;

    /* ---------- Limites d’angle ---------- */
    public float minVerticalAngle = -15f;
    public float maxVerticalAngle =  55f;

    /* ---------- Layer à ignorer totalement ---------- */
    [Header("Layer spécial à ignorer")]
    [Tooltip("Nom du layer à ignorer même s’il est dans collisionLayers")]
    public string ignoreLayerName = "Notrigger";

    /* ---------- Privé ---------- */
    float rotationX, rotationY;
    int   ignoreLayer;          // index numérique du layer à ignorer

    /* ---------- Initialisation ---------- */
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        float savedSens = PlayerPrefs.GetFloat("sensitivity");
        mouseSensitivity = (savedSens > 0.25f) ? savedSens * 100f : 50f;

        ignoreLayer = LayerMask.NameToLayer(ignoreLayerName);
        if (ignoreLayer == -1)
            Debug.LogWarning($"[CameraFreeLook] Layer « {ignoreLayerName} » introuvable !");
    }

    /* ---------- Caméra ---------- */
    void LateUpdate()
    {
        if (!player) return;

        /* -- rotation selon la souris -- */
        if (!cameraFrozen)
        {
            rotationX += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            rotationY = Mathf.Clamp(rotationY, minVerticalAngle, maxVerticalAngle);
        }

        /* -- paramètres dynamiques selon l’échelle du joueur -- */
        Vector3 scale       = player.localScale;
        float   scaleFactor = scale.y / 0.15f;

        float dynDist   = baseDistance    * scaleFactor;
        float dynHeight = baseCameraHeight * Mathf.Pow(scaleFactor, 1.15f);
        float dynMin    = baseMinDistance * scaleFactor;
        float dynHead   = baseHeadHeight  * scaleFactor;

        Quaternion rot   = Quaternion.Euler(rotationY, rotationX, 0f);
        Vector3    offset = rot * new Vector3(0f, dynHeight, -dynDist);
        Vector3    target = player.position + offset;

        Vector3 rayOrigin    = player.position + Vector3.up * dynHead;
        Vector3 rayDirection = target - rayOrigin;

        int finalMask = collisionLayers;
        if (ignoreLayer != -1)
            finalMask &= ~(1 << ignoreLayer);

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, dynDist,
                            finalMask, QueryTriggerInteraction.Ignore))
        {
            float adjDist = Mathf.Max(dynMin, hit.distance - 0.2f);
            target = player.position + (hit.point - player.position).normalized * adjDist;

            Debug.Log($"[CameraFreeLook] Collision caméra : {hit.collider.name} | layer={LayerMask.LayerToName(hit.collider.gameObject.layer)} | dist={hit.distance:F2}");
        }

        /* -- FOV dynamique -- */
        if (mainCamera)
        {
            float t = Mathf.InverseLerp(0.07f, 0.3f, scale.y);
            mainCamera.fieldOfView = Mathf.Lerp(minFOV, maxFOV, t);
        }

        /* -- Appliquer la position et regarder la tête du joueur -- */
        transform.position = target;
        transform.LookAt(player.position + Vector3.up * dynHead);
    }
}
