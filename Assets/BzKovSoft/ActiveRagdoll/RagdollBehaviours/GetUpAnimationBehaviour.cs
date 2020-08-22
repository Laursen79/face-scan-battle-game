using System;
using System.Collections;
using UnityEngine;

namespace BzKovSoft.ActiveRagdoll.RagdollBehaviours
{
	/// <summary>
	/// Active ragdoll event handler. This component calling the getup animation.
	/// </summary>
	public class GetUpAnimationBehaviour : MonoBehaviour, IRagdollBehaviour
	{
		IBzRagdoll _ragdoll;
		Animator _animator;

		IBzBalancer _balancer;
		[SerializeField]
		string _animationGetUpFromBelly = "GetUp.GetUpFromBelly";
		[SerializeField]
		string _animationGetUpFromBack = "GetUp.GetUpFromBack";
		[SerializeField]
		float _transitionTime = 2f;

		void OnEnable()
		{
			_ragdoll = GetComponent<IBzRagdoll>();
		}

		public void OnIsRagdolledChanged(bool newValue)
		{
		}

		public void OnIsConnectedChanged(bool newValue)
		{
			if (!newValue | !isActiveAndEnabled)
				return;

			_balancer = GetComponent<IBzBalancer>();

			if (_balancer == null)
				throw new InvalidOperationException("No balancer defined on the character");

			if (_balancer.PoseError < 0.3f)
				return;

			_animator = GetComponent<Animator>();

			Transform mainTransRag = _ragdoll.RagdollRigid.transform;
			Transform mainTransSkl = _ragdoll.GetSkeletonTransform(mainTransRag);

			// get distanse to floor
			Vector3 shiftPos = mainTransRag.position - mainTransSkl.position;
			shiftPos.y = GetDistanceToFloor(shiftPos.y, mainTransRag);

			// shift and rotate character node without children
			MoveNodeWithoutChildren(shiftPos, mainTransRag);

			//Initiate the get up animation
			string getUpAnim = CheckIfLieOnBack(mainTransRag) ? _animationGetUpFromBack : _animationGetUpFromBelly;
			_animator.Play(getUpAnim, 0, 0);    // you have to set time to 0, or if your animation will interrupt, next time animation starts from previous position

			_ragdoll.ResetLimbs();

			StartCoroutine(TransitionToAnimation());
		}

		private IEnumerator TransitionToAnimation()
		{
			float startTime = Time.time;
			for (float rate = 0; rate < 1f; rate += (Time.time - startTime) / _transitionTime)
			{
				_ragdoll.SpringRate = rate;

				yield return new WaitForFixedUpdate();
			}

			_ragdoll.SpringRate = 1f;
		}

		private float GetDistanceToFloor(float currentY, Transform hipsTransform)
		{
			RaycastHit[] hits = Physics.RaycastAll(new Ray(hipsTransform.position, Vector3.down));
			float distFromFloor = float.MinValue;

			foreach (RaycastHit hit in hits)
				if (!hit.transform.IsChildOf(transform))
					distFromFloor = Mathf.Max(distFromFloor, hit.point.y);

			if (Mathf.Abs(distFromFloor - float.MinValue) > Mathf.Epsilon)
				currentY = distFromFloor - transform.position.y;

			return currentY;
		}

		private void MoveNodeWithoutChildren(Vector3 shiftPos, Transform transformToNotMove)
		{
			Vector3 ragdollDirection = GetRagdollDirection(transformToNotMove);

			// shift character node position without children
			var tmpPos = transformToNotMove.position;
			transform.position += shiftPos;
			transformToNotMove.position = tmpPos;

			// rotate character node without children
			Vector3 forward = transform.forward;
			var tmpRot = transformToNotMove.rotation;
			transform.rotation = Quaternion.FromToRotation(forward, ragdollDirection) * transform.rotation;
			transformToNotMove.rotation = tmpRot;
		}

		private Vector3 GetRagdollDirection(Transform hipsTransform)
		{
			Vector3 ragdolledFeetPosition = (hipsTransform.position);// +
																	  //_anim.GetBoneTransform(HumanBodyBones.RightToes).position) * 0.5f;
			Vector3 ragdolledHeadPosition = GetBone(HumanBodyBones.Head).position;
			Vector3 ragdollDirection = ragdolledFeetPosition - ragdolledHeadPosition;
			ragdollDirection.y = 0;
			ragdollDirection = ragdollDirection.normalized;

			if (CheckIfLieOnBack(hipsTransform))
				return ragdollDirection;
			else
				return -ragdollDirection;
		}

		private bool CheckIfLieOnBack(Transform hipsTransform)
		{
			var left = GetBone(HumanBodyBones.LeftUpperLeg).position;
			var right = GetBone(HumanBodyBones.RightUpperLeg).position;
			var hipsPos = hipsTransform.position;

			left -= hipsPos;
			left.y = 0f;
			right -= hipsPos;
			right.y = 0f;

			var q = Quaternion.FromToRotation(left, Vector3.right);
			var t = q * right;

			return t.z < 0f;
		}

		private Transform GetBone(HumanBodyBones humanBodyBone)
		{
			var sklBone = _animator.GetBoneTransform(humanBodyBone);
			return _ragdoll.GetRagdollTransform(sklBone);
		}
	}
}

