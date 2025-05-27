using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableObject", menuName = "Camera", order = 0)]

public class CameraProfil : ScriptableObject
{
    public float distanceFromPlayer;
    public float minDistanceFromPlayer;
    public float cameraHeight;
    public float headheightOffset;
    public float minVerticalAngle;
    public float maxVerticalAngle;
}

