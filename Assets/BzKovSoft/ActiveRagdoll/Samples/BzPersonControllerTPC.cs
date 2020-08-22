using UnityEngine;

namespace BzKovSoft.ActiveRagdoll.Samples
{
	public class BzPersonControllerTPC : BzPersonControllerBase
	{
		private void Update()
		{
			float h = Input.GetAxis("Horizontal");
			float v = Input.GetAxis("Vertical");
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
			bool attackPressed = Input.GetKeyDown(KeyCode.Mouse0);
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
	}
}