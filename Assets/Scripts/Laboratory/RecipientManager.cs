using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField]
    private List<Sprite> numberList = new List<Sprite>();
    [SerializeField]
    private Image ingredientQtt;
    [SerializeField]
    private Image ingredientMax;
    [SerializeField]
    private TextMeshProUGUI textScreen;
    [SerializeField]
    private Animator screenAnimation;
    [SerializeField]
    private AudioSource audio;
    private int nbIngredient = 0;
    private List<CollectibleObject> ingredientsList = new List<CollectibleObject>();
    public bool potionIsCreate = false;




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

    private void UpdateScreen()
    {
        ingredientQtt.sprite = numberList[ingredientsList.Count];
    }


    public void CheckRecipe()
    {
        if (ingredientsList.Count == 6 && carrotNB == 0 && gemNB == 0)
        {
            potionIsCreate = true;
            ThrowPotion();
        }
        else
        {
            for (int i = ingredientsList.Count - 1; i >= 0; i--)
            {
                ThrowBackIngredient(ingredientsList[i]);
                ingredientsList.Remove(ingredientsList[i]);
                UpdateScreen();
            }
        }
    }

    private void TooMuchIngredient()
    {
        textScreen.color = Color.red;
        ingredientQtt.color = Color.red;
        ingredientMax.color = Color.red;
        screenAnimation.SetTrigger("Full");
        StartCoroutine(ResetScreen());
    }

    private IEnumerator ResetScreen()
    {
        yield return new WaitForSeconds(1);
        textScreen.color = Color.black;
        ingredientQtt.color = Color.black;
        ingredientMax.color = Color.black;
    }

    public void AddIngredient()
    {
        if (ingredientsList.Count >= 6)
        {
            TooMuchIngredient();
        }
        else
        {
            InventorySystem inventory = player.GetComponent<InventorySystem>();
            CollectibleObject ingredient = inventory.itemInHand;
            inventory.RemoveItemInHand();
            if (ingredient == null) return;
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
            ingredientsList.Add(ingredient);
            ingredient.gameObject.SetActive(false);
            UpdateScreen();
        }
    }

    public void ThrowBackIngredient(CollectibleObject ingredient)
    {
        audio.volume = AudioManager.instance.GetVolume();
        audio.Play();
        ingredient.gameObject.SetActive(true);
        ingredient.transform.position = transform.position - (Vector3.right * 1f);
        ingredient.rb.AddForce(new Vector3(0, throwStrenght, 0) - new Vector3(1, 0, 0) * throwStrenght, ForceMode.Impulse);
    }

    public void ThrowPotion()
    {
        audio.volume = AudioManager.instance.GetVolume();
        audio.Play();
        var _potion = Instantiate(potion);
        _potion.transform.position = transform.position - (Vector3.right * 1f);
        Rigidbody _rb = _potion.GetComponent<Rigidbody>();
        _rb.AddForce(new Vector3(0, throwStrenght, 0) - new Vector3(1, 0, 0) * throwStrenght, ForceMode.Impulse);
    }
}
