using UnityEngine;

namespace BzKovSoft.ActiveRagdoll.Samples
{
	[DisallowMultipleComponent]
	public class BzPersonIK : MonoBehaviour
	{
		[Range(0.5f, 1.5f)]
		public float _castOffset = 0.5f;
		[Range(-0.5f, 0.5f)]
		public float _footOffset = -0.11f;
		[Range(0f, 1f)]
		public float _footUpperLimit = 0.4f;
		[Range(0f, 1f)]
		public float _footLowerLimit = 0.4f;
		[Range(-1f, 0f)]
		public float _heightLowerLimit = -0.16f;
		[Range(0.01f, 0.2f)]
		public float _footWeightDistance = 0.05f;
		[SerializeField]
		float _correctionUpDownSpeed = 10f;

		Animator _animator;
		IBzRagdoll _ragdoll;
		float _heightOffsetCorrection = 0f;

		FootHitResult _leftFootHit;
		FootHitResult _rightFootHit;

		void OnEnable()
		{
			_leftFootHit = new FootHitResult();
			_rightFootHit = new FootHitResult();

			_animator = GetComponent<Animator>();
			_ragdoll = GetComponent<IBzRagdoll>();
		}


		private void FixedUpdate()
		{
			bool isRagdoll = _ragdoll.IsRagdolled && !_ragdoll.IsConnected;
			if (isRagdoll)
			{
				return;
			}

			FootPositionSolver(_leftFootHit);
			FootPositionSolver(_rightFootHit);
		}

		private void OnAnimatorIK(int layerIndex)
		{
			bool isRagdoll = _ragdoll.IsRagdolled && !_ragdoll.IsConnected;
			if (isRagdoll)
			{
				return;
			}

			if (layerIndex != 0)
				return;

			float totalOffset;
			if (_leftFootHit.Success & _rightFootHit.Success)
			{
				var rootPos = _animator.rootPosition;
				float lOffsetPosition = _leftFootHit.HitPosition.y + _leftFootHit.FootHeightCorrection - rootPos.y;
				float rOffsetPosition = _rightFootHit.HitPosition.y + _rightFootHit.FootHeightCorrection - rootPos.y;
				totalOffset = Mathf.Min(lOffsetPosition, rOffsetPosition);
				totalOffset = Mathf.Max(totalOffset, _heightLowerLimit);
			}
			else
			{
				totalOffset = 0f;
			}

			_heightOffsetCorrection = Mathf.Lerp(_heightOffsetCorrection, totalOffset, _correctionUpDownSpeed * Time.deltaTime);
			var bodyPosition = _animator.bodyPosition;
			bodyPosition.y += _heightOffsetCorrection;
			_animator.bodyPosition = bodyPosition;

			ProcessFoot(HumanBodyBones.LeftFoot, AvatarIKGoal.LeftFoot, _leftFootHit);
			ProcessFoot(HumanBodyBones.RightFoot, AvatarIKGoal.RightFoot, _rightFootHit);
		}

		private void FootPositionSolver(FootHitResult footResult)
		{
			Vector3 sklFootPos = footResult.StepOrigPosition;

			var footCastBegin = new Vector3(sklFootPos.x, transform.position.y + _footUpperLimit + _castOffset, sklFootPos.z);
			var castVector = new Vector3(0f, -_castOffset - _footLowerLimit - _footUpperLimit, 0f);
			Debug.DrawRay(footCastBegin, new Vector3(0, -_castOffset, 0), Color.yellow);
			Debug.DrawRay(footCastBegin + new Vector3(0, -_castOffset, 0), new Vector3(0, -_footLowerLimit - _footUpperLimit, 0), Color.blue);

			RaycastHit hit;
			footResult.Success = HitGround(footCastBegin, castVector, out hit);

			if (footResult.Success)
			{
				footResult.HitPosition = hit.point;
				footResult.Normal = hit.normal;
			}
		}

