using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAnimatorDetector : MonoBehaviour
{
    [SerializeField] private repairCar car;
    public void Complete()
    {
        car.BackToMenu();
    }
}
