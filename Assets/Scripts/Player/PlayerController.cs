using System;
using System.Collections;
using UnityEngine;
using BzKovSoft.ActiveRagdoll.Samples;
using UnityEngine.UI;

namespace Battle.Player
{
    public class PlayerController : BzPersonControllerBase
    {
        [SerializeField] private float reviveTime = 2.0f;
        // Player 1 or 2?
        [SerializeField] private int player = 1;
        [SerializeField] private Controller controller;
        private int _otherPlayer;
        private AudioSource _audio;
        [SerializeField] private AudioClip[] punchSounds;

        [SerializeField] private Slider healthBar;
        public float health = 100;

        private void Start()
        {
            _otherPlayer = player == 1 ? 2 : 1;
            _audio = GetComponent<AudioSource>();
        }

        private void Update()
        {
            var move = CalculateMovementVector(player);
            float h = move.x;
            float v = move.y;
            Transform camera = Camera.main.transform;

            Vector3 axisDir = new Vector3(h, 0, v);
            float dirMag = axisDir.magnitude;
            Vector3 moveDir = camera.TransformDirection(axisDir);
            moveDir.y = 0f;
            moveDir = moveDir.normalized * dirMag;

            Vector3 currDir = transform.forward;
            currDir.y = 0f;
            currDir.Normalize();

            bool jumpPressed = Input.GetKeyDown(KeyCode.Space);
            bool attackPressed = controller.isAttacking(player);
            bool connectPressed = Input.GetKeyDown(KeyCode.Q);
            bool ragdollPressed = Input.GetKeyDown(KeyCode.R);

                if (connectPressed)
            {
                _ragdoll.IsConnected = !_ragdoll.IsConnected;
            }

            if (ragdollPressed)
            {
                _ragdoll.IsRagdolled = !_ragdoll.IsRagdolled;
            }

            Move(moveDir, jumpPressed, attackPressed);
        }
        
        public Vector2 CalculateMovementVector(int player)
        {
            var newPos = player == 1
                ? controller.Player1Touch.position
                : controller.Player2Touch.position;
            var oldPos = player == 1
                ? controller.player1TouchPoint
                : controller.player2TouchPoint;
            
            var v = newPos - oldPos;
            v /= Screen.height * 0.5f;
            if(v.magnitude > 1) 
                v = v.normalized;
            return v;
        }

        private void OnCollisionEnter(Collision other)
        {
            var vel = other.relativeVelocity.magnitude;
            if (other.gameObject.layer == LayerMask.NameToLayer("Player" + _otherPlayer)
                && vel > 10)
            {
                _audio.PlayOneShot(punchSounds[0]);
                TakeDamage(vel);
                if (vel > 20)
                {
                    _ragdoll.IsConnected = false;
                    StartCoroutine(Revive());
                }
            }
        }

        private void TakeDamage(float damage)
        {
            var newHealth = health - damage;
            if (newHealth <= 0)
                GameManager.Die(player);
            else
            {
                health = newHealth; 
                UpdateHealthBar();
            }
        }

        private void UpdateHealthBar()
        {
            healthBar.value = health/100;
        }

        private IEnumerator Revive()
        {
            print("start doing stuff");
            yield return new WaitForSeconds(reviveTime);
            print("do it");
            _ragdoll.IsConnected = true;
        }
    }
}