		private void ProcessFoot(HumanBodyBones foot, AvatarIKGoal goal, FootHitResult footResult)
		{
			var originalPos = _animator.GetBoneTransform(foot).position;

			var hitFinalPos = footResult.HitPosition;
			var shiftPos = new Vector3(0f, _footOffset, 0f);

			Quaternion rotDelta;
			if (footResult.Success)
			{
				rotDelta = Quaternion.FromToRotation(Vector3.up, footResult.Normal);
			}
			else
			{
				rotDelta = Quaternion.identity;
			}

			var posDelta = rotDelta * shiftPos;
			Vector3 stepOriginPos = originalPos + posDelta;
			footResult.StepOrigPosition = stepOriginPos;
			footResult.FootHeightCorrection = _footOffset - posDelta.y;

			stepOriginPos += Vector3.up * _heightOffsetCorrection;

			if (!footResult.Success)
			{
				return;
			}

			DrawRectangle(stepOriginPos, Vector3.up, Color.yellow);
			DrawRectangle(hitFinalPos, footResult.Normal, Color.blue);

			float ikFootWeight;
			float posYDist = hitFinalPos.y - stepOriginPos.y;
			if (posYDist > 0f)
			{
				ikFootWeight = 1f;
			}
			else
			{
				ikFootWeight = (_footWeightDistance + posYDist) / _footWeightDistance;
				ikFootWeight = Mathf.Clamp01(ikFootWeight);
				DrawWeight(stepOriginPos, ikFootWeight);
			}

			Vector3 posDiff = hitFinalPos - stepOriginPos;
			footResult.PosDiff = Vector3.Lerp(footResult.PosDiff, posDiff, _correctionUpDownSpeed * Time.deltaTime);
			Vector3 ikPos = stepOriginPos + footResult.PosDiff;

			_animator.SetIKPosition(goal, ikPos - posDelta);
			_animator.SetIKPositionWeight(goal, ikFootWeight);

			footResult.RotDiff = Quaternion.Lerp(footResult.RotDiff, rotDelta, _correctionUpDownSpeed * Time.deltaTime);
			var rot = footResult.RotDiff * transform.rotation;

			_animator.SetIKRotation(goal, rot);
			_animator.SetIKRotationWeight(goal, ikFootWeight);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		private void DrawWeight(Vector3 originalPos, float ikFootWeight)
		{
			if (ikFootWeight > 0f)
			{
				Vector3 endPos = originalPos + new Vector3(0f, -_footWeightDistance, 0f);
				Vector3 forward = transform.forward * 0.1f;
				Debug.DrawLine(originalPos + forward, endPos, Color.red);
				Debug.DrawLine(originalPos - forward, endPos, Color.red);
			}
		}

		[System.Diagnostics.Conditional("DEBUG")]
		private void DrawRectangle(Vector3 hitPos, Vector3 up, Color color)
		{
			var left = Vector3.Cross(transform.forward, up).normalized;
			var forward = Vector3.Cross(up, left).normalized;
			left *= 0.1f;
			forward *= 0.15f;
			Debug.DrawRay(hitPos + forward - left, left * 2, color);
			Debug.DrawRay(hitPos - forward - left, left * 2, color);
			Debug.DrawRay(hitPos - forward - left, forward * 2, color);
			Debug.DrawRay(hitPos - forward + left, forward * 2, color);
		}

		private bool HitGround(Vector3 castBegin, Vector3 dir, out RaycastHit hit)
		{
			var hits = Physics.RaycastAll(castBegin, dir, dir.magnitude);
			int hitGroundIndex = -1;
			float distance = float.MaxValue;

			for (int i = 0; i < hits.Length; i++)
			{
				var hit2 = hits[i];
				if (hit2.transform.IsChildOf(gameObject.transform))
					continue;

				if (hitGroundIndex == -1 || hit2.distance < distance)
				{
					hitGroundIndex = i;
					distance = hit2.distance;
				}
			}

			if (hitGroundIndex == -1)
			{
				hit = default(RaycastHit);
				return false;
			}
			else
			{
				hit = hits[hitGroundIndex];
				return true;
			}
		}

		class FootHitResult
		{
			public bool Success;
			public Vector3 StepOrigPosition;
			public Vector3 HitPosition;
			public Vector3 Normal;
			public float FootHeightCorrection;
			public Vector3 PosDiff;
			public Quaternion RotDiff;
		}
	}
}