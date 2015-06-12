using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

namespace Tools {
	public class DownloadTools:MonoBehaviour
	{

		public string message = "";
		public Texture[] frames;
		public int framesPerSecond = 10;
		private Texture loadingMovie;

		// Download JSON
		public IEnumerator openOrDownloadFile (string localPath, string remotePath, string filename, System.Action<WWW> result) { // ref string message,
			// check is local

			if (!File.Exists (localPath + filename)) {
				Debug.Log ("downloading " + remotePath + filename);
				message = "downloading " + remotePath + filename;
				yield return StartCoroutine(startDownload(remotePath + filename, localPath + filename, filename));
				//while 
				Debug.Log ("Returned");
			}

			if (File.Exists (localPath + filename)) {
				// return local version
				Debug.Log ("Opening local file " +  filename);
				message = "Opening local file " +  filename;
				WWW www = new WWW("file:///"+localPath + filename);
				yield return www;
				if (!string.IsNullOrEmpty (www.error)) {
					Debug.Log (www.error);
					//message = www.error;
					result(null);
					yield break;
				}
				message = "";
				result(www);

			}
		}

		public void Start () {
			object[] textures = (object[]) Resources.LoadAll("loading", typeof(Texture));//frames = (Texture2D[])Resources.LoadAll ("loading", typeof(Texture2D));
			frames = new Texture[textures.Length];
			for (int i = 0; i < textures.Length; i++) {
				frames[i] = (Texture)textures[i];
			}
			// start loading video
			if (frames.Length > 0) {
				loadingMovie = frames [0];
			}
		}

		string getFileSize(double len) {
			string[] sizes = { "B", "KB", "MB", "GB" };
			int order = 0;
			while (len >= 1024 && order + 1 < sizes.Length) {
				order++;
				len = len/1024;
			}
			
			// Adjust the format string to your preferences. For example "{0:0.#}{1}" would
			// show a single decimal place, and no space.
			return string.Format("{0:0.##} {1}", len, sizes[order]);
			
		}

		public string GetPlatform() {
			switch(Application.platform) 
			{
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.WindowsEditor:
				return "PC";
			case RuntimePlatform.Android:
				return "Android";
			case RuntimePlatform.OSXWebPlayer:
			case RuntimePlatform.WindowsWebPlayer:
				return "Web";
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.OSXEditor:
				return "OSX";
			case RuntimePlatform.IPhonePlayer:
				return "iOS";
			default:
				return "(unknown platform)";
				break;
			}
		}

		// Use this for initialization
		IEnumerator startDownload(string url, string targetLocation, string shortName) { // ref string message
			message = "downloading " + url;
			Debug.Log ("downloading " + url);

			Uri myUri = new Uri(url);   
			string host = myUri.Host;
			string uri = myUri.LocalPath;

			uint contentLength;
			int n = 0;
			int read = 0;

			NetworkStream networkStream; 
			FileStream fileStream;
			Socket client;
			
			
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
			
			fileStream = new FileStream( targetLocation+"_temp", FileMode.Create);
			

			while (true) {

				byte[] buffer = new byte[256 * 1024];
				
				if (n < contentLength) {
					if (networkStream.DataAvailable) {
						read = networkStream.Read (buffer, 0, buffer.Length);
						n += read;
						fileStream.Write (buffer, 0, read);
					}
					Debug.Log ("Downloaded: " + getFileSize ((double)n) + " of " + getFileSize ((double)contentLength) + " bytes ...");
					message = "Downloading " + shortName + " - " + getFileSize ((double)n) + " of " + getFileSize ((double)contentLength) + " bytes";
					yield return null;
				} else {
					fileStream.Flush ();
					fileStream.Close ();
					
					client.Close ();
					Debug.Log ("Download done");
					if (File.Exists (targetLocation+"_temp")) {
						// start loader and destroy this
						File.Move(targetLocation+"_temp", targetLocation);
						return true;
					}
				}
			}
			
		}

		public void loadingScreen() {
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

				loadingMovie = frames[Mathf.FloorToInt((Time.time * framesPerSecond) % frames.Length)];
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


	}
}