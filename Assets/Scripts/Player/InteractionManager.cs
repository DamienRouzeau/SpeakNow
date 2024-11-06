using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractionManager : MonoBehaviour
{
    private UnityEvent interact = new UnityEvent();
    private List<UnityAction> subscribers = new List<UnityAction>();
    public List<GameObject> interactibleObjects;
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

    public void Sub(UnityAction _action, GameObject _interactibleInRange)
    {
        interact.AddListener(_action);
        subscribers.Add(_action);
        interactibleObjects.Add(_interactibleInRange);
    }

    public void Unsub(UnityAction _action, GameObject _interactibleInRange)
    {
        interact.RemoveListener(_action);
        subscribers.Remove(_action);
        interactibleObjects.Remove(_interactibleInRange);
    }

    //public void Unsub(int _index)
    //{
    //    if(subscribers.Count - 1 >= _index) interact.RemoveListener(subscribers[_index]);
    //    if (subscribers.Count - 1 >= _index)  subscribers.Remove(subscribers[_index]);
    //    if (interactibleObjects.Count - 1 >= _index)  interactibleObjects.Remove(interactibleObjects[_index]);
    //}

    public void Interact()
    {
        if (interactibleObjects.Count > 1)
        {
            int idClosestObject = 0;
            float closestDistance = Vector3.Distance(interactibleObjects[0].transform.position, transform.position);

            foreach (GameObject obj in interactibleObjects)
            {
                float tryDistance = Vector3.Distance(obj.transform.position, transform.position);
                if (tryDistance < closestDistance)
                {
                    closestDistance = tryDistance;
                    idClosestObject = interactibleObjects.IndexOf(obj);
                }
            }
            subscribers[idClosestObject].Invoke();
            Unsub(subscribers[idClosestObject], interactibleObjects[idClosestObject]);
        }
        else if(InventorySystem.instance.itemInHand != null && interactibleObjects.Count <= 0)
        {
            InventorySystem.instance.RemoveItemInHand();
        }
        else
        {
            interact.Invoke();
        }
    }
}
