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
        if (interactionManager == null)
        {
            Debug.LogError($"{gameObject.name} n'a pas pu récupérer l'instance d'InteractionManager.");
        }
        else
        {
            Debug.Log($"{gameObject.name} a récupéré InteractionManager.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InteractionArea"))
        {
            Debug.Log($"{gameObject.name} a détecté InteractionArea dans OnTriggerEnter");
            interactionManager.Sub(CheckRecette, transform);
            interactionManager.HighlightClosestObject();
            player = other.gameObject.transform.parent.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("InteractionArea"))
        {
            Debug.Log($"{gameObject.name} a quitté InteractionArea dans OnTriggerExit");
            interactionManager.Unsub(CheckRecette, transform);
            player = null;
        }
    }


    public void CheckRecette()
    {
        animator.SetTrigger("Clic");
        if (recipient.potionIsCreate)
        {
            recipient.ThrowPotion();
        }
        else
        {
            recipient.CheckRecipe();
        }
    }
}