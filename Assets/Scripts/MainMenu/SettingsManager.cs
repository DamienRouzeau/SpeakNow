using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    private static SettingsManager Instance { get; set; }
    public static SettingsManager instance => Instance;

    public Animator generalAnim;

    [Header("Onlgets")]
    [SerializeField]
    private GameObject ongletGeneral;
    [SerializeField]
    private GameObject ongletAudio;
    [SerializeField]
    private GameObject ongletControls;
    [SerializeField]
    private GameObject ongletGraphic;

    [Header("Controls")]
    [SerializeField]
    private Slider sensitivitySlider;
    [SerializeField]
    private TMPro.TextMeshProUGUI sensitivityText;

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

    // Start is called before the first frame update
    void Start()
    {
        float savedSensitivity = PlayerPrefs.GetFloat("sensitivity");
        if (savedSensitivity > sensitivitySlider.minValue)
        {
            sensitivitySlider.value = PlayerPrefs.GetFloat("sensitivity");
            sensitivityText.text = PlayerPrefs.GetFloat("sensitivity").ToString();
        }
    }

    public void SaveNewSensitivity(Slider _slider)
    {
        float _value = _slider.value;
        sensitivityText.text = _value.ToString();
        PlayerPrefs.SetFloat("sensitivity", _value);
        PlayerPrefs.Save();
    }

    #region onglets activation 
    public void OnShowGeneral()
    {
        ongletAudio.SetActive(false);
        ongletControls.SetActive(false);
        ongletGeneral.SetActive(true);
        ongletGraphic.SetActive(false);
    }

    public void OnShowAudio()
    {
        ongletAudio.SetActive(true);
        ongletControls.SetActive(false);
        ongletGeneral.SetActive(false);
        ongletGraphic.SetActive(false);
        generalAnim.SetTrigger("Normal");
    }

    public void OnShowControls()
    {
        ongletAudio.SetActive(false);
        ongletControls.SetActive(true);
        ongletGeneral.SetActive(false);
        ongletGraphic.SetActive(false);
        generalAnim.SetTrigger("Normal");
    }

    public void OnShowGraphic()
    {
        ongletAudio.SetActive(false);
        ongletControls.SetActive(false);
        ongletGeneral.SetActive(false);
        ongletGraphic.SetActive(true);
        generalAnim.SetTrigger("Normal");
    }

    #endregion
}
