using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondDetector : MonoBehaviour
{

    [SerializeField]
    Animator doorToOpen;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.name == "Diamond")
        {
            doorToOpen.SetTrigger("Open");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Diamond")
        {
            doorToOpen.SetTrigger("Open");
        }
    }
}
