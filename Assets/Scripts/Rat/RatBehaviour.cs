using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RatBehaviour : MonoBehaviour
{
    [SerializeField] private List<GameObject> spots = new List<GameObject>();
    private int targettedSpotId;
    private NavMeshAgent agent;
    private bool isInSpot = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && isInSpot)
        {
            GoToOtherSpot();
        }
    }

    private void FixedUpdate()
    {
        if (targettedSpotId < spots.Count)
        {
            isInSpot = agent.SetDestination(spots[targettedSpotId].transform.position);  
        }
    }

    private void GoToOtherSpot()
    {
        targettedSpotId++;
        if (targettedSpotId > spots.Count - 1)
            targettedSpotId = 0;
    }
}
