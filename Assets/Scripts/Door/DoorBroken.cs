using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBroken : MonoBehaviour
{
    [SerializeField]
    private List<AudioSource> audios = new List<AudioSource>();
    [SerializeField]
    private float minDelay, maxDelay;
    private bool waitBeforePlay = false;
    public bool isSemiOpen = false;

    // Update is called once per frame
    void Update()
    {
        if(!waitBeforePlay && isSemiOpen)
        {
            waitBeforePlay = true;
            StartCoroutine(WaitDelay());
        }
    }

    private IEnumerator WaitDelay()
    {
        float delay = Random.Range(minDelay, maxDelay);
        yield return new WaitForSeconds(delay);
        AudioSource _audio = audios[Random.Range(0, audios.Count)];
        _audio.volume = AudioManager.instance.GetVolume();
        _audio.Play();
        waitBeforePlay = false;
    }
}
