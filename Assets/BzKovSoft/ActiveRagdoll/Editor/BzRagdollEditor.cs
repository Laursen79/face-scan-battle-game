using UnityEditor;
using UnityEngine;

namespace BzKovSoft.ActiveRagdoll.Editor
{
	[CustomEditor(typeof(BzRagdoll))]
	class BzRagdollEditor : UnityEditor.Editor
	{
		BzRagdoll _ragdoll;
		//SerializedProperty lookAtPoint;

		void OnEnable()
		{
			_ragdoll = (BzRagdoll)target;
			//lookAtPoint = serializedObject.FindProperty("lookAtPoint");
		}

		public override void OnInspectorGUI()
		{
			if (EditorApplication.isPlaying)
			{
				ButtonsBefore();
			}

			base.OnInspectorGUI();
			//serializedObject.Update();
			//EditorGUILayout.PropertyField(lookAtPoint);
			//serializedObject.ApplyModifiedProperties();

			if (EditorApplication.isPlaying)
			{
				ButtonsAfter();
			}
		}

		private void ButtonsBefore()
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("IsRagdolled"))
			{
				_ragdoll.IsRagdolled = !_ragdoll.IsRagdolled;
			}
			GUILayout.Label(_ragdoll.IsRagdolled.ToString(), GUILayout.Width(100f));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("IsConnected"))
			{
				_ragdoll.IsConnected = !_ragdoll.IsConnected;
			}
			GUILayout.Label(_ragdoll.IsConnected.ToString(), GUILayout.Width(100f));
			GUILayout.EndHorizontal();
		}

		private void ButtonsAfter()
		{
			if (GUILayout.Button("Apply Properties"))
			{
				_ragdoll.ApplyModifiedProperties();
			}
		}
	}
}
