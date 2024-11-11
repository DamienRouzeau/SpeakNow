using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractionManager : MonoBehaviour
{
    private List<(float distance, UnityAction action, Transform interactableTransform)> interactionQueue = new List<(float, UnityAction, Transform)>();
    private static InteractionManager Instance { get; set; }
    public static InteractionManager instance => Instance;
    public Transform playerTransform;

    [SerializeField]
    private float highlightWidth = 5;

    public List<GameObject> interactibleObjects = new List<GameObject>();
    private Transform lastHighlightedObject = null;

    [SerializeField]
    private Transform handTransform; // Transform de la main du personnage

    private InventorySystem inventorySystem; // Référence à InventorySystem

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Assurez-vous que l'InventorySystem est attaché au joueur
        if (playerTransform != null)
        {
            inventorySystem = playerTransform.GetComponent<InventorySystem>();
            if (inventorySystem == null)
            {
                Debug.LogWarning("InventorySystem n'est pas assigné dans InteractionManager.");
            }
        }
    }

    public void Sub(UnityAction action, Transform interactableTransform)
    {
        if (interactableTransform != null && interactableTransform.GetComponent<Collider>().enabled)
        {
            float distance = Vector3.Distance(playerTransform.position, interactableTransform.position);
            interactionQueue.Add((distance, action, interactableTransform));
            interactibleObjects.Add(interactableTransform.gameObject);
            Debug.Log($"Objet ajouté à l'interaction: {interactableTransform.name}");
        }
        else
        {
            Debug.LogWarning($"Échec d'abonnement : Collider manquant ou désactivé sur {interactableTransform?.name}");
        }
    }

    public void Unsub(UnityAction action, Transform interactableTransform)
    {
        interactionQueue.RemoveAll(item => item.action == action);
        interactibleObjects.Remove(interactableTransform.gameObject);
        Debug.Log($"Objet désabonné de l'interaction : {interactableTransform.name}");

        if (lastHighlightedObject == interactableTransform)
        {
            Outline outlineToDisable = interactableTransform.GetComponent<Outline>();
            if (outlineToDisable != null)
            {
                outlineToDisable.outlineWidth = 0;
                outlineToDisable.UpdateMaterialProperties();
            }
            lastHighlightedObject = null;
        }
    }

    public void Interact()
    {
        if (interactionQueue.Count > 0)
        {
            interactionQueue.Sort((a, b) => a.distance.CompareTo(b.distance));
            var closestAction = interactionQueue[0].action;
            closestAction.Invoke();
            Debug.Log("Interagissant avec l'objet le plus proche.");
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        for (int i = interactionQueue.Count - 1; i >= 0; i--)
        {
            var item = interactionQueue[i];

            if (item.interactableTransform == null)
            {
                interactionQueue.RemoveAt(i);
                continue;
            }

            float updatedDistance = Vector3.Distance(playerTransform.position, item.interactableTransform.position);
            interactionQueue[i] = (updatedDistance, item.action, item.interactableTransform);
        }

        HighlightClosestObject();
    }

    public void HighlightClosestObject()
{
    if (interactionQueue.Count == 0)
    {
        if (lastHighlightedObject != null)
        {
            Outline outlineToDisable = lastHighlightedObject.GetComponent<Outline>();
            if (outlineToDisable != null)
            {
                outlineToDisable.outlineWidth = 0;
                outlineToDisable.UpdateMaterialProperties();
            }
            lastHighlightedObject = null;
        }
        return;
    }

    interactionQueue.Sort((a, b) => a.distance.CompareTo(b.distance));
    Transform closestObject = interactionQueue[0].interactableTransform;

    // Vérifie si l'objet est celui actuellement en main et l'ignore pour le surlignage
    if (inventorySystem != null && inventorySystem.itemInHand != null)
    {
        Debug.Log($"Objet actuellement en main: {inventorySystem.itemInHand.name}");

        if (closestObject == inventorySystem.itemInHand.transform)
        {
            Debug.Log($"Objet ignoré pour le highlight car il est tenu dans la main: {closestObject.name}");

            // Si l'objet est dans la main, on retire également tout highlight en cours
            if (lastHighlightedObject != null)
            {
                Outline outlineToDisable = lastHighlightedObject.GetComponent<Outline>();
                if (outlineToDisable != null)
                {
                    outlineToDisable.outlineWidth = 0;
                    outlineToDisable.UpdateMaterialProperties();
                }
                lastHighlightedObject = null;
            }

            return; // Ignore l'objet en main pour le highlight
        }
    }

    // Si un autre objet est déjà surligné, désactive son surlignage
    if (lastHighlightedObject != null && lastHighlightedObject != closestObject)
    {
        Outline outlineToDisable = lastHighlightedObject.GetComponent<Outline>();
        if (outlineToDisable != null)
        {
            outlineToDisable.outlineWidth = 0;
            outlineToDisable.UpdateMaterialProperties();
        }
    }

    // Active le surlignage pour l'objet le plus proche s'il n'est pas dans la main
    Outline outlineToEnable = closestObject.GetComponent<Outline>();
    if (outlineToEnable != null)
    {
        outlineToEnable.outlineWidth = highlightWidth;
        outlineToEnable.UpdateMaterialProperties();
        Debug.Log($"Highlight activé sur : {closestObject.name}");
    }

    lastHighlightedObject = closestObject;
}
}
