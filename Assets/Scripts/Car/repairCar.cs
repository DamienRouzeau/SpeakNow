using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class repairCar : MonoBehaviour
{
    private bool isAroundCar;
    private GameObject player;
    [SerializeField] Animator playerAnim;
    private InteractionManager interactionManager;

    public ParticleSystem smoke;
    public ParticleSystem spark;
    [SerializeField] private Animator anim;
    public Light[] headlights;

    private bool smokeFixed = false;
    private bool lightsFixed = false;
    private bool extractDone = false;


    private void Start()
    {
        interactionManager = InteractionManager.instance;

        if (interactionManager == null)
        {
            Debug.LogError("InteractionManager.instance is null. Please ensure it is initialized properly.");
        }
    }

    #region Collision
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InteractionArea"))
        {
            player = other.transform.parent.gameObject;

            if (player == null)
            {
                Debug.LogError("Player not found in the parent of the 'interactCar' object.");
                return;
            }

            interactionManager.Sub(RepairCar, transform);
            isAroundCar = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("InteractionArea"))
        {
            interactionManager.Unsub(RepairCar, transform);
            isAroundCar = false;
            player = null;
        }
    }
    #endregion

    private void RepairCar()
    {
        if (!isAroundCar || !Input.GetKeyDown(KeyCode.E)) return;

        InventorySystem inventory = player.GetComponent<InventorySystem>();
        if (inventory == null) return;

        CollectibleObject itemInHand = inventory.itemInHand;

        if (itemInHand == null)
        {
            return;
        }

        switch (itemInHand.itemName.ToLower())
        {
            case "wrench":
                if (!smokeFixed)
                {
                    smokeFixed = true;
                    inventory.RemoveItemInHand();
                    smoke.Stop();
                    Destroy(itemInHand.gameObject);
                }
                break;

            case "headlight":
                if (!lightsFixed)
                {
                    lightsFixed = true;
                    spark.Stop();
                    inventory.RemoveItemInHand();
                    foreach (var headlight in headlights)
                    {
                        if (headlight == null) continue;

                        if (headlight.TryGetComponent<Animator>(out var animator))
                        {
                            animator.enabled = false;
                        }

                        headlight.intensity = 0f;
                        headlight.gameObject.SetActive(true);
                        StartCoroutine(SmoothLightIntensity(headlight, 10f, 1f));
                    }
                    Destroy(itemInHand.gameObject);
                }
                break;

            case "shovel":
                if (!extractDone)
                {
                    extractDone = true;
                    inventory.RemoveItemInHand();
                    anim.SetTrigger("extract");
                    Destroy(itemInHand.gameObject);
                }
                break;

            default:
                itemInHand.gameObject.SetActive(false);
                break;
        }
        CheckAllRepair();

        if (smokeFixed && lightsFixed)
        {
            Debug.Log("Car fully repaired!");
        }
    }

    private void CheckAllRepair()
    {
        if(extractDone && lightsFixed && smokeFixed)
        {
            anim.SetTrigger("Final");
            playerAnim.SetBool("Final", true);
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    private IEnumerator SmoothLightIntensity(Light light, float targetIntensity, float duration)
    {
        float startIntensity = light.intensity;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            light.intensity = Mathf.Lerp(startIntensity, targetIntensity, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        light.intensity = targetIntensity;
    }
}
