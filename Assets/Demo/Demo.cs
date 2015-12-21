using UnityEngine;
using System.Collections;
using System;

public class Demo : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		

	}

	private string text = "http://www.google.com";

	void OnGUI() {
		text = GUILayout.TextField(text, GUILayout.Width(300));
		if(GUILayout.Button("GET")) {
			Job.Create(
				June.Util.ExecuteGetCommand(
					"http://www.google.com", 
					www => Debug.Log(www.text)));
		}
			
		GUILayout.Label("Test Config: ", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
		GUILayout.Label(GetConfigString(), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
	}

	/// <summary>
	/// Gets the config string.
	/// </summary>
	/// <returns>The config string.</returns>
	string GetConfigString() {
		if(null != TestConfigManager.Instance.Items) {
			return string.Join(
				Environment.NewLine, 
				TestConfigManager.Instance.Items.ConvertAll<string>(i => i.ToString()).ToArray());
		}
		return "<null>";
	}
}
