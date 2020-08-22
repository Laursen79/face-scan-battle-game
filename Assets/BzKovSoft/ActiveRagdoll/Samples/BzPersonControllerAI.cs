using UnityEngine;

namespace BzKovSoft.ActiveRagdoll.Samples
{
	public class BzPersonControllerAI : BzPersonControllerBase
	{
#pragma warning disable 0649
		[SerializeField]
		GameObject _enemy;
#pragma warning restore 0649

		float nextJumpTime;
		float nextHitTime;

		private void Update()
		{
			if (_ragdoll.IsConnected)
			{
				transform.LookAt(_enemy.transform, Vector3.up);
			}

			Vector3 moveDir = _enemy.transform.position - transform.position;
			moveDir = moveDir.normalized * Mathf.Clamp01(moveDir.magnitude - 0.8f);

			bool jumpPressed = false;
			bool attackPressed = false;
			if (nextJumpTime < Time.time && moveDir.magnitude > 0.9f)
			{
				jumpPressed = true;
				nextJumpTime = Time.time + 20f;
			}
			if (nextHitTime < Time.time)
			{
				attackPressed = true;
				nextHitTime = Time.time + 2f;
			}

			Move(moveDir, jumpPressed, attackPressed);
		}
	}
}