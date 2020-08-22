using System.Collections.Generic;
using UnityEngine;

namespace BzKovSoft.ActiveRagdoll
{
	public class SkeletonDrawer : MonoBehaviour
	{
		Color _lineColor = Color.green;
		List<LineFromTo> _lines;

		public Color Color
		{
			get { return _lineColor; }
			set { _lineColor = value; }
		}

		void OnEnable()
		{
			_lines = new List<LineFromTo>();

			GetLines(transform);
		}

		void OnDisable()
		{
			_lines = null;
		}

		private void GetLines(Transform transform)
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				var child = transform.GetChild(i);
				_lines.Add(new LineFromTo { from = transform, to = child });
				GetLines(child);
			}
		}

		void LateUpdate()
		{
			for (int i = 0; i < _lines.Count; i++)
			{
				var line = _lines[i];
				Debug.DrawLine(line.from.position, line.to.position, _lineColor);
			}
		}

		struct LineFromTo
		{
			public Transform from;
			public Transform to;
		}
	}
}