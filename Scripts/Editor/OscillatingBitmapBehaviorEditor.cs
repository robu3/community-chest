using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(OscillatingBitmapBehavior))]
public class OscillatingBitmapBehaviorEditor : Editor {
	public override void OnInspectorGUI ()
	{
		OscillatingBitmapBehavior ob = target as OscillatingBitmapBehavior;
		DrawDefaultInspector();

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Add Part"))
		{
			ob.AddPart(Vector3.up * .1f, .3f, 0f);
		}
		GUILayout.EndHorizontal();
	}
}
