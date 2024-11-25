using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager Instance { get; set; }
    public static AudioManager instance => Instance;

    private float volume;

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
        volume = PlayerPrefs.GetFloat("volume");
    }

    public float GetVolume() // always put this line before playing audio : audio.volume = AudioManager.instance.GetVolume();
    {
        return volume;
    }
}
