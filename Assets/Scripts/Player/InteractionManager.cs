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

    // Ajout du champ interactibleObjects
    public List<GameObject> interactibleObjects = new List<GameObject>(); 
    
    private Transform lastHighlightedObject = null;

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

    public void Sub(UnityAction action, Transform interactableTransform)
    {
        if (interactableTransform != null && interactableTransform.GetComponent<Collider>().enabled)
        {
            float distance = Vector3.Distance(playerTransform.position, interactableTransform.position);
            interactionQueue.Add((distance, action, interactableTransform));
            Debug.Log($"Objet ajouté avec distance {distance}. Nombre total d'abonnés : {interactionQueue.Count}");

            // Ajout de l'objet à la liste des objets interactifs
            interactibleObjects.Add(interactableTransform.gameObject);
        }
    }

    public void Unsub(UnityAction action)
    {
        interactionQueue.RemoveAll(item => item.action == action);
        Debug.Log("Objet supprimé de l'interaction. Nombre total d'abonnés : " + interactionQueue.Count);
    }

    public void Interact()
    {
        if (interactionQueue.Count > 0)
        {
            interactionQueue.Sort((a, b) => a.distance.CompareTo(b.distance));
            var closestAction = interactionQueue[0].action;
            closestAction.Invoke();
            Debug.Log("Interaction exécutée pour l'objet le plus proche.");
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
                interactionQueue.RemoveAt(i); // Supprime l'entrée si l'objet a été détruit
                continue;
            }

            float updatedDistance = Vector3.Distance(playerTransform.position, item.interactableTransform.position);
            interactionQueue[i] = (updatedDistance, item.action, item.interactableTransform);
        }

        HighlightClosestObject();
    }

    public void HighlightClosestObject()
    {
        if (interactionQueue.Count == 0) return;

        interactionQueue.Sort((a, b) => a.distance.CompareTo(b.distance));
        Transform closestObject = interactionQueue[0].interactableTransform;

        if (lastHighlightedObject != null && lastHighlightedObject != closestObject)
        {
            Outline outlineToDisable = lastHighlightedObject.GetComponent<Outline>();
            if (outlineToDisable != null)
            {
                outlineToDisable.outlineWidth = 0;
                outlineToDisable.UpdateMaterialProperties();
            }
        }

        Outline outlineToEnable = closestObject.GetComponent<Outline>();
        if (outlineToEnable != null)
        {
            outlineToEnable.outlineWidth = highlightWidth;
            outlineToEnable.UpdateMaterialProperties();
        }

        lastHighlightedObject = closestObject;
    }
}
