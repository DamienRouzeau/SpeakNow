using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractionManager : MonoBehaviour
{
    private SortedList<int, UnityAction> interactionQueue = new SortedList<int, UnityAction>(); // Trie les interactions par priorité
    private static InteractionManager Instance { get; set; }
    public static InteractionManager instance => Instance;

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

    // Méthode pour s'abonner avec une priorité
    public void Sub(UnityAction action, int priority)
    {
        // Assurez-vous que chaque clé (priorité) est unique
        while (interactionQueue.ContainsKey(priority)) priority++;
        
        interactionQueue.Add(priority, action);
        Debug.Log($"Objet ajouté avec priorité {priority}. Nombre total d'abonnés : {interactionQueue.Count}");
    }

    // Méthode pour se désabonner
    public void Unsub(UnityAction action)
    {
        foreach (var key in interactionQueue.Keys)
        {
            if (interactionQueue[key] == action)
            {
                interactionQueue.Remove(key);
                break;
            }
        }
        Debug.Log("Objet supprimé de l'interaction. Nombre total d'abonnés : " + interactionQueue.Count);
    }

    // Méthode pour invoquer l'interaction de priorité la plus élevée
    public void Interact()
    {
        if (interactionQueue.Count > 0)
        {
            var highestPriorityAction = interactionQueue.Values[0];
            highestPriorityAction.Invoke();
            Debug.Log("Interaction exécutée pour l'objet avec la plus haute priorité.");
        }
    }
}