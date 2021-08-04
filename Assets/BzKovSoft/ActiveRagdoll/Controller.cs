using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.BzKovSoft.ActiveRagdoll
{
    public class Controller : MonoBehaviour
    {
        public Vector2 Movement; //{ get; private set; }

        private Dictionary<int, (Vector2, int)> TrackedTouches = new Dictionary<int, (Vector2, int)>();
    
        private void Update()
        {
            foreach (var touch in Input.touches)
            {
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        TrackedTouches.Add(touch.fingerId, (touch.position, WhoDoesTouchBelongTo(touch)));
                        break;
                    case TouchPhase.Stationary:
                    case TouchPhase.Moved:
                        Movement = touch.position - TrackedTouches[touch.fingerId].Item1;
                        // The movement vector should be normalized in proportion to the height of the screen.
                        // Assuming an aspect ratio of about 2:1, the players will have a square of height * height to use for inputs.
                        Movement /= Screen.height * 0.5f;
                        if(Movement.magnitude > 1) 
                            Movement = Movement.normalized;
                        break;
                    case TouchPhase.Canceled:
                    case TouchPhase.Ended:
                        //Stop tracking the finger.
                        TrackedTouches.Remove(touch.fingerId);
                        Movement = Vector2.zero;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private int WhoDoesTouchBelongTo(Touch touch)
        {
            return touch.position.x < Screen.width * 0.5f ? 1 : 2;
        }
    }
}
