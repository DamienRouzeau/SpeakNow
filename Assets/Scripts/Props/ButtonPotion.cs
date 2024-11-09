using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPotion : MonoBehaviour
{
    private GameObject player;
    private InteractionManager interactionManager;
    [SerializeField]
    private RecipientManager recipient;
    [SerializeField]
    private Animator animator;

    private void Start()
    {
        interactionManager = InteractionManager.instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InteractionArea"))
        {
            interactionManager.Sub(CheckRecette, transform);
            interactionManager.HighlightClosestObject();
            player = other.gameObject.transform.parent.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("InteractionArea"))
        {
            interactionManager.Unsub(CheckRecette);
            player = null;
        }
    }

    public void CheckRecette()
    {
        animator.SetTrigger("Clic");
        if (recipient.carrotNB <= 0 && recipient.gemNB <= 0)
        {
            recipient.ThrowPotion();
        }
    }
}