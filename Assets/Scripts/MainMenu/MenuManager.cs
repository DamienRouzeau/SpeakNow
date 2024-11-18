using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class MenuManager : MonoBehaviour
{

    [SerializeField]
    private Animator animation;
    [SerializeField]
    private Animator carAnimation;

    [Header("Cameras")]
    [SerializeField]
    private CinemachineVirtualCamera mainCam;
    [SerializeField]
    private CinemachineVirtualCamera settingsCam;
    [SerializeField]
    private CinemachineVirtualCamera creditsCam;

    [Header("UI")]
    [SerializeField]
    private GameObject mainMenuInterface;
    [SerializeField]
    private GameObject settingsInterface;
    [SerializeField]
    private GameObject creditsInterface;


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

    #region MAIN_MENU
    public void OnBackToMenu()
    {
        mainCam.Priority = 1;
        settingsCam.Priority = 0;
        creditsCam.Priority = 0;
        mainMenuInterface.SetActive(true);
        creditsInterface.SetActive(false);
        settingsInterface.SetActive(false);
    }

    #endregion

    #region SETTINGS
    public void OnOpenSettings()
    {
        mainCam.Priority = 0;
        settingsCam.Priority = 1;
        creditsCam.Priority = 0;
        mainMenuInterface.SetActive(false);
        settingsInterface.SetActive(true);
        SettingsManager.instance.OnShowGeneral();
        SettingsManager.instance.generalAnim.SetTrigger("Selected");
    }
    public void OnCloseSettings()
    {
        mainCam.Priority = 1;
        settingsCam.Priority = 0;
        creditsCam.Priority = 0;
        mainMenuInterface.SetActive(true);
        settingsInterface.SetActive(false);
    }

    #endregion

    #region CREDITS
    public void OnShowCredits()
    {
        mainCam.Priority = 0;
        settingsCam.Priority = 0;
        creditsCam.Priority = 1;
        mainMenuInterface.SetActive(false);
        creditsInterface.SetActive(true);
    }

    public void OnHideCredits()
    {
        mainCam.Priority = 1;
        settingsCam.Priority = 0;
        creditsCam.Priority = 0;
        mainMenuInterface.SetActive(true);
        creditsInterface.SetActive(false);
    }

    #endregion

    public void OnQuit()
    {
        Application.Quit();
    }
}
