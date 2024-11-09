using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private Transform startTransform;
    private GameObject player;
    private InteractionManager interactionManager;
    [SerializeField]
    private GameObject canvas;
    [SerializeField]
    private float dialogueZoom;
    [SerializeField]
    private float dialogueDuration;
    [SerializeField]
    private Animator animator;

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
        animator.SetBool("talk", true);
        transform.LookAt(targetPosition);
        canvas.SetActive(true);
        // activer le bruit de talk ici
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("InteractionArea"))
        {
            interactionManager.Sub(StartDialogue, transform);
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
            animator.SetBool("talk", false);
        }
    }
}
