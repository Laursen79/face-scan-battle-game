using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private Transform _mainCameraTransform;
    private void Awake()
    {
        _mainCameraTransform = Camera.main.transform;
    }
    
    void Update()
    {
        transform.LookAt(_mainCameraTransform.position);
    }
}
