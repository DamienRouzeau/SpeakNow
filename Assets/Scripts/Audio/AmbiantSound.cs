using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbiantSound : MonoBehaviour
{

    [SerializeField]
    private AudioSource audio;
    [SerializeField]
    private AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        audio.volume = audioManager.GetVolume();
        audio.Play();
    }

}
