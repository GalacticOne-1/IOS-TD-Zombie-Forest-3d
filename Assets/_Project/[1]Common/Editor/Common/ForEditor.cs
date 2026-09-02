using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ForEditor : EditorWindow {

	#if UNITY_EDITOR
	
	
	[MenuItem("Add/Clear Data")]
	public static void Clear(){

		PlayerPrefs.DeleteAll();
	}
	
	#endif
}
