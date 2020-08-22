using UnityEngine;

namespace BzKovSoft.ActiveRagdoll.Samples
{
	public class MoveCamera : MonoBehaviour
	{
		public float TurnSpeed = 4.0f;
		public Transform target;

		private float yaw = 0f;
		private float pitch = 0f;

		void Update()
		{
			yaw += Input.GetAxis("Mouse X");
			pitch -= Input.GetAxis("Mouse Y");
			transform.eulerAngles = new Vector3(TurnSpeed * pitch, TurnSpeed * yaw, 0.0f);
		}

		private void FixedUpdate()
		{
			var curCamPos = transform.position;
			var targetPos = target.position;
			transform.position = Vector3.Lerp(curCamPos, targetPos, 10f * Time.deltaTime);
		}
	}
}