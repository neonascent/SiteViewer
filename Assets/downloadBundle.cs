using UnityEngine;
using System.Collections;
using System.IO;

public class downloadBundle : MonoBehaviour {

	public string guid = "cafe";
	public string bundleLocation = "http://herein.tacticalspace.org/reconstructions/";


	public Texture[] frames;
	public int framesPerSecond = 10;
	private Texture loadingMovie;

	private string bundleURL = null;
	private string bundleFilePath = ""; 
	private string AssetPath = ""; 
	
	public string message = "started";


	// Use this for initialization
	IEnumerator Start () {
		// start loading video
		if (frames.Length > 0) {
			loadingMovie = frames[0];
		}

		// check for json file
		AssetPath	= Application.persistentDataPath + "/" + guid + "/";
		bundleFilePath = AssetPath + guid;
		bundleURL = bundleLocation + guid;
		if (!File.Exists(bundleFilePath)) {
			// if directory doesn't exist then create it
			if (!Directory.Exists(Application.persistentDataPath + "/" + guid)) {
				Directory.CreateDirectory(Application.persistentDataPath + "/" + guid); 
			}	
			
			yield return StartCoroutine(downloadBundleFile(bundleURL));
			Debug.Log("Download done");
			if (File.Exists(bundleFilePath)) {
				// start loader and destroy this
				Application.LoadLevel("Main");
			}
		} else {
			message = "Bundle already downloaded and at " + bundleFilePath;
			Debug.Log("Bundle already downloaded and at " + bundleFilePath);
			Application.LoadLevel("Main");
		}	
		

	}

	// Download JSON
	IEnumerator downloadBundleFile (string url) {
		WWW www = new WWW(url);
		while( !www.isDone ) {
			message = "Downloading "+ guid + " - " + string.Format("{0:0}", (www.progress * 100))+"%";
			yield return null;
		}
		if (!string.IsNullOrEmpty(www.error)) {
			// error!
			message = www.error + "  " + url;
		} else {
			// success!
			// save bundle locally
			if (www.bytes.Length > 0) {
				File.WriteAllBytes(bundleFilePath, www.bytes);
				message = "Bundle downloaded, and saved to " + bundleFilePath;
			}
		}
		//yield return www;
		
	}


	void OnGUI () {
		int multiplier = (Screen.currentResolution.width > 960) ? 2 : 1;
		// Make a background box
		int boxWidth = 530 * multiplier;
		int boxHeight = 90 * multiplier;
		int boxTop = (Screen.height - boxHeight) / 2;
		Rect guiBox = new Rect ((Screen.width - boxWidth) / 2, boxTop, boxWidth, boxHeight);
		GUIStyle bottom = GUI.skin.GetStyle("Label");
		bottom.alignment = TextAnchor.LowerCenter;
		
		if (loadingMovie != null) {
			
			GUI.DrawTexture (guiBox, loadingMovie, ScaleMode.ScaleToFit);
			//GUI.Label (guiBox, message, bottom);
			GUI.Box (guiBox, message);
		} else {
			GUI.Box (guiBox, message);
		}

		int buttonWidth = 100 * multiplier;
		int buttonHeight = 20 * multiplier;
		//GUI.TextField(new Rect(20,35, 490,20), Application.persistentDataPath.ToString());
		
		if (GUI.Button(new Rect((Screen.width-buttonWidth)/2,20 + boxTop + 10 + boxHeight + 10,buttonWidth,buttonHeight), "Cancel")) {
			//Application.Quit();
			Application.LoadLevel("Menu");
		}
	}
	




	// Update is called once per frame
	void Update () {
		int index  = Mathf.FloorToInt((Time.time * framesPerSecond) % frames.Length);
		loadingMovie = frames[index];
	}
}