using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondDetector : MonoBehaviour
{
    [SerializeField]
    private AudioSource audio;
    [SerializeField]
    private Animator doorToOpen;
    private bool doorIsOpen = false;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.name == "Diamond")
        {
            doorToOpen.SetTrigger("Open");
            audio.volume = AudioManager.instance.GetVolume();
            audio.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Diamond" && !doorIsOpen)
        {
            doorToOpen.SetTrigger("Open");
            doorIsOpen = true;
            audio.volume = AudioManager.instance.GetVolume();
            audio.Play();
        }
    }
}
