using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private bool move;
    
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;

    [SerializeField] private float distance;
    [SerializeField] private float height;
    [SerializeField] private Camera mainCamera;
    
    private Vector3 _middle;

    private void Start()
    {
        StartCoroutine(Reposition());
    }

    private IEnumerator Reposition()
    {
        while (true)
        {
            if (move)
            {
                _middle = player2.position - player1.position;
                mainCamera.transform.position = _middle - new Vector3(0, -height, -distance);
            }

            yield return null;
        }
    }
}
