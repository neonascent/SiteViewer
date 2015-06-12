using UnityEngine;
using System.Collections;
using System.IO;
using Tools;

public class loadMessage : MonoBehaviour {

	public string message;

	public string guid;
	public string remoteURL;
	public string platform;

	DownloadTools dt;
	
	IEnumerator Start() {
		guid = PlayerPrefs.GetString("guid");
		remoteURL = PlayerPrefs.GetString("url");
		// create download helper
		dt = gameObject.AddComponent<DownloadTools>() as DownloadTools;
		dt.message = "loading parts for "+ guid;

		
		// get platform name 
		platform = dt.GetPlatform ();

		// try to get manifest
		WWW www = null;
		
		yield return StartCoroutine (dt.openOrDownloadFile (Application.persistentDataPath + "/" + guid + "/", remoteURL, platform, value => www = value)); //(Application.persistentDataPath + "/" + guid + "/", remoteLocation, platform, value => www = value));
		if (www != null) {
			AssetBundleManifest manifest = (AssetBundleManifest)www.assetBundle.LoadAsset ("AssetBundleManifest");
			www.assetBundle.Unload (false);
		
			foreach (string bundle in manifest.GetAllAssetBundles()) {
				Debug.Log (bundle);
				// load bundle
				WWW bundleWWW = null; 
				yield return StartCoroutine (dt.openOrDownloadFile (Application.persistentDataPath + "/" + guid + "/", remoteURL, bundle, value => bundleWWW = value)); //(Application.persistentDataPath + "/" + guid + "/", remoteLocation, platform, value => www = value));
				if (bundleWWW != null) {
					AssetBundle ab = bundleWWW.assetBundle;
					bundleWWW = null;
					// Load the object asynchronously
					AssetBundleRequest request = ab.LoadAssetAsync (bundle);
					
					// Wait for completion
					yield return request;

					Instantiate(request.asset);
					Debug.Log (bundle + " loaded");
					
					ab.Unload(false);
				} else {
					Debug.Log ("Problem accessing bundle: " +bundle);
					dt.message = "Problem accessing bundle: " +bundle;
				}

				
			}
		
		
			Debug.Log ("yay");
			dt.message = "yay";
			this.enabled = false;
		} else {
			Debug.Log ("Problem accessing site manifest");
			dt.message = "Problem accessing site manifest";
		}
	


	
	}
	

	void OnGUI () {
		dt.loadingScreen ();
	}




}
