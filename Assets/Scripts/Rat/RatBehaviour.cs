using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RatBehaviour : MonoBehaviour
{
    [SerializeField] private List<GameObject> spots = new List<GameObject>();
    [SerializeField] private List<bool> spotsLocked = new List<bool>();
    [SerializeField] private GameObject outSpot;
    private int targettedSpotId;
    private NavMeshAgent agent;
    private bool isInSpot = false;
    private GameObject currentSpot;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isInSpot)
        {
            GoToOtherSpot();
        }
    }

    private void FixedUpdate()
    {
        if (currentSpot != null)
            isInSpot = agent.SetDestination(currentSpot.transform.position);
        else currentSpot = spots[targettedSpotId];
    }

    private void GoToOtherSpot()
    {
        targettedSpotId++;
        if (targettedSpotId > spots.Count - 1)
            targettedSpotId = 0;
        if (spotsLocked[targettedSpotId]) return;
        currentSpot = spots[targettedSpotId];
    }

    public void GoToOutSpot()
    {
        if (spotsLocked[targettedSpotId]) return;
        currentSpot = outSpot;
    }

    public void LockDoor(int doorID)
    {
        Debug.Log("Door locked");
        spotsLocked[doorID] = true;
    }

    public void UnlockDoor(int doorID)
    {
        Debug.Log("Door unlocked");
        spotsLocked[doorID] = false;
    }
}
