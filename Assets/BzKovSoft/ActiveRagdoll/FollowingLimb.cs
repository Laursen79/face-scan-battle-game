using UnityEngine;

namespace BzKovSoft.ActiveRagdoll
{
	class FollowingLimb
	{
		public Transform sklTransRoot;
		public Transform ragTransRoot;
		public Rigidbody ragRigidRoot;

		public Transform sklTransBone;
		public Transform ragTransBone;
		public Rigidbody ragRigidBone;

		public Vector3 prevPos;
		public Quaternion prevRot;
		public Vector3 velocity;
		public Vector3 angularVelocity;
		public Quaternion limitRotation;
		
		public float maxAngle;
	}
}
