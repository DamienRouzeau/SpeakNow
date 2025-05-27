using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipelineTrigger : MonoBehaviour
{
    [SerializeField] RatBehaviour rat;
    [SerializeField] Material material;
    [SerializeField] float translucideValue;
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            rat.GoToOutSpot();
            GoTranslucide();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DeTranslucide();
        }
    }

    private void GoTranslucide()
    {
        material.color = new Color(material.color.r, material.color.g, material.color.b, translucideValue);
    }

    private void DeTranslucide()
    {
        material.color = new Color(material.color.r, material.color.g, material.color.b, 1);
    }


}
