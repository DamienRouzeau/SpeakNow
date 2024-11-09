using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractionManager : MonoBehaviour
{
    private List<(float distance, UnityAction action, Transform interactableTransform)> interactionQueue = new List<(float, UnityAction, Transform)>();
    private static InteractionManager Instance { get; set; }
    public static InteractionManager instance => Instance;
    public Transform playerTransform; // Assurez-vous de l'assigner au joueur dans l'inspecteur.

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
        if (interactableTransform.GetComponent<Collider>().enabled) // Vérifie si l'interaction est possible
        {
            float distance = Vector3.Distance(playerTransform.position, interactableTransform.position);
            interactionQueue.Add((distance, action, interactableTransform));
            Debug.Log($"Objet ajouté avec distance {distance}. Nombre total d'abonnés : {interactionQueue.Count}");
        }
    }


    // Méthode pour se désabonner
    public void Unsub(UnityAction action)
    {
        interactionQueue.RemoveAll(item => item.action == action);
        Debug.Log("Objet supprimé de l'interaction. Nombre total d'abonnés : " + interactionQueue.Count);
    }

    // Méthode pour invoquer l'interaction la plus proche
    public void Interact()
    {
        if (interactionQueue.Count > 0)
        {
            // Trier par distance pour que l'objet le plus proche soit en premier
            interactionQueue.Sort((a, b) => a.distance.CompareTo(b.distance));

            var closestAction = interactionQueue[0].action;
            closestAction.Invoke();
            Debug.Log("Interaction exécutée pour l'objet le plus proche.");
        }
    }

    // Mise à jour des distances pour chaque objet dans la file d'attente
    private void Update()
    {
        if (playerTransform == null) return;

        for (int i = 0; i < interactionQueue.Count; i++)
        {
            var item = interactionQueue[i];
            float updatedDistance = Vector3.Distance(playerTransform.position, item.interactableTransform.position);
            interactionQueue[i] = (updatedDistance, item.action, item.interactableTransform);
        }
    }
}
