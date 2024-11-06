using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CornQuest : MonoBehaviour
{

    private static CornQuest Instance { get; set; }
    public static CornQuest instance => Instance;

    [SerializeField]
    private DoorController doorController;


    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void Resolve()
    {
        doorController.ToggleDoor();
    }
}
