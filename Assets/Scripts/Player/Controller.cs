using System;
using UnityEngine;

namespace Battle.Player
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private PlayerInput _input;

        public Touch Player1Touch;
        public Touch Player2Touch;
        
        public Vector2 player1TouchPoint;
        public Vector2 player2TouchPoint;

        public bool isAttacking(int player)
        {
            bool result;
            if (player == 1)
            {
                result = player1Punch;
                player1Punch = false;
            }
            else
            {
                result = player2Punch;
                player2Punch = false;
            }

            return result;
        }
        public bool player1Punch;
        public bool player2Punch;

        private void Update()
        {
            foreach (var touch in Input.touches)
            {
                // Check who the finger should belong to.
                var player = WhoDoesTouchBelongTo(touch);
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        switch (player)
                        {
                            case 1 when Player1Touch.Equals(default(Touch)):
                                Player1Touch = touch;
                                player1TouchPoint = touch.position;
                                player1Punch = true;
                                break;
                            case 2 when Player2Touch.Equals(default(Touch)):
                                Player2Touch = touch;
                                player2Punch = true;
                                player2TouchPoint = touch.position;
                                break;
                        }
                        break;
                    
                    case TouchPhase.Canceled:
                    case TouchPhase.Ended:
                        //Stop tracking the finger.
                        if (touch.fingerId == Player1Touch.fingerId)
                        {
                            Player1Touch = default;
                            player1TouchPoint = Vector2.zero;
                        } if (touch.fingerId == Player2Touch.fingerId)
                        {
                            Player2Touch = default;
                            player2TouchPoint = Vector2.zero;
                        }
                        break;
                    case TouchPhase.Moved:
                        switch (player)
                        {
                            case 1 when Player1Touch.fingerId == touch.fingerId:
                                Player1Touch = touch;
                                break;
                            case 2 when Player2Touch.fingerId == touch.fingerId:
                                Player2Touch = touch;
                                break;
                        }
                        break;
                    case TouchPhase.Stationary:
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
