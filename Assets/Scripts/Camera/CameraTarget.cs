using System;
using UnityEngine;

namespace Battle
{
    public class CameraTarget : MonoBehaviour
    {

        protected virtual void Awake() =>
            CameraTargetManager.Instance.AddCameraTarget(this);
        


        
    }
}
