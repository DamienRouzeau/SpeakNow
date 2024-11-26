using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class repairCar : MonoBehaviour
{
    private bool isAroundCar;
    [SerializeField] private bool isCarBroken = true;
    private GameObject player;
    private InteractionManager interactionManager;
    public ParticleSystem smoke;
    public ParticleSystem spark;
    public Light[] headlights;
    
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
            player = other.gameObject.transform.parent.gameObject;

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
            Debug.Log("Player exited the car interaction zone.");
            player = null;
        }
    }
    #endregion

    private void RepairCar()
    {
        if (isAroundCar && Input.GetKeyDown(KeyCode.E))
        {
            if (isCarBroken)
            {
                InventorySystem inventory = player.GetComponent<InventorySystem>();

                if (inventory == null)
                {
                    return;
                }

                CollectibleObject itemInHand = inventory.itemInHand;

                if (itemInHand != null && itemInHand.itemName == "wrench")
                {
                    inventory.RemoveItemInHand();
                    isCarBroken = false;
                    smoke.Stop();
                    spark.Stop();
                    foreach (var headlight in headlights)
                    {
                        if (headlight == null) continue; // Vérifie si le phare est nul avant d'y accéder

                        // Réinitialise l'intensité à 0
                        headlight.intensity = 0f;

                        // Vérifie si un Animator est attaché, puis arrête l'animation
                        if (headlight.TryGetComponent<Animator>(out var animator))
                        {
                            animator.StopPlayback(); // Utilise une méthode appropriée pour arrêter l'Animator
                        }
                        headlight.gameObject.SetActive(true);
                        // Lance la coroutine pour ajuster l'intensité de la lumière
                        StartCoroutine(SmoothLightIntensity(headlight, 10f, 1f));
                    }

                    Destroy(itemInHand.gameObject);
                }
                else if (itemInHand != null)
                {
                    itemInHand.gameObject.SetActive(false);
                }
                else
                {
                    Debug.Log("No item in hand to repair the car.");
                }
            }
            else
            {
                Debug.Log("Car is already repaired.");
            }
        }
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
