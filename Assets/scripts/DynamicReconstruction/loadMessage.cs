using UnityEngine;
using System.Collections;
using System.IO;

public class loadMessage : MonoBehaviour {

	public string message;

	public string guid;
	public string localURL;
	public Texture[] frames;
	public int framesPerSecond = 10;

	private Texture loadingMovie;
	void Start() {
		guid = PlayerPrefs.GetString("guid");
		string localBundlePath = Application.persistentDataPath + "/" + guid + "/" + guid;
		localURL = "file:///" + localBundlePath;
		if (File.Exists (localBundlePath)) {
			message = "Loading " + guid + "\r\n" + getFileSize (localBundlePath);
			// start loading video
			if (frames.Length > 0) {
				loadingMovie = frames[0];
			}
			StartCoroutine(loadBundleObject());
		} else {
			message = "Can't find " + localBundlePath;
		}
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

	string getFileSize(string filename) {
		string[] sizes = { "B", "KB", "MB", "GB" };
		double len = new FileInfo(filename).Length;
		int order = 0;
		while (len >= 1024 && order + 1 < sizes.Length) {
			order++;
			len = len/1024;
		}
		
		// Adjust the format string to your preferences. For example "{0:0.#}{1}" would
		// show a single decimal place, and no space.
		return string.Format("{0:0.##} {1}", len, sizes[order]);

	}


	IEnumerator loadBundleObject() {


		Debug.Log ("loading");

		using(WWW www = new WWW(localURL))
		{
			yield return www;
			if(!string.IsNullOrEmpty(www.error))
			{
				Debug.Log(www.error);
				message = www.error;
				yield break;
			}
			Instantiate(www.assetBundle.LoadAsset(guid));
			Debug.Log ("done");
			yield return null;
			
			www.assetBundle.Unload(false);
			this.enabled = false;
		}




	}

	// Update is called once per frame
	void Update () {
		int index  = Mathf.FloorToInt((Time.time * framesPerSecond) % frames.Length);
		loadingMovie = frames[index];
	}
}
