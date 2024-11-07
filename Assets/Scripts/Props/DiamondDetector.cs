using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondDetector : MonoBehaviour
{

    [SerializeField]
    Animator doorToOpen;
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("AAA");
        if (collision.collider.gameObject.name == "Diamond")
        {
            doorToOpen.SetTrigger("Open");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("BBB");
        if (other.gameObject.name == "Diamond")
        {
            doorToOpen.SetTrigger("Open");
        }
    }
}
