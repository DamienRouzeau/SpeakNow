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
    private bool blocked = false;
    private int doorLocked = 0;
    [SerializeField] private List<AnchorSystem> anchor = new List<AnchorSystem>();
    [SerializeField] private BoxCollider wrench;
    [SerializeField] ParticleSystem particles;

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
        if (agent.velocity.magnitude < 0.1f)
        {
            particles.Stop();
        }
        else if (!particles.isPlaying)
            particles.Play();

    }

    private void GoToOtherSpot()
    {
        doorLocked = 0;
        foreach (AnchorSystem anch in anchor)
        {
            if (anch.GetOccupied())
                doorLocked++;
        }
        if (blocked && doorLocked == 0) blocked = false;
        if (blocked && wrench != null)
        {
            wrench.enabled = true;
            return;
        }
        if(wrench != null)
            wrench.enabled = false;
        isInSpot = false;
        targettedSpotId++;
        if(doorLocked > 0)
        {
            targettedSpotId = 1;
            blocked = true;
        }
        if (targettedSpotId > spots.Count - 1)
        {
            targettedSpotId = 0;
        }
        if (currentSpot == outSpot && spotsLocked[targettedSpotId])
        {
            targettedSpotId++;
        }
        if (spotsLocked[targettedSpotId])
        {
            isInSpot = false;
        }
        currentSpot = spots[targettedSpotId];
    }

    public void GoToOutSpot()
    {
        if (blocked) return;
        if (spotsLocked[targettedSpotId]) return;
        currentSpot = outSpot;
    }

    public void LockDoor(int doorID)
    {
        Debug.Log("Door locked");
        spotsLocked[doorID] = true;
        doorLocked++;
    }

    public void UnlockDoor(int doorID)
    {
        Debug.Log("Door unlocked");
        spotsLocked[doorID] = false;
        doorLocked--;
        blocked = doorLocked == 0 ? false : true;
    }
}
