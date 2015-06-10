using UnityEngine;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Text;
using System;

public class downloadBundle : MonoBehaviour {

	public string guid = "cafe";
	public string bundleLocation = "http://herein.tacticalspace.org/reconstructions/";


	public Texture[] frames;
	public int framesPerSecond = 10;
	private Texture loadingMovie;

	private string bundleURL = null;
	private string bundleFilePath = ""; 
	private string AssetPath = ""; 

	uint contentLength;
	int n = 0;
	int read = 0;
	
	
	NetworkStream networkStream; 
	FileStream fileStream;
	Socket client;
	
	public string message = "started";


	// Use this for initialization
	void Start () {
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

			Uri myUri = new Uri(bundleURL);   
			string host = myUri.Host;
			string uri = myUri.LocalPath;

			// start downloading
			startDownload(host, uri, bundleFilePath);
		
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

	// Use this for initialization
	void startDownload(string host, string uri, string targetLocation) {
			string query = "GET " + uri.Replace(" ", "%20") + " HTTP/1.1\r\n" +
				"Host: " + host + "\r\n" +
					"User-Agent: undefined\r\n" +
					"Connection: close\r\n"+
					"\r\n";
			
			
			Debug.Log (query);
			
			client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
			client.Connect(host, 80);   
			
			networkStream = new NetworkStream(client);
			
			var bytes = Encoding.Default.GetBytes(query);
			networkStream.Write(bytes, 0, bytes.Length);
			
			var bReader = new BinaryReader(networkStream, Encoding.Default);
			
			string response = "";
			string line;
			char c;
			
			do 
			{
				line = "";
				c = '\u0000';
				while (true) 
				{
					c = bReader.ReadChar();
					if (c == '\r')
						break;
					line += c;
				}
				c = bReader.ReadChar();
				response += line + "\r\n";
			} 
			while (line.Length > 0);  
			
			Debug.Log ( response );
			
			Regex reContentLength = new Regex(@"(?<=Content-Length:\s)\d+", RegexOptions.IgnoreCase);
			contentLength = uint.Parse(reContentLength.Match(response).Value);
			
			fileStream = new FileStream( targetLocation, FileMode.Create);
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

		byte[] buffer = new byte[256 * 1024];
		
		if (n < contentLength) 
		{
			if (networkStream.DataAvailable) 
			{
				read = networkStream.Read(buffer, 0, buffer.Length);
				n += read;
				fileStream.Write(buffer, 0, read);
			}
			Debug.Log ( "Downloaded: " + n + " of " + contentLength + " bytes ..." );
			message = "Downloading "+ guid + " - " + n + " of " + contentLength + " bytes";
		}
		else
		{
			fileStream.Flush();
			fileStream.Close();
			
			client.Close();
			Debug.Log("Download done");
			if (File.Exists(bundleFilePath)) {
				// start loader and destroy this
				Application.LoadLevel("Main");
			}
		}
	}

}




// Based on this:
// http://stackoverflow.com/questions/4768443/downloading-file-via-tcpclient-from-http-server

