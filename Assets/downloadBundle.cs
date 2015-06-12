using UnityEngine;
using System.Collections;
using System.IO;
using System;
using Tools;

public class downloadBundle : MonoBehaviour {

	public string guid = "cafe";
	public string bundleLocation = "http://herein.tacticalspace.org/reconstructions/";
	public string platform = "PC";

	private string bundleURL = null;
	private string bundleFilePath = ""; 
	private string AssetPath = ""; 

	DownloadTools dt;

	// Use this for initialization
	void Start () {
		// create download helper
		dt = gameObject.AddComponent<DownloadTools>() as DownloadTools;
		dt.message = "started";

		// get platform name 
		platform = dt.GetPlatform ();
		string remoteLocation = "";
		// write "remote" file to local
		string targetFile	= Application.persistentDataPath + "/" + guid + "/remote.txt";
		if (!File.Exists (targetFile)) {
			Directory.CreateDirectory(Application.persistentDataPath + "/" + guid);

			// write remote URL to file
			remoteLocation = bundleLocation + guid + "/" + platform + "/";
			StreamWriter writer = new StreamWriter(targetFile);
			writer.WriteLine(remoteLocation);
			writer.Close();
			dt.message = "Initialising Site at " + targetFile;
			Debug.Log("Initialising Site at " + targetFile);
		} else {
			// read remote location
			StreamReader reader=new  StreamReader(targetFile);
			remoteLocation = reader.ReadLine().TrimEnd();
			dt.message = "Pointing to " + remoteLocation;
			Debug.Log("Pointing to " + remoteLocation);
		}
		PlayerPrefs.SetString("url", remoteLocation);
		// go to main part






		Application.LoadLevel("Main");


	
//		// check for manifest
//		AssetPath	= Application.persistentDataPath + "/" + guid + "/";
//		bundleFilePath = AssetPath + guid;
//		bundleURL = bundleLocation + guid;
//		if (!File.Exists(bundleFilePath)) {
//			// if directory doesn't exist then create it
//			if (!Directory.Exists(Application.persistentDataPath + "/" + guid)) {
//				Directory.CreateDirectory(Application.persistentDataPath + "/" + guid); 
//			}	
//
//
//
//			// start downloading
//			yield return StartCoroutine(startDownload(host, uri, bundleFilePath));
//		
//		} else {
//			message = "Bundle already downloaded and at " + bundleFilePath;
//			Debug.Log("Bundle already downloaded and at " + bundleFilePath);
//
//		}	
		

	}








	void OnGUI () {
		dt.loadingScreen ();
	}

}




// Based on this:
// http://stackoverflow.com/questions/4768443/downloading-file-via-tcpclient-from-http-server

