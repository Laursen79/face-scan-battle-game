using System;
using System.Collections;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// The camera aims to capture all CameraTargets in the scene by positioning itself optimally - without being obnoxious.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        private float distanceFromTarget = 1.0f;
        [SerializeField]
        private float height = 4.0f;
        [SerializeField]
        private float speed = 1.0f;

        private float _hFOV=0;

        [SerializeField] private GameObject midpointMarker;

        private GameObject _marker;
        private void Awake()
        {
            ConfigureFOV();
            _marker = Instantiate(midpointMarker);
        }

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(Move());
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void ConfigureFOV()
        {
            var FOV = Camera.main.fieldOfView;
            var aspectRatio = Camera.main.aspect;
            _hFOV = Camera.VerticalToHorizontalFieldOfView(FOV, aspectRatio);
        }
        
        private Vector3 NextPosition()
        {
            var target1 = CameraTargetManager.Instance.TargetFarthestFromMiddle();
            var target1Position = target1.transform.position;
            var target2 = CameraTargetManager.Instance.TargetFarthestFromPoint(target1Position);
            var target2Position = target2.transform.position;
            
            var axisMid = (target1Position + target2Position) / 2;
            Rotate(axisMid);
            _marker.transform.position = axisMid;
            
            print("Angle: " + _hFOV/2);
            
            var midToTarget1 = Vector3.Distance(target1Position, axisMid);
            print("Mid-Target-distance: " + midToTarget1);
            
            var cameraDistance = midToTarget1 / Mathf.Tan(Mathf.Deg2Rad * _hFOV/2);
            print("CameraDistance: " + cameraDistance);
            var temp = (target2Position - target1Position).normalized;
            var direction = new Vector3
            {
                x = -temp.z,
                z = temp.x
            };
            return axisMid + direction * -(cameraDistance + distanceFromTarget) + Vector3.up * height;
        }

        private void Rotate(Vector3 midpoint)
        {
            transform.LookAt(midpoint);
        }

        private IEnumerator Move()
        {
            while (true)
            {
                var position = transform.position;
                position += (NextPosition() - position) * (speed / 100);
                transform.position = position;
                yield return null;
            }
        }
    
    }
}
