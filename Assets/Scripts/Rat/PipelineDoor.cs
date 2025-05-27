using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipelineDoor : MonoBehaviour
{
    [SerializeField] int doorID;
    [SerializeField] Transform anchor;
    [SerializeField] RatBehaviour rat;
    private bool objectBlockingDoor;
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.parent != null)
        {
            Debug.Log("Parent isn't null");
            if (other.CompareTag("Highlightable") && other.gameObject.transform.parent.name != "Hand")
            {
                Debug.Log("Parent isn't Hand + is item");
                if (objectBlockingDoor) return;
                Rigidbody rb = other.GetComponent<CollectibleObject>().rb;
                rb.isKinematic = true;
                //other.gameObject.transform.parent = anchor;
                other.gameObject.transform.position = anchor.transform.position;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rat.LockDoor(doorID);
                objectBlockingDoor = true;
            }
        }
        else
        {
            Debug.Log("Don't have parent");
            if (other.CompareTag("Highlightable"))
            {
                Debug.Log("Is item");
                if (objectBlockingDoor) return;
                Rigidbody rb = other.GetComponent<CollectibleObject>().rb;
                rb.isKinematic = true;
                other.gameObject.transform.position = anchor.transform.position;
                //other.gameObject.transform.parent = anchor;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rat.LockDoor(doorID);
                objectBlockingDoor = true;
            }
        }
    }

    private IEnumerator RemoveKinematic(Rigidbody rb)
    {
        yield return new WaitForFixedUpdate();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
    }

    public void OnTriggerExit(Collider other)
    {
        if (!objectBlockingDoor) return;
        rat.UnlockDoor(doorID);
    }
}
