using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeetDetectors : MonoBehaviour
{
    private int surfaceCollisionCounter = 0;
    private bool isGrounded = false;

    public void Start()
    {
        Collider collider = GetComponent<Collider>();
        if(collider != null)
        {
            collider.enabled = true;
        }
        else
        {
            Debug.LogWarning("Feet collider not found");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        surfaceCollisionCounter++;
        isGrounded = true;
    }

    private void OnTriggerExit(Collider other)
    {
        surfaceCollisionCounter--;
        if(surfaceCollisionCounter <= 0)
        {
            surfaceCollisionCounter = 0;
            isGrounded = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        surfaceCollisionCounter++;
        isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        surfaceCollisionCounter--;
        if (surfaceCollisionCounter <= 0)
        {
            surfaceCollisionCounter = 0;
            isGrounded = false;
        }
    }

    public bool GetGrounded()
    {
        return isGrounded;
    }

    public void ResetCounter()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }
        else
        {
            Debug.LogWarning("Feet collider not found");
        }
        surfaceCollisionCounter = 0;
        isGrounded = false;
    }
}
