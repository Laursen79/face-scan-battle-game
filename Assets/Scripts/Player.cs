using System.Collections;
using System.Collections.Generic;
using Battle;
using UnityEngine;

namespace Battle
{
    [RequireComponent(typeof(CharacterController))]
    public class Player : CameraTarget
    {
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
            _controller.SimpleMove(new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Horizontal")));
        }
    }
    
}
