using System.Collections;
using UnityEngine;

namespace BzKovSoft.ActiveRagdoll.Samples
{
	public class Bullet : MonoBehaviour
	{
		void Start()
		{
			StartCoroutine(Die());
		}

		private IEnumerator Die()
		{
			yield return new WaitForSeconds(100f);
			Destroy(gameObject);
		}
	}
}