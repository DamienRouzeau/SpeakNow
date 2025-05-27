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
        currentSpot = spots[targettedSpotId];
    }

    public void GoToOutSpot()
    {
        currentSpot = outSpot;
    }

    public void LockDoor(int doorID)
    {
        spotsLocked[doorID] = true;
    }

    public void UnlockDoor(int doorID)
    {
        spotsLocked[doorID] = false;
    }
}
