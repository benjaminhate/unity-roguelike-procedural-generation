using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GenerateLevel))]
public class DungeonGeneratorEditor : Editor {

	void OnSceneGUI(){
		GenerateLevel generator = (GenerateLevel)target;

		Handles.color = Color.red;

		int half_tile = Mathf.FloorToInt ((float)generator.tile_size / 2);

		long width = generator.tile_size * generator.width + half_tile;
		long height = generator.tile_size * generator.height + half_tile;

		Handles.DrawLine (new Vector3 (-half_tile, -half_tile, 0), new Vector3 (-half_tile, height, 0));
		Handles.DrawLine (new Vector3 (-half_tile, -half_tile, 0), new Vector3 (width, -half_tile, 0));
		Handles.DrawLine (new Vector3 (width, -half_tile, 0), new Vector3 (width, height, 0));
		Handles.DrawLine (new Vector3 (-half_tile, height, 0), new Vector3 (width, height, 0));
	}

	public override void OnInspectorGUI(){
		base.OnInspectorGUI ();

		GenerateLevel generator = (GenerateLevel)target;

		GUILayout.BeginHorizontal ();

		if (GUILayout.Button ("Generate")) {
			generator.ResetDungeonVoid (true);
		}

		if (GUILayout.Button ("Clear")) {
			generator.ClearDungeon (true);
		}

		GUILayout.EndHorizontal ();
	}

}
