using UnityEngine;

namespace Battle.Player
{
    [CreateAssetMenu]
    public class PlayerInput : ScriptableObject
    {
        // The finger used to move the player.
        public Touch[] JoyStickFingers = new []{default(Touch), default(Touch)};

        // The original position of the joystick finger when it first hit the screen.
        public Vector2[] TouchPoints= new []{default(Vector2), default(Vector2)};
    }
}
