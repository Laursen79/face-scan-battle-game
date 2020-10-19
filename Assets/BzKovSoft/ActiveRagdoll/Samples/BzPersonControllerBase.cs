using System;
using UnityEngine;

namespace BzKovSoft.ActiveRagdoll.Samples
{
	[DisallowMultipleComponent]
	public abstract class  BzPersonControllerBase : MonoBehaviour
	{
		[SerializeField]
		float _jumpForce = 4f;
		[SerializeField]
		bool _useBalancer = true;
		[SerializeField]
		float _movingTurnSpeed = 360f;
		[SerializeField]
		float _stationaryTurnSpeed = 180f;
		[SerializeField]
		ControllerMoveType _controllerMoveType = ControllerMoveType.Transform;

		[SerializeField]
		private LayerMask _groundLayer = 0;

		readonly int _animatorVelocityX = Animator.StringToHash("VelocityX");
		readonly int _animatorVelocityY = Animator.StringToHash("VelocityY");
		readonly int _animatorJump = Animator.StringToHash("Jump");
		readonly int _animatorAttack = Animator.StringToHash("Attack");
		readonly int _animatorJumpLeg = Animator.StringToHash("JumpLeg");
		readonly int _animatorJumpProgress = Animator.StringToHash("JumpProgress");
		const float RunCycleLegOffset = 0.2f;   // animation cycle offset (0-1) used for determining correct leg to jump off
		float _connectionFinishTime;
		bool _grounded;
		Vector3 _airVelocity;
		Vector3 _prevPos;
		Vector3 _velocity;

		protected Animator _animator;
		protected IBzRagdoll _ragdoll;
		protected IBzBalancer _balancer;
		protected CharacterController _characterController;
		protected Rigidbody _rigidbody;

		private void Awake()
		{
			if (_controllerMoveType == ControllerMoveType.Transform)
			{
				_characterController = GetComponent<CharacterController>();
			}
			else
			{
				_rigidbody = GetComponent<Rigidbody>();
			}
		}

		void OnEnable()
		{
			_animator = GetComponent<Animator>();
			_ragdoll = GetComponent<IBzRagdoll>();
			_balancer = GetComponent<IBzBalancer>();

			if (_balancer == null)
			{
				throw new InvalidOperationException("No balancer defined on the character");
			}

			_ragdoll.IsConnectedChanged -= ConnectionChanged;
			_ragdoll.IsConnectedChanged += ConnectionChanged;
		}

		void OnDisable()
		{
			_ragdoll.IsConnectedChanged -= ConnectionChanged;
		}

		private void ConnectionChanged()
		{
			if (_ragdoll.IsConnected)
			{
				_connectionFinishTime = Time.time + 2f;
			}
		}

		/// <summary>
		/// This method must be executed per each frame
		/// </summary>
		protected void Move(Vector3 moveDir, bool jumpPressed, bool attackPressed)
		{
			_airVelocity += Physics.gravity * Time.deltaTime;

			Vector3 balanceVelocity = Vector3.zero;
			if (_useBalancer & _ragdoll.IsConnected & _ragdoll.SpringRate > .9f)
			{
				Vector3 balVel = _balancer.BalanceVelocity / 30f;
				float magnitude = balVel.magnitude;
				if (_balancer.PoseError > 0.5f & _connectionFinishTime <= Time.time)
				{
					_ragdoll.IsConnected = false;
					return;
				}

				if (magnitude > 0.3f)
				{
					balanceVelocity = balVel;
				}
			}

			bool isRagdoll = _ragdoll.IsRagdolled && !_ragdoll.IsConnected;
			if (isRagdoll)
			{
				return;
			}

			var moveDirLoc = transform.InverseTransformDirection(moveDir) + transform.InverseTransformDirection(balanceVelocity);
			float turnAmount, forwardAmount;
			turnAmount = Mathf.Atan2(moveDirLoc.x, moveDirLoc.z);
			forwardAmount = moveDirLoc.z;

			ApplyExtraTurnRotation(turnAmount, forwardAmount);

			Vector3 jumpDir = -Physics.gravity.normalized;
			if (jumpPressed)
			{
				_airVelocity = _velocity;
				_airVelocity += jumpDir * _jumpForce;

				if (_controllerMoveType == ControllerMoveType.Rigidbody)
				{
					_rigidbody.velocity = _airVelocity;
				}

				// calculate which leg is behind, so as to leave that leg trailing in the jump animation
				// (This code is reliant on the specific run cycle offset in our animations,
				// and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
				float runCycle = Mathf.Repeat(
					_animator.GetCurrentAnimatorStateInfo(0).normalizedTime + RunCycleLegOffset, 1);

				float jumpLeg = (runCycle < 0.5f ? 1 : -1) * forwardAmount;
				_animator.SetFloat(_animatorJumpLeg, jumpLeg);
			}


			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				_ragdoll.IsConnected = !_ragdoll.IsConnected;
			}

			var pr = Vector3.Project(_airVelocity, jumpDir);
			var airVelocityUp = pr.magnitude * Mathf.Sign(Vector3.Dot(pr, jumpDir));
			bool inJump = !_grounded | airVelocityUp > 0f;
			if (inJump)
			{
				_animator.SetFloat(_animatorJumpProgress, airVelocityUp);
			}

			// update the animator parameters
			_animator.SetBool(_animatorJump, inJump);
			if (attackPressed)
			{
				_animator.SetTrigger(_animatorAttack);
			}

			_animator.SetFloat(_animatorVelocityX, turnAmount, 0.2f, Time.deltaTime);
			_animator.SetFloat(_animatorVelocityY, forwardAmount, 0.2f, Time.deltaTime);
		}

		private void ApplyExtraTurnRotation(float turnAmount, float forwardAmount)
		{
			if (_connectionFinishTime > Time.time | !_ragdoll.IsConnected)
			{
				return;
			}

			// help the character turn faster (this is in addition to root rotation in the animation)
			float turnSpeed = Mathf.Lerp(_stationaryTurnSpeed, _movingTurnSpeed, forwardAmount);
			transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
		}

		void OnAnimatorMove()
		{
			var move = _animator.deltaPosition;

			if (_controllerMoveType == ControllerMoveType.Transform)
			{
				move += _airVelocity * Time.deltaTime;

				if (_grounded & move.y <= 0 & move.y > -0.1f)
				{
					move.y = -0.1f;
				}

				_characterController.Move(move);
				transform.rotation *= _animator.deltaRotation;
				_grounded = _characterController.isGrounded;
			}
			else
			{
				_rigidbody.MovePosition(_rigidbody.position + move);
				transform.rotation *= _animator.deltaRotation;
				_grounded = false;
			}

			if (!_grounded && move.y <= 0f)
			{
				_grounded = Physics.Raycast(new Ray(transform.position + new Vector3(0f, 1f, 0f), Vector3.down), 1.1f, _groundLayer);
			}

			if (_grounded)
			{
				_airVelocity = Vector3.zero;
			}

			var pos = transform.position;
			_velocity = (pos - _prevPos) / Time.deltaTime;
			_prevPos = pos;
		}
	}
}