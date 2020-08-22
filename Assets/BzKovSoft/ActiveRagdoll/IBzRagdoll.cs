using UnityEngine;

namespace BzKovSoft.ActiveRagdoll
{
	public delegate void Propertychanged();

	/// <summary>
	/// Active ragdoll main component inteface
	/// </summary>
	public interface IBzRagdoll
	{
		/// <summary>
		/// If character is ragdolled, the value is True. Otherwise False
		/// </summary>
		bool IsRagdolled { get; set; }

		/// <summary>
		/// Is ragdoll connected to animation
		/// </summary>
		bool IsConnected { get; set; }

		/// <summary>
		/// The force the body limbs is trying to get its correct position. Could be from 0 to 1.
		/// </summary>
		float SpringRate { get;  set; }

		/// <summary>
		/// Main rigid component defined in inspector property
		/// </summary>
		Rigidbody RagdollRigid { get; }

		/// <summary>
		/// fired each time whenever IsConnected property changed
		/// </summary>
		/// <remarks>Do not forget to unsubscribe! )</remarks>
		event Propertychanged IsConnectedChanged;

		/// <summary>
		/// fired each time whenever IsRagdolled property changed
		/// </summary>
		/// <remarks>Do not forget to unsubscribe! )</remarks>
		event Propertychanged IsRagdolledChanged;

		/// <summary>
		/// Get ragdoll body limb by skeleton limb
		/// </summary>
		Transform GetRagdollTransform(Transform skeletonTransform);

		/// <summary>
		/// Get skeleton body limb by ragdoll limb
		/// </summary>
		Transform GetSkeletonTransform(Transform ragdollTransform);

		/// <summary>
		/// If you roughly changed the animation and do not want the ragdoll to do the rough movement, call it.
		/// </summary>
		void ResetLimbs();
	}
}

