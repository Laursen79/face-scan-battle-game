using UnityEngine;

namespace BzKovSoft.ActiveRagdoll
{
	/// <summary>
	/// Character balance controller
	/// </summary>
	public interface IBzBalancer
	{
		/// <summary>
		/// Velocity you need to apply to your character to find balance (needed for realistic purpose)
		/// </summary>
		Vector3 BalanceVelocity { get; }
		/// <summary>
		/// Difference between ragdolled skeleton and animated
		/// </summary>
		float PoseError { get; }
	}
}
