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
}
