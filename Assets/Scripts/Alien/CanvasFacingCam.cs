using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasFacingCam : MonoBehaviour
{
    [SerializeField]
    private GameObject camera;

    private void FixedUpdate()
    {
        transform.LookAt(camera.transform.position);
    }
}
