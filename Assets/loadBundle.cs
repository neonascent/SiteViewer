using UnityEngine;
using System.Collections;

public class loadBundle : MonoBehaviour {
	public string url = "https://s3.amazonaws.com/tactical-space/viewer/Bundles/PC/murujuga";
	public string objectName = "Box001";
	private bool loading = false;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.U)) {
			print("u key was pressed");
			StartCoroutine (loadAsset ());

		}
	}

	void OnGUI() {
		if (loading) {
			GUI.TextArea(new Rect(0,0,300,300), "loading");
		}
	}

	IEnumerator loadAsset() {
		Debug.Log ("loading");
		loading = true;
		using(WWW www = WWW.LoadFromCacheOrDownload(url, 1))
		{
			yield return www;
			if(!string.IsNullOrEmpty(www.error))
			{
				Debug.Log(www.error);
				yield break;
			}
			Instantiate(www.assetBundle.LoadAsset(objectName));
			Debug.Log ("done");
			loading = false;
			yield return null;
			
			www.assetBundle.Unload(false);
		}
	}
}
