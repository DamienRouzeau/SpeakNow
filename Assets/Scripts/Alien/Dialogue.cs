using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    private Transform startTransform;
    private GameObject player;
    private InteractionManager interactionManager;
    [SerializeField]
    private GameObject canvas;

    private void Start()
    {
        interactionManager = InteractionManager.instance;
        startTransform = transform;
        canvas.SetActive(false);
    }

    private void StartDialogue()
    {
        Vector3 targetPosition = new Vector3
            (
            player.transform.position.x,
            transform.position.y,
            player.transform.position.z
            );
        
        transform.LookAt(targetPosition);
        canvas.SetActive(true);

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("InteractionArea"))
        {
            interactionManager.Sub(StartDialogue);
            Debug.Log(other.gameObject.name);
            player = other.gameObject.transform.parent.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("InteractionArea"))
        {
            interactionManager.Unsub(StartDialogue);
            player = null;
            canvas.SetActive(false);
        }
    }
}
