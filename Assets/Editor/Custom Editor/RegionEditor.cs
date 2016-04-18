using UnityEngine;
using System.Collections;
using UnityEditor;

public class RegionEditor : EditorWindow {

	ProceduralSphere.Region[] Regions;
	ProceduralSphere.Region Selected;


	public static void Init (ProceduralSphere.Region[] regions) {
		
		RegionEditor window = (RegionEditor)EditorWindow.GetWindow (typeof (RegionEditor));
		window.Regions = regions;
		window.minSize = new Vector2(250,300);
		window.Show();
	}

	void OnGUI () {
		for (int i = 0; i < Regions.Length; i++) {
			Texture2D tex = new Texture2D (100, (int)(250*Regions[i].height));
			for (int y = 0; y < tex.height; y++) {
				for (int x = 0; x < tex.width; x++) {
					tex.SetPixel (x, y, Regions[i].color);
				}
			}
			tex.Apply ();
			GUI.backgroundColor = Color.clear;
			if (GUILayout.Button (tex, GUILayout.Width(tex.width), GUILayout.Height(tex.height))) {
				Selected = Regions [i];
			}
		}
		if (Selected != null) {
			GUILayout.Label ("Wuuhuu");
		}
	}
}