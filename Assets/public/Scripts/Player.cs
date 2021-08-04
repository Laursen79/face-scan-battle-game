using System.Collections;
using System.Collections.Generic;
using Battle;
using UnityEngine;
using UnityEngine.XR;

namespace Battle
{
    [RequireComponent(typeof(CharacterController))]
    public class Player : CameraTarget
    {
        [SerializeField] private int playerID;
        [SerializeField] private float speed = 2.0f;

        [SerializeField] private KeyCode forward;
        [SerializeField] private KeyCode back;
        [SerializeField] private KeyCode right;
        [SerializeField] private KeyCode left;

        private CharacterController _controller;

        protected override void Awake()
        {
            _controller = GetComponent<CharacterController>();
            base.Awake();
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            var movement = Vector3.zero;
            int fwd, bck, rgt, lft;
            
            fwd = Input.GetKey(forward) ? 1 : 0;
            bck = Input.GetKey(back) ? 1 : 0;
            rgt = Input.GetKey(right) ? 1 : 0;
            lft = Input.GetKey(left) ? 1 : 0;

            _controller.SimpleMove(new Vector3(rgt - lft, 0, fwd - bck) * speed);
        }
    }
    
}
