using System;
using UnityEngine;

namespace Battle
{
    public abstract class CameraTarget : MonoBehaviour
    {

        protected virtual void Awake() =>
            CameraTargetManager.Instance.AddCameraTarget(this);
        


        
    }
}
