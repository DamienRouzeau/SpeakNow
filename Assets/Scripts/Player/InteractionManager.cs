using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractionManager : MonoBehaviour
{
    private UnityEvent interact = new UnityEvent();
    private List<UnityAction> subscribers = new List<UnityAction>();
    private static InteractionManager Instance { get; set; }
    public static InteractionManager instance => Instance;

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void Sub(UnityAction _action)
    {
        interact.AddListener(_action);
        subscribers.Add(_action);
    }

    public void Unsub(UnityAction _action)
    {
        interact.RemoveListener(_action);
        subscribers.Remove(_action);
    }

    public void Interact()
    {
        interact.Invoke();
    }
}
