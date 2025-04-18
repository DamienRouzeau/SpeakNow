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
    [SerializeField]
    private string questName;
    private InventorySystem inventory = InventorySystem.instance;
    [SerializeField]
    private bool shouldLookAtPlayer = true;
    [Header("Audio")]
    [SerializeField]
    private AudioSource audio;
    [SerializeField]
    private AudioSource questFinishedAudio;
    [SerializeField]
    private float minPitch, maxPitch;

    private void Start()
    {
        interactionManager = InteractionManager.instance;
        startTransform = transform;
        canvas.SetActive(false);
    }

    private void StartDialogue()
    {
        if(CheckQuest())
        {
            ResolveQuest();
            return;
        }
        audio.pitch = Random.Range(minPitch, maxPitch);
        audio.volume = AudioManager.instance.GetVolume();
        audio.Play();
        Vector3 targetPosition = new Vector3
            (
            player.transform.position.x,
            transform.position.y,
            player.transform.position.z
            );
        animator.SetBool("talk", true);
        if(shouldLookAtPlayer) transform.LookAt(targetPosition);
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
            interactionManager.Unsub(StartDialogue, transform);
            player = null;
            canvas.SetActive(false);
            animator.SetBool("talk", false);
        }
    }

    private bool CheckQuest()
    {
        switch(questName)
        {
            case "CornQuest":
                inventory = InventorySystem.instance;
                if (inventory.itemInHand != null)
                {
                    if (inventory.itemInHand.itemName == "Corn") return true;
                }
                break;
            case "CalculQuest":
                inventory = InventorySystem.instance;
                if (inventory.itemInHand != null)
                {
                    if (inventory.itemInHand.itemName == "7") return true;
                }
                break;
            default: return false;
        }
        return false;
    }

    private void ResolveQuest()
    {
        animator.SetBool("talk", false);
        animator.SetTrigger("QuestCompleted");
        questFinishedAudio.pitch = Random.Range(minPitch, maxPitch);
        questFinishedAudio.volume = AudioManager.instance.GetVolume();
        questFinishedAudio.Play();
        switch (questName)
        {
            case "CornQuest":
                CornQuest.instance.Resolve();
                inventory.DestroyItemInHand();
                break;
            case "CalculQuest":
                CalculQuest.instance.Resolve();
                inventory.DestroyItemInHand();
                break;
        }
    }
}
