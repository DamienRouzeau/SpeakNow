using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculQuest : MonoBehaviour
{

    private static CalculQuest Instance { get; set; }
    public static CalculQuest instance => Instance;

    public Animator doorToOpen;


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
        doorToOpen.SetTrigger("MiOpen");
    }
}
