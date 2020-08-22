using System;
using UnityEngine;

namespace BzKovSoft.ActiveRagdoll
{
	/// <summary>
	/// Character balance controller
	/// </summary>
	public class BzBalancerDefault : MonoBehaviour, IBzBalancer
	{
		[SerializeField]
		Transform _hips;
		[SerializeField]
		Transform _head;
		[SerializeField]
		Transform _lFoot;
		[SerializeField]
		Transform _rFoot;

		IBzRagdoll _ragdoll;

		Vector3 _balanceVelocity;
		public Vector3 BalanceVelocity { get { return _balanceVelocity; } }
		public float PoseError
		{
			get
			{
				var hips_skeleton = _ragdoll.GetSkeletonTransform(_hips);
				var head_skeleton = _ragdoll.GetSkeletonTransform(_head);
				return
					Mathf.Sqrt(
					(_hips.position - hips_skeleton.position).sqrMagnitude +
					(_head.position - head_skeleton.position).sqrMagnitude
					) / 2f;
			}
		}

		void OnEnable()
		{
			var animator = GetComponent<Animator>();
			if (_hips == null)
				_hips = animator.GetBoneTransform(HumanBodyBones.Hips);
			if (_head == null)
				_head = animator.GetBoneTransform(HumanBodyBones.Head);
			if (_lFoot == null)
				_lFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
			if (_rFoot == null)
				_rFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

			if (_hips == null | _head == null | _lFoot == null | _rFoot == null)
			{
				throw new InvalidOperationException("You need to explicitly specify properties for this type of model");
			}

			_ragdoll = GetComponent<IBzRagdoll>();
		}

		private void FixedUpdate()
		{
			if (!_ragdoll.IsConnected)
			{
				return;
			}

			var hips_skeleton = _ragdoll.GetSkeletonTransform(_hips);
			var head_skeleton = _ragdoll.GetSkeletonTransform(_head);
			var lFoot_skeleton = _ragdoll.GetSkeletonTransform(_lFoot);
			var rFoot_skeleton = _ragdoll.GetSkeletonTransform(_rFoot);

			Vector3 hipsShift = _hips.position - hips_skeleton.position;
			Vector3 headShift = _head.position - head_skeleton.position;
			Vector3 feetShift =
				(_lFoot.position + _rFoot.position) -
				(lFoot_skeleton.position + rFoot_skeleton.position);

			_balanceVelocity = hipsShift + headShift - (feetShift / 2f);
			_balanceVelocity /= Time.deltaTime;
		}
	}
}
