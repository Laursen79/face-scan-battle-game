using System.Collections;
using UnityEngine;

namespace BzKovSoft.ActiveRagdoll.Samples
{
	public class Shotgun : MonoBehaviour
	{
		[SerializeField]
		public GameObject _bullet;
		[SerializeField]
		public Vector3 _velocity;

		// Start is called before the first frame update
		void Start()
		{
			StartCoroutine(Fire());
		}

		private IEnumerator Fire()
		{
			for (; ; )
			{
				yield return new WaitForSeconds(2f);
				var bullet = Instantiate(_bullet);
				bullet.transform.position = transform.position;
				var rigid = bullet.GetComponent<Rigidbody>();
				rigid.velocity = _velocity;
			}
		}

		// Update is called once per frame
		void Update()
		{

		}
	}
}