namespace BzKovSoft.ActiveRagdoll.RagdollBehaviours
{
	/// <summary>
	/// Active ragdoll event handler
	/// </summary>
	public interface IRagdollBehaviour
	{
		void OnIsRagdolledChanged(bool newValue);
		void OnIsConnectedChanged(bool newValue);
	}
}

