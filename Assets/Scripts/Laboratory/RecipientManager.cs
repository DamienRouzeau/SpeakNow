using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipientManager : MonoBehaviour
{
    private GameObject player;
    private InteractionManager interactionManager;
    public int carrotNB = 2;
    public int gemNB = 4;
    [SerializeField]
    private float throwStrenght = 2;
    private bool isGoodIngredient;
    [SerializeField]
    private GameObject potion;


    private void Start()
    {
        interactionManager = InteractionManager.instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InteractionArea"))
        {
            Debug.Log($"{gameObject.name} a détecté InteractionArea dans OnTriggerEnter");
            interactionManager.Sub(AddIngredient, transform);
            interactionManager.HighlightClosestObject();
            player = other.gameObject.transform.parent.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("InteractionArea"))
        {
            Debug.Log($"{gameObject.name} a quitté InteractionArea dans OnTriggerExit");
            interactionManager.Unsub(AddIngredient, transform);
            player = null;
        }
    }


    public void AddIngredient()
    {
        InventorySystem inventory = player.GetComponent<InventorySystem>();
        CollectibleObject ingredient = inventory.itemInHand;
        if (ingredient == null) return;
        // inventory.RemoveItemInHand();
        switch (ingredient.itemName)
        {
            case "Carrot":
                carrotNB -= 1;
                if (carrotNB < 0)
                {
                    isGoodIngredient = false;
                    carrotNB = 0;
                }
                else isGoodIngredient = true;
                break;

            case "Gem":
                gemNB -= 1;
                if (gemNB < 0)
                {
                    isGoodIngredient = false;
                    gemNB = 0;
                }
                else isGoodIngredient = true;
                break;

            default:
                isGoodIngredient = false;
                break;
        }

        if (isGoodIngredient)
        {
            Destroy(ingredient.gameObject);
        }
        else
        {
            StartCoroutine(ThrowBackIngredient(ingredient));
            ingredient.gameObject.SetActive(false);
        }
    }

    public IEnumerator ThrowBackIngredient(CollectibleObject ingredient)
    {
        yield return new WaitForSeconds(1);
        ingredient.gameObject.SetActive(true);
        ingredient.transform.position = transform.position - (Vector3.right * 1f);
        ingredient.rb.AddForce(new Vector3(0, throwStrenght, 0) - new Vector3(1, 0, 0) * throwStrenght, ForceMode.Impulse);
    }

    public void ThrowPotion()
    {
        var _potion = Instantiate(potion);
        _potion.transform.position = transform.position - (Vector3.right * 1f);
        Rigidbody _rb = _potion.GetComponent<Rigidbody>();
        _rb.AddForce(new Vector3(0, throwStrenght, 0) - new Vector3(1, 0, 0) * throwStrenght, ForceMode.Impulse);
    }

}
