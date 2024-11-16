using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    [SerializeField]
    private Animator animation;
    [SerializeField]
    private Animator carAnimation;

    public void OnStartNewGame()
    {
        carAnimation.SetTrigger("start");
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(1);
    }

    public void OnContinue()
    {
        carAnimation.SetTrigger("start");
        StartCoroutine(StartGame());
    }

    public void OnOpenSettings()
    {

    }

    public void OnShowCredits()
    {

    }

    public void OnQuit()
    {
        Application.Quit();
    }
}
