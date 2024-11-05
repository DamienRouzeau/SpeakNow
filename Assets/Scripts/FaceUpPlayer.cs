using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FaceUpPlayer : MonoBehaviour
{
    [SerializeField]
    private GameObject target;
    [SerializeField]
    private float range;
    [SerializeField]
    private float rotationSpeed;

    // Update is called once per frame
    void Update()
    {
        //float _distance = Vector3.Distance(transform.position, target.transform.position);
        //if (_distance <= range)
        //{
        //    Quaternion _rotation = Quaternion.LookRotation(target.transform.position - transform.position);

        //    _rotation.x = transform.rotation.x;
        //    _rotation.z = transform.rotation.z;
        //    _rotation.w = transform.rotation.w;
        //    transform.rotation = _rotation;
        //    //rotationAngle.y *= rotationSpeed;
        //    //transform.rotation = rotationAngle;
        //}
    }
}
