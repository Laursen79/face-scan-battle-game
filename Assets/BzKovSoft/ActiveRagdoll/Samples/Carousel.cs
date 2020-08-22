using UnityEngine;

namespace BzKovSoft.ActiveRagdoll.Samples
{
	public class Carousel : MonoBehaviour
	{
#pragma warning disable 0649
		[SerializeField]
		Vector3 _rotation;
#pragma warning restore 0649

		void Update()
		{
			transform.Rotate(_rotation * Time.deltaTime);
		}
	}
}