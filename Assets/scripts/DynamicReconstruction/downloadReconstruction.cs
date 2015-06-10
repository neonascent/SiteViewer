using UnityEngine;
using System.Collections;
using System.IO;
using JsonFx.Json;
using System.Collections.Generic;
using Reconstruction;


public class downloadReconstruction : MonoBehaviour {
	
	public string guid = "cafe";
	public string reconstructionLocation = "http://herein.tacticalspace.org/reconstructions/";
	
	private string jsonURL = null;
	private string JSONFilePath = ""; 
	private string AssetPath = ""; 
	
	public string message = "started";
	
	
	
	
	IEnumerator Start() {
		
		// check for json file
		AssetPath	= Application.persistentDataPath + "/" + guid + "/";
		JSONFilePath = AssetPath + "reconstruction.json";
		jsonURL = reconstructionLocation + guid + "/reconstruction.json";
		if (!File.Exists(JSONFilePath)) {
			// if directory doesn't exist then create it
			if (!Directory.Exists(Application.persistentDataPath + "/" + guid)) {
				Directory.CreateDirectory(Application.persistentDataPath + "/" + guid); 
			}	
			
			yield return StartCoroutine(downloadJSON(jsonURL));
			
		} else {
			message = "JSON already downloaded and at " + JSONFilePath;
			Debug.Log("JSON already downloaded and at " + JSONFilePath);
		}	
		
		if (File.Exists(JSONFilePath)) {
			// start loader and destroy this
			GetComponent<loadMessage>().enabled = true;
			Destroy(this, 0);
		}
	}
	
	
	
	// Download JSON
	IEnumerator downloadJSON (string url) {
		
        WWW www = new WWW(url);
        yield return www;
		
		if (www.error != null) {
			message = www.error;
		} else {
		    
			// convert to local format and save
			
			Reconstruction.Reconstruction r = JsonReader.Deserialize<Reconstruction.Reconstruction>(www.text);
			
			// download images
			for (int i = 0; i < r.model_count; i++) {
				RModel m = r.models[i];
				Debug.Log("Downloading Image Index: " + (i+1).ToString() + " of " + r.model_count.ToString() + "  URL: " + r.remoteURL + m.model + ".obj");
				message = "Downloading Images (" +  (i+1).ToString() + " of " + r.model_count.ToString() + ")";
				// download images and name correctly 
				yield return StartCoroutine(downloadFile(r.remoteURL, m.model + ".obj"));
			}
			
			// save original
			File.WriteAllText(JSONFilePath, www.text );  
			message = "JSON downloaded, parsed, and saved to " + JSONFilePath;
		}	
	
	}
	
	
	IEnumerator downloadFile(string remotePath, string filename) {
		while (!File.Exists(AssetPath + filename)) {
			Debug.Log(remotePath+filename);
			WWW www = new WWW(remotePath+filename);
        	yield return www;
			if (www.bytes.Length > 0) 
				File.WriteAllBytes(AssetPath + filename, www.bytes);
		}
	}
	
	void OnGUI () {
		// Make a background box
		int multiplier =  (Screen.currentResolution.width > 960)?2:1;
		
		int boxWidth = 530 * multiplier;
		int boxHeight = 90 * multiplier;
		int boxTop = (Screen.height-boxHeight)/2;
		Rect guiBox = new Rect((Screen.width-boxWidth)/2, boxTop, boxWidth, boxHeight);
		GUI.Box(guiBox, "Downloading");
		
		int titleWidth = 490 * multiplier;
		int titleHeight = 20 * multiplier;
		
		GUI.TextField(new Rect((Screen.width-titleWidth)/2, 20 + boxTop + 10, titleWidth,titleHeight), message);
		
		int buttonWidth = 100 * multiplier;
		int buttonHeight = 20 * multiplier;
		//GUI.TextField(new Rect(20,35, 490,20), Application.persistentDataPath.ToString());
		
		if (GUI.Button(new Rect((Screen.width-buttonWidth)/2,20 + boxTop + 10 + titleHeight + 10,buttonWidth,buttonHeight), "Cancel")) {
			//Application.Quit();
			Application.LoadLevel("Menu");
		}
	}
	
	
	// Update is called once per frame
	void Update () {
	
	}
	
	
	
}
