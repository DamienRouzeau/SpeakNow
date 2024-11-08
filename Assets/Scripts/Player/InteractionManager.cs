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
    [SerializeField]
    private float highlightWidht = 5;

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
        if (_interactibleInRange.CompareTag("Highlightable"))
        {
            Outline _outlineToDisable = _interactibleInRange.GetComponent<Outline>();
            _outlineToDisable.outlineWidth = 0;
            _outlineToDisable.UpdateMaterialProperties();
        }
        interact.RemoveListener(_action);
        subscribers.Remove(_action);
        interactibleObjects.Remove(_interactibleInRange);
    }

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
        else if(interactibleObjects.Count == 1)
        {
            subscribers[0].Invoke();
        }
        else if (InventorySystem.instance.itemInHand != null && interactibleObjects.Count <= 0)
        {
            InventorySystem.instance.RemoveItemInHand();
        }
        else
        {
            interact.Invoke();
        }
    }


    public void HighlightClosest()
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
                if (interactibleObjects[idClosestObject].CompareTag("Highlightable"))
                {
                    Outline _outlineToActive = interactibleObjects[idClosestObject].GetComponent<Outline>();
                    _outlineToActive.outlineWidth = 0;
                    _outlineToActive.UpdateMaterialProperties();
                }
            }
        }
        if (interactibleObjects[idClosestObject].CompareTag("Highlightable"))
        {
            Outline _outlineToActive = interactibleObjects[idClosestObject].GetComponent<Outline>();
            _outlineToActive.outlineWidth = highlightWidht;
            _outlineToActive.UpdateMaterialProperties();
        }

    }
}
