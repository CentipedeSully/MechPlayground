using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BindCamera : MonoBehaviour
{
    [SerializeField] private Transform _targetTransform;

    // Update is called once per frame
    void Update()
    {
        if (_targetTransform != null)
        {
            transform.position = _targetTransform.position;
            transform.rotation = _targetTransform.rotation;
        }
            
    }
}
