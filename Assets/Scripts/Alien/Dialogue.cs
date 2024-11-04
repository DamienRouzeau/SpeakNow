using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    private Transform startTransform;
    private GameObject player;
    private InteractionManager interactionManager;

    private void Start()
    {
        interactionManager = InteractionManager.instance;
        startTransform = transform;
    }

    private void StartDialogue()
    {
        Debug.Log("Interact !!!!!!");
        Quaternion _rotation = Quaternion.LookRotation(player.transform.position - transform.position);
        _rotation.x = transform.rotation.x;
        _rotation.z = transform.rotation.z;
        _rotation.w = transform.rotation.w;
        transform.rotation = _rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("InteractionArea"))
        {
            Debug.Log("Enter");
            interactionManager.Sub(StartDialogue);
            player = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("InteractionArea"))
        {
            Debug.Log("Exit");
            interactionManager.Unsub(StartDialogue);
            player = null;
        }
    }
}
