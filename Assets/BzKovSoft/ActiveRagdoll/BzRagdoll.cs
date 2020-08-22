using BzKovSoft.ActiveRagdoll.RagdollBehaviours;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace BzKovSoft.ActiveRagdoll
{
	/// <summary>
	/// Active ragdoll component
	/// </summary>
	[DisallowMultipleComponent]
	public class BzRagdoll : MonoBehaviour, IBzRagdoll
	{
#pragma warning disable 0649
		[SerializeField]
		bool _convertOnStart;
		[Range(0.1f, 1f)]
		public float _massReduction = 0.1f;
		[SerializeField]
		Rigidbody _ragdollRigid;

		public ControllerMoveType _controllerMoveType;
		public RigidbodyTurnoffType _rigidbodyTurnoffType;
		public BzBoneConfiguration[] _boneConfigurations;
		public float _correctVelocity = 10f;
		public float _springVelocity = 100f;
		[SerializeField, Range (0f, 1f)]
		float _springRate = 1f;

		[SerializeField]
		bool _deleteJoints;
		[SerializeField]
		bool _deleteColliders;
#pragma warning restore 0649

		Animator _animator;
		Dictionary<Transform, Transform> _tranToSkelet;
		Dictionary<Transform, Transform> _tranToRagdoll;
		Transform _skeletonObject;
		FixedLimb[] _fixedLimbs;
		FollowingLimb[] _followingLimbs;
		FollowingLimb _rootLimb;
		bool _ragdolled;
		bool _isConnected = false;
		Vector3 _prevPos;
		Vector3 _velocity;

		public event Propertychanged IsConnectedChanged;
		public event Propertychanged IsRagdolledChanged;

		public bool IsRagdolled
		{
			get { return _ragdolled; }
			set
			{
				if (_ragdolled == value)
					return;

				IsConnected = false;

				_ragdolled = value;

				if (value)
				{
					_animator.enabled = false;
					ConvertToRagdoll();
					EnableRigids(true);
				}
				else
				{
					EnableRigids(false);
					ConvertFromRagdoll();
					_animator.enabled = true;
				}

				OnIsRagdolledChanged();
			}
		}

		public Rigidbody RagdollRigid { get { return _ragdollRigid; } }

		/// <summary>
		/// is ragdoll connected to skeleton
		/// </summary>
		public bool IsConnected
		{
			get { return _isConnected; }
			set
			{
				if (!gameObject.activeInHierarchy)
				{
					throw new InvalidOperationException("GameObject must be active");
				}

				if (_isConnected == value)
					return;

				IsRagdolled = true;
				_isConnected = value;

				_animator.enabled = _isConnected;

				float massFactor = _isConnected ? _massReduction : 1f / _massReduction;
				ApplyMassFactor(massFactor);

				if (_isConnected)
				{
					ResetLimbs();
				}
				else
				{
					if (_controllerMoveType != ControllerMoveType.Rigidbody)
					{
						for (int i = 0; i < _followingLimbs.Length; i++)
						{
							var limb = _followingLimbs[i];
							ApplyVelocity(limb);
						}

						ApplyVelocity(_rootLimb);
					}
				}

				OnIsConnectedChanged();
			}
		}

		public float SpringRate
		{
			get { return _springRate; }
			set
			{
				_springRate = value;
			}
		}

		private void OnIsRagdolledChanged()
		{
			var bahaviours = GetComponents<IRagdollBehaviour>();
			for (int i = 0; i < bahaviours.Length; i++)
			{
				var behaviour = bahaviours[i];
				behaviour.OnIsRagdolledChanged(IsRagdolled);
			}
			IsRagdolledChanged?.Invoke();
		}

		private void OnIsConnectedChanged()
		{
			var behaviours = GetComponents<IRagdollBehaviour>();
			for (int i = 0; i < behaviours.Length; i++)
			{
				var behaviour = behaviours[i];
				behaviour.OnIsConnectedChanged(IsConnected);
			}

			IsConnectedChanged?.Invoke();
		}

		private void ApplyMassFactor(float massFactor)
		{
			_rootLimb.ragRigidBone.mass *= massFactor;
			for (int i = 0; i < _followingLimbs.Length; i++)
			{
				var limb = _followingLimbs[i];
				limb.ragRigidBone.mass *= massFactor;
			}
		}

		private void ApplyVelocity(FollowingLimb limb)
		{
			limb.ragRigidBone.velocity += _velocity;
		}

		public Transform GetRagdollTransform(Transform skeletonTransform)
		{
			if (!IsRagdolled)
			{
				throw new InvalidOperationException("Ragdoll must be ragdolled");
			}

			Transform result;
			if (_tranToRagdoll.TryGetValue(skeletonTransform, out result))
			{
				return result;
			}

			throw new KeyNotFoundException("Key '" + skeletonTransform.ToString() + "' not found");
		}

		public Transform GetSkeletonTransform(Transform ragdollTransform)
		{
			if (!IsRagdolled)
			{
				throw new InvalidOperationException("Ragdoll must be ragdolled");
			}

			Transform result;
			if (_tranToSkelet.TryGetValue(ragdollTransform, out result))
			{
				return result;
			}

			throw new KeyNotFoundException("Key '" + ragdollTransform.ToString() + "' not found");
		}

		public void ApplyModifiedProperties()
		{
			if (!IsRagdolled | _boneConfigurations == null)
			{
				return;
			}

			_rootLimb.maxAngle = 0f;
			foreach (var limb in _followingLimbs)
			{
				limb.maxAngle = 0f;
			}

			foreach (var boneConfig in _boneConfigurations)
			{
				FollowingLimb foundLimb = null;
				if (boneConfig.bone == _rootLimb.ragRigidBone)
				{
					foundLimb = _rootLimb;
				}
				else
				{
					foreach (var limb in _followingLimbs)
					{
						if (boneConfig.bone == limb.ragRigidBone)
						{
							foundLimb = limb;
							break;
						}
					}
				}

				if (foundLimb != null)
				{
					foundLimb.maxAngle = boneConfig.maxAngle;
				}
				else
				{
					UnityEngine.Debug.Log("Incorrect bone configuration for " + (boneConfig.bone == null ? "[null]" : boneConfig.bone.name), gameObject);
				}
			}
		}

		private void Awake()
		{
			_animator = GetComponent<Animator>();

			EnableRigids(false);
			ResolveMainColliders(true);
		}

		private void Start()
		{
			if (_convertOnStart)
			{
				IsRagdolled = true;
				IsConnected = true;
			}
		}

		private void EnableRigids(bool enable)
		{
			var rigids = _ragdollRigid.GetComponentsInChildren<Rigidbody>();

			for (int i = 0; i < rigids.Length; i++)
			{
				var rigid = rigids[i];
				if (rigid.transform == transform)
					continue;

				switch (_rigidbodyTurnoffType)
				{
					case RigidbodyTurnoffType.IsKinimatic:
						rigid.isKinematic = !enable;
						break;
					case RigidbodyTurnoffType.DetectCollisions:
						rigid.detectCollisions = enable;
						break;
					case RigidbodyTurnoffType.Full:
						rigid.isKinematic = !enable;
						rigid.detectCollisions = enable;
						break;
					default: throw new InvalidOperationException();
				}
			}
		}

		void FixedUpdate()
		{
			var pos = (_rootLimb?.ragTransBone ?? transform).position;
			_velocity = (pos - _prevPos) / Time.deltaTime;
			_prevPos = pos;

			if (_isConnected)
			{
				UpdateLimbs();
			}
		}

		void ConvertToRagdoll()
		{
			Validate();

			var ragdollTrans = _ragdollRigid.transform;
			_skeletonObject = new GameObject(ragdollTrans.name).transform;
			_skeletonObject.SetParent(ragdollTrans.parent, false);

			var ragdollRoot = new GameObject("ragdollRoot");
			ragdollRoot.transform.SetParent(ragdollTrans.parent, false);
			ragdollTrans.SetParent(ragdollRoot.transform, false);

			CloneSkeleton(ragdollTrans, _skeletonObject);
			FindLimbs(ragdollRoot);
			ApplyModifiedProperties();

			AnimatorRebind();

			SomeDebugOperationsStart();
		}

		void ConvertFromRagdoll()
		{
			_ragdollRigid.transform.SetParent(_skeletonObject.transform.parent, false);
			_skeletonObject.gameObject.name = "For deletion";
			Destroy(_skeletonObject.gameObject);
			Destroy(_rootLimb.ragTransRoot.gameObject);

			SomeDebugOperationsEnd();

			AnimatorRebind();

			_skeletonObject = null;
			_rootLimb = null;
			_fixedLimbs = null;
			_followingLimbs = null;
			_tranToSkelet = null;
			_tranToRagdoll = null;
		}

		private void AnimatorRebind()
		{
			var transs = GetComponentsInChildren<Transform>();
			var transPos = new Vector3[transs.Length];
			var transRot = new Quaternion[transs.Length];
			for (int i = 0; i < transs.Length; i++)
			{
				var trans = transs[i];
				transPos[i] = trans.localPosition;
				transRot[i] = trans.localRotation;
			}

			var clipPositions = new AnimatorStateInfo[_animator.layerCount];
			for (int i = 0; i < _animator.layerCount; i++)
			{
				clipPositions[i] = _animator.GetCurrentAnimatorStateInfo(i);
			}

			var parameters = _animator.parameters;
			foreach (var parameter in parameters)
			{
				switch (parameter.type)
				{
					case AnimatorControllerParameterType.Bool:
						parameter.defaultBool = _animator.GetBool(parameter.nameHash);
						break;
					case AnimatorControllerParameterType.Float:
						parameter.defaultFloat = _animator.GetFloat(parameter.nameHash);
						break;
					case AnimatorControllerParameterType.Int:
						parameter.defaultInt = _animator.GetInteger(parameter.nameHash);
						break;
				}
			}

			_animator.Rebind();

			foreach (var parameter in parameters)
			{
				switch (parameter.type)
				{
					case AnimatorControllerParameterType.Bool:
						_animator.SetBool(parameter.nameHash, parameter.defaultBool);
						break;
					case AnimatorControllerParameterType.Float:
						_animator.SetFloat(parameter.nameHash, parameter.defaultFloat, 0, 0);
						break;
					case AnimatorControllerParameterType.Int:
						_animator.SetInteger(parameter.nameHash, parameter.defaultInt);
						break;
				}
			}

			for (int i = 0; i < transs.Length; i++)
			{
				var trans = transs[i];
				trans.localPosition = transPos[i];
				trans.localRotation = transRot[i];
			}

			for (int i = 0; i < _animator.layerCount; i++)
			{
				var state = clipPositions[i];
				_animator.Play(state.shortNameHash, i, state.normalizedTime);
			}
		}

		private void ResolveMainColliders(bool offCollision)
		{
			var mainCollider = GetComponent<Collider>();
			if (mainCollider == null)
				return;

			var colliders = GetComponentsInChildren<Collider>();
			for (int i = 0; i < colliders.Length; i++)
			{
				var collider = colliders[i];
				if (collider == mainCollider)
					continue;

				Physics.IgnoreCollision(mainCollider, collider, offCollision);
				collider.isTrigger = !offCollision;
			}
		}

		[Conditional("DEBUG")]
		private void Validate()
		{
			if (_ragdollRigid == null)
			{
				throw new ArgumentNullException("'Ragdoll Rigid' must have value");
			}

			if (_ragdollRigid.transform == this.transform)
			{
				throw new ArgumentException("'Ragdoll Rigid' cannot be on the character object");
			}

			for (var tr = _ragdollRigid.transform.parent; tr != this.transform; tr = tr.parent)
			{
				if (tr.GetComponent<Rigidbody>() != null)
				{
					throw new ArgumentException("'Ragdoll Rigid' have to be set to the top most Rigidbody object");
				}
			}
		}

		private void CloneSkeleton(Transform from, Transform to)
		{
			_tranToSkelet = new Dictionary<Transform, Transform>();
			_tranToRagdoll = new Dictionary<Transform, Transform>();
			CloneSkeletonRec(from, to);
		}

		public void ResetLimbs()
		{
			// For some reason I'm not able to rid of some movement after animation was changed.
			// So I need to remember position to restore it later
			var pos = transform.localPosition;
			var rot = transform.localRotation;
			_animator.Update(0); // update skeleton bone positions and rotations according to animation
			transform.localPosition = pos;
			transform.localRotation = rot;

			CalcMovement(_rootLimb);

			for (int i = 0; i < _followingLimbs.Length; i++)
			{
				var limb = _followingLimbs[i];
				CalcMovement(limb);
			}

			UpdateFixed();
		}

		[Conditional("DEBUG")]
		private void SomeDebugOperationsStart()
		{
			var ragdollGO = _ragdollRigid.gameObject;
			if (ragdollGO.GetComponent<SkeletonDrawer>() == null)
			{
				ragdollGO.AddComponent<SkeletonDrawer>().Color = Color.blue;
			}
			if (_skeletonObject.gameObject.GetComponent<SkeletonDrawer>() == null)
			{
				_skeletonObject.gameObject.AddComponent<SkeletonDrawer>().Color = Color.green;
			}

			if (_deleteColliders)
			{
				foreach (var item in ragdollGO.GetComponentsInChildren<Collider>())
				{
					Destroy(item);
				}
			}

			for (int i = 0; i < _followingLimbs.Length; i++)
			{
				var limb = _followingLimbs[i];

				if (_deleteJoints)
				{
					var joint = limb.ragTransBone.GetComponent<Joint>();
					if (joint != null)
					{
						Destroy(joint);
					}
				}
			}
		}

		[Conditional("DEBUG")]
		private void SomeDebugOperationsEnd()
		{
			var ragdollGO = _ragdollRigid.gameObject;
			Destroy(ragdollGO.GetComponent<SkeletonDrawer>());
			Destroy(_skeletonObject.gameObject.AddComponent<SkeletonDrawer>());
		}

		private void FindLimbs(GameObject ragdollRoot)
		{
			var followingLimbs = new List<FollowingLimb>();
			var fixedLimbs = new List<FixedLimb>();

			foreach (var item in _tranToSkelet)
			{
				var ragTransBone = item.Key;
				var sklTransBone = item.Value;
				var ragRigidBone = ragTransBone.GetComponent<Rigidbody>();
				var curJoint = ragTransBone.GetComponent<Joint>();

				if (ragRigidBone == null)
				{
					FixedLimb fixedLimb;
					fixedLimb.sklTransBone = sklTransBone;
					fixedLimb.ragTransBone = ragTransBone;
					fixedLimbs.Add(fixedLimb);
					continue;
				}

				if (curJoint == null)
				{
					if (_ragdollRigid == null)
					{
						_ragdollRigid = ragRigidBone;
					}
					else if (ragRigidBone.transform != _ragdollRigid.transform)
					{
						throw new InvalidOperationException(
							"You must have only one rigid without joint attached. " +
							"And it must be a root bone");
					}

					var ragTransRoot = ragdollRoot.transform;

					var followingLimb = new FollowingLimb();

					followingLimb.sklTransRoot = this.transform;
					followingLimb.sklTransBone = sklTransBone;

					followingLimb.ragTransRoot = ragTransRoot;
					followingLimb.ragTransBone = ragTransBone;
					followingLimb.ragRigidBone = ragRigidBone;

					_rootLimb = followingLimb;

					continue;
				}
				else
				{
					var ragRigRoot = curJoint.connectedBody;
					var ragTransRoot = ragRigRoot.transform;

					var followingLimb = new FollowingLimb();

					followingLimb.sklTransRoot = GetSkeletonTransform(ragTransRoot);
					followingLimb.sklTransBone = sklTransBone;

					followingLimb.ragTransRoot = ragTransRoot;
					followingLimb.ragRigidRoot = ragRigRoot;
					followingLimb.ragTransBone = ragTransBone;
					followingLimb.ragRigidBone = ragRigidBone;

					followingLimbs.Add(followingLimb);
				}
			}

			_followingLimbs = followingLimbs.ToArray();
			_fixedLimbs = fixedLimbs.ToArray();
		}

		private void UpdateLimbs()
		{
			UpdateFixed();

			CalcMovement(_rootLimb);
			AddLimbMovement(_rootLimb);

			for (int i = 0; i < _followingLimbs.Length; i++)
			{
				var limb = _followingLimbs[i];
				CalcMovement(limb);
				AddLimbMovement(limb);
			}
		}

		private void UpdateFixed()
		{
			for (int i = 0; i < _fixedLimbs.Length; i++)
			{
				var limb = _fixedLimbs[i];
				limb.ragTransBone.localPosition = limb.sklTransBone.localPosition;
				limb.ragTransBone.localRotation = limb.sklTransBone.localRotation;
			}
		}

		private void CalcMovement(FollowingLimb limb)
		{
			Vector3 pos;
			if (_controllerMoveType == ControllerMoveType.Transform)
			{
				pos = transform.position;
			}
			else // if Rigidbody
			{
				pos = Vector3.zero;
			}

			// add position delta
			Vector3 centerOfMass = limb.ragRigidBone.centerOfMass;
			Vector3 sklPos = limb.sklTransBone.position + limb.sklTransBone.TransformDirection(centerOfMass) - pos;
			Vector3 ragPos = limb.ragTransBone.position + limb.ragTransBone.TransformDirection(centerOfMass) - pos;

			Vector3 positionDeltaSklSkl = sklPos - limb.prevPos;
			Vector3 positionDeltaSklRag = sklPos - ragPos;
			positionDeltaSklRag = Vector3.ClampMagnitude(positionDeltaSklRag, _correctVelocity);
			Vector3 positionDelta = Vector3.Lerp(positionDeltaSklSkl, positionDeltaSklRag, _correctVelocity * Time.deltaTime);
			limb.prevPos = sklPos;
			limb.velocity = Vector3.Lerp(limb.ragRigidBone.velocity, positionDelta / Time.deltaTime, _springVelocity * _springRate * Time.deltaTime);

			Quaternion rotationDeltaSklSkl = limb.prevRot * Quaternion.Inverse(limb.sklTransBone.rotation);
			Quaternion rotationDeltaSklRag = limb.sklTransBone.rotation * Quaternion.Inverse(limb.ragTransBone.rotation);
			limb.prevRot = limb.sklTransBone.rotation;

			// add extra rotation
			limb.limitRotation = Quaternion.identity;
			if (limb.maxAngle > float.Epsilon & limb.maxAngle < 359f)
			{
				rotationDeltaSklRag.ToAngleAxis(out float angleSklRag, out Vector3 axisSklRag);
				angleSklRag %= 360f;
				if (angleSklRag > 180f)
				{
					angleSklRag -= 360f;
				}

				if (Mathf.Abs(angleSklRag) < 1f)
				{
					axisSklRag = Vector3.up;
				}
				if (Mathf.Abs(angleSklRag) > limb.maxAngle)
				{
					float newAngle = limb.maxAngle * Mathf.Sign(angleSklRag);
					float limitAngle = angleSklRag - newAngle;
					limb.limitRotation = Quaternion.AngleAxis(limitAngle, axisSklRag);
					rotationDeltaSklRag *= Quaternion.Inverse(limb.limitRotation);
				}
			}

			// add rotation delta
			Quaternion rotationDelta = Quaternion.Lerp(rotationDeltaSklSkl, rotationDeltaSklRag, _correctVelocity * Time.deltaTime);
			rotationDelta.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);
			angleInDegrees %= 360f;
			if (angleInDegrees > 180f)
			{
				angleInDegrees -= 360f;
			}

			if (Mathf.Abs(angleInDegrees) < 1f)
			{
				rotationAxis = Vector3.up;
			}


			Vector3 angularDisplacement = rotationAxis * angleInDegrees * Mathf.Deg2Rad;
			Vector3 angularSpeed = angularDisplacement / Time.deltaTime;

			if (float.IsNaN(angularSpeed.x))
			{
				UnityEngine.Debug.Log(rotationAxis + " _ " + angleInDegrees);
			}

			limb.angularVelocity = Vector3.Lerp(limb.ragRigidBone.angularVelocity, angularSpeed, _springVelocity * _springRate * Time.deltaTime);
		}

		private void AddLimbMovement(FollowingLimb limb)
		{
			limb.ragRigidBone.velocity = limb.velocity;
			limb.ragRigidBone.angularVelocity = limb.angularVelocity;
			limb.ragTransBone.rotation = limb.limitRotation * limb.ragTransBone.rotation;
		}

		private void CloneSkeletonRec(Transform tr, Transform newTr)
		{
			_tranToSkelet.Add(tr, newTr);
			_tranToRagdoll.Add(newTr, tr);
			ReflectMatrix(tr, newTr);

			for (int i = 0; i < tr.childCount; i++)
			{
				var child = tr.GetChild(i);
				var newChild = new GameObject(child.name).transform;

				newChild.SetParent(newTr, false);

				CloneSkeletonRec(child, newChild);
			}
		}

		private static void ReflectMatrix(Transform from, Transform to)
		{
			to.localPosition = from.localPosition;
			to.localRotation = from.localRotation;
			to.localScale = from.localScale;
		}
	}
}
