using UnityEngine;

public class CameraFreeLook : MonoBehaviour
{
    public Transform player; // Le joueur à suivre
    public float mouseSensitivity = 100f; // Sensibilité de la souris
    public float distanceFromPlayer = 5f; // Distance de la caméra par rapport au joueur
    public float minDistanceFromPlayer = 2f; // Distance minimale de la caméra par rapport au joueur
    public float cameraHeight = 3f; // Hauteur de la caméra par rapport au joueur (tu peux l'augmenter)
    public LayerMask collisionLayers; // Les couches avec lesquelles la caméra peut entrer en collision (maison, props, etc.)

    private float rotationX = 0f;
    private float rotationY = 0f;
    public float minVerticalAngle = -15f;
    public float maxVerticalAngle = 55f; // Ajuste cette valeur selon tes besoins

    void Start()
    {
        // Verrouiller le curseur pour qu'il ne sorte pas de l'écran
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        // Récupérer les mouvements de la souris
        rotationX += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        rotationY = Mathf.Clamp(rotationY, minVerticalAngle, maxVerticalAngle); // Limiter la rotation verticale

        // Calculer la position et la rotation de la caméra
        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);
        Vector3 offset = rotation * new Vector3(0, cameraHeight, -distanceFromPlayer);

        // Calculer la position souhaitée de la caméra sans collision
        Vector3 targetPosition = player.position + offset;

        // Lancer un raycast entre le joueur et la caméra pour détecter les collisions
        RaycastHit hit;
        Vector3 rayOrigin = player.position + Vector3.up * 0.5f; // Décalage vertical pour éviter le sol
        Vector3 rayDirection = targetPosition - rayOrigin;

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, distanceFromPlayer, collisionLayers))
        {
            // Ajuster la distance de la caméra pour éviter de pénétrer dans l'objet, mais ne pas descendre en dessous de minDistanceFromPlayer
            float adjustedDistance = Mathf.Max(minDistanceFromPlayer, hit.distance - 0.2f);
            targetPosition = player.position + (hit.point - player.position).normalized * adjustedDistance;
        }

        // Appliquer la nouvelle position de la caméra
        transform.position = targetPosition;
        transform.LookAt(player); // Toujours regarder le joueur
    }
}