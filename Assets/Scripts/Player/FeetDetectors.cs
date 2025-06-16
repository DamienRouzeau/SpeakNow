using UnityEngine;

public class FeetDetectors : MonoBehaviour
{
    [SerializeField] private LayerMask groundMask;

    private int surfaceCollisionCounter = 0;
    private bool isGrounded = false;

    private void Start()
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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & groundMask) == 0) return;

        surfaceCollisionCounter++;
        isGrounded = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & groundMask) == 0) return;

        surfaceCollisionCounter = Mathf.Max(0, surfaceCollisionCounter - 1);
        isGrounded = surfaceCollisionCounter > 0;
    }

    public bool GetGrounded()
    {
        return isGrounded;
    }

    public void ResetCounter()
    {
        surfaceCollisionCounter = 0;
        isGrounded = false;

        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }
        else
        {
            Debug.LogWarning("Feet collider not found");
        }
    }
}