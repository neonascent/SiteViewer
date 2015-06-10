using UnityEngine;
using System.Collections;
using System.IO;
using JsonFx.Json;
using System.Collections.Generic;
using Reconstruction;
using UnityEditor;

public class SaveReconstruction  : EditorWindow {
	
	public string guid = "";
	public enum formatType { JPG, PNG };
	public formatType format = formatType.JPG;
	public string reconstructionLocation = "https://s3.amazonaws.com/tactical-space/viewer/";
	GameObject root = null;
	public string exportPath = "";
	public string output = "";

	string[] shaderMap = 	new string[] 		{	"_MainTex",	 	"_BumpMap", 	"_ParallaxMap"};


	// Add menu item named "My Window" to the Window menu
	[MenuItem("TacticalSpace/Create recon index file")]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow e = EditorWindow.GetWindow(typeof(SaveReconstruction));
		e.title = "Create recon index file";
	}


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI () {

		GUILayout.Label ("Object Root", EditorStyles.boldLabel);

		if (root = (GameObject)EditorGUILayout.ObjectField (root, typeof(GameObject), true)) {
			if ((root != null) && (root.transform.childCount > 0)) {
				//output = root.transform.childCount.ToString() + " parts found\r\n";
				guid = root.name;
				if (root.transform.GetChild(0).gameObject.transform.childCount > 0) {
					GameObject objectInsides = root.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
					exportPath = getModelFileLocation( objectInsides);
				} else {
					//output = "Valid part not found\r\n";
				}
			}
		}

		guid = EditorGUILayout.TextField ("Recon name", guid);
		reconstructionLocation = EditorGUILayout.TextField ("Remote Location", reconstructionLocation);
		exportPath = EditorGUILayout.TextField ("Output Location", exportPath);

		EditorGUI.BeginDisabledGroup ((root == null) || (exportPath == "") || (guid == ""));
		if( GUILayout.Button("Create index")) {
			ExportRecon();
		}
		EditorGUI.EndDisabledGroup ();

		EditorGUILayout.TextArea (output, GUILayout.Height (100));

	}


	string getModelFileLocation (GameObject o) {
		string objectPath = "";
		// get model location
		MeshFilter mf = o.GetComponent<MeshFilter>();
		if (mf != null) {
			Mesh sm = mf.sharedMesh;
			objectPath = Path.GetDirectoryName (AssetDatabase.GetAssetPath (sm));
		}
		if (objectPath == "") output = "Valid part not found\r\n";
		return objectPath;

	}

	
	void ExportRecon() {

		
		// create object
		Reconstruction.Reconstruction exportObject = new Reconstruction.Reconstruction();
		exportObject.guid = guid;
		exportObject.remoteURL = reconstructionLocation;
		exportObject.model_count = root.transform.childCount;
		
		// positioning
		RPosition rp = new RPosition ();
		rp.x = root.transform.localPosition.x;
		rp.y = root.transform.localPosition.y;
		rp.z = root.transform.localPosition.z;
		exportObject.position = rp;

		ROrientation ro = new ROrientation ();
		ro.qx = root.transform.localRotation.x;
		ro.qy = root.transform.localRotation.y;
		ro.qz = root.transform.localRotation.z;
		ro.qw = root.transform.localRotation.w;
		exportObject.rotation = ro;

		RScale rs = new RScale ();
		rs.sx = root.transform.localScale.x;
		rs.sy = root.transform.localScale.y;
		rs.sz = root.transform.localScale.z;
		exportObject.scale = rs;
		
		
		// add each model

		// go through each object....
		for (int i = 0; i < root.transform.childCount;i++)
		{
			RModel rm = new RModel();
			GameObject child = root.transform.GetChild(i).gameObject;
			rm.model = child.name;
			GameObject objectInsides = child.transform.GetChild(0).gameObject;

			// get shader
			// now apply to object
			Renderer r = objectInsides.GetComponent<Renderer>();
			Material objectMaterial;

			if (r != null) {
				objectMaterial = r.material;

				// get shader name
				rm.ShaderName = objectMaterial.shader.name;

				// get height
				if (objectMaterial.HasProperty ("_Parallax")) rm.parallax = objectMaterial.GetFloat ("_Parallax");

				// list all textures
				// set texture
				// let's work through image types
				foreach (string tName in shaderMap) {
					// set texture
					if (objectMaterial.HasProperty(tName)) {
						Texture t = objectMaterial.GetTexture(tName);
						ShaderTexture st = new ShaderTexture();
						st.name = tName;
						st.texture = t.name;

//						//Debug.Log ("has property: " + shaderMap[i]);
//							setTextureImportSettings(fp_base, maxSizes[i], imageFormats[i], imageTypes[i] == "Normal");
//							Texture t = (Texture)AssetDatabase.LoadAssetAtPath(fp_base, typeof(Texture));
//							objectMaterial.SetTexture(shaderMap[i], t);
					}
					
				}


			}



			exportObject.models.Add(rm);
			
		}

		if (writeIndexFile (exportPath, exportObject)) {
			output = "File saved as " + exportPath + "/reconstruction.json";
		} else {
			output = "Problem writing file " + exportPath + "/reconstruction.json";
		}

	}



//	void setMaterial(GameObject obj, string objectName, string path) {
//		
//		string[] pathContents = Directory.GetFiles(path);
//		Directory.CreateDirectory(path+"\\Materials");
//		string[] materials = Directory.GetFiles(path+"\\Materials");
//		
//		Material objectMaterial;
//		// check for material
//		if (materials.Contains(path+"\\Materials\\"+objectName+".mat")) {
//			objectMaterial = (Material)AssetDatabase.LoadAssetAtPath(path+"\\Materials\\"+objectName+".mat", typeof(Material));
//			objectMaterial.shader = selectedShader;
//			Debug.Log ("exists "+path+"\\Materials\\"+objectName+".mat");
//		} else {
//			// generate another one
//			objectMaterial = new Material( selectedShader );
//			objectMaterial.name = objectName;
//			AssetDatabase.CreateAsset(objectMaterial, path+"\\Materials\\"+objectName+".mat");
//			Debug.Log ("created "+path+"\\Materials\\"+objectName+".mat");
//		}
//		
//		// let's work through image types
//		for (int i = 0; i < imageTypes.Count(); i++) {
//			string fp_base = path+"\\"+objectName+imageFiles[i];
//			if (pathContents.Contains(fp_base)) {
//				// set texture
//				if (objectMaterial.HasProperty(shaderMap[i])) {
//					//Debug.Log ("has property: " + shaderMap[i]);
//					setTextureImportSettings(fp_base, maxSizes[i], imageFormats[i], imageTypes[i] == "Normal");
//					Texture t = (Texture)AssetDatabase.LoadAssetAtPath(fp_base, typeof(Texture));
//					objectMaterial.SetTexture(shaderMap[i], t);
//				}
//				
//			}
//			
//		}
//		
//		// set height
//		if (objectMaterial.HasProperty ("_Parallax"))
//			objectMaterial.SetFloat ("_Parallax", parallax);
//		
//		// now apply to object
//		Renderer r = obj.GetComponent<Renderer>();
//		
//		if (r != null) {
//			r.material = objectMaterial;
//		}
//
//	}





	bool writeIndexFile(string path, Reconstruction.Reconstruction r) {

		// if directory doesn't exist then create it
		if (!Directory.Exists(path)) {
			Directory.CreateDirectory(path); 
		}	
		
		
		string exportTarget = path + "/reconstruction.json";
		
		if (File.Exists(exportTarget)) {
			File.Delete(exportTarget);
		}	
		
		string str = JsonWriter.Serialize(r);
		File.WriteAllText(exportTarget, str); 

		// file created?
		return File.Exists (exportTarget);

	}

}
