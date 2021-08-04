using Battle.BzKovSoft.ActiveRagdoll;
using UnityEngine;

namespace BzKovSoft.ActiveRagdoll.Samples
{
	public class BzPersonControllerTPC : BzPersonControllerBase
	{
		[SerializeField] private Controller controller;
		private void Update()
		{
			float h = controller.Movement.x;
			float v = controller.Movement.y;
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
			bool attackPressed = Input.GetKeyDown(KeyCode.B);
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