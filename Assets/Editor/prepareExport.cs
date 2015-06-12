	//C# Example
	using UnityEditor;
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.IO;
	
	public class prepareExport : EditorWindow
	{
		GameObject root = null;
		
		// Add menu item named "My Window" to the Window menu
		[MenuItem("TacticalSpace/Export/Prepare for Bundle export", false, 10)]
		public static void ShowWindow()
		{
			//Show existing window instance. If one doesn't exist, make one.
			EditorWindow.GetWindow(typeof(prepareExport));
		}
		
		void OnGUI()
		{
			GUILayout.Label ("Object Root", EditorStyles.boldLabel);
			
			
			if (root = (GameObject)EditorGUILayout.ObjectField (root, typeof(GameObject), true)) {
				if (root != null) {
					// do something?
				}
			}


			GUILayout.Label ("Tools", EditorStyles.boldLabel);
			EditorGUI.BeginDisabledGroup ((root == null) || (root.transform.childCount < 1));
			
			if( GUILayout.Button("Propagate Local Positions")) {
				setPositions(root);
			}
			if( GUILayout.Button("Export Parts")) {
				preparePrefabs(root);
			}
			EditorGUI.EndDisabledGroup ();	

			


			
			

			
		}


		// move position from root to parts
		void setPositions(GameObject activeObj) {
			string objectName = "";
			
			Transform t = activeObj.transform;

			// go through each object....
			for (int i = 0; i < activeObj.transform.childCount;i++)
			{
				
				GameObject child = activeObj.transform.GetChild(i).gameObject;

				child.transform.localPosition = child.transform.localPosition + t.position;
			child.transform.localRotation =  Quaternion.Euler(t.localRotation.eulerAngles.x + child.transform.localRotation.eulerAngles.x, t.localRotation.eulerAngles.y+child.transform.localRotation.eulerAngles.y,child.transform.localRotation.eulerAngles.z + t.localRotation.eulerAngles.z);
				child.transform.localScale = new Vector3(child.transform.localScale.x * t.localScale.x, child.transform.localScale.y * t.localScale.y, child.transform.localScale.z * t.localScale.z);
				
			}
			
			t.position = Vector3.zero;
			t.rotation = Quaternion.identity;
			t.localScale = new Vector3 (1, 1, 1);

		}


	
	// move position from root to parts
	void preparePrefabs(GameObject activeObj) {
		// Create prefab output folder
		//AssetDatabase.DeleteAsset(prefabPath);
		if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) 	AssetDatabase.CreateFolder("Assets", "Prefabs");



		// Create the array of bundle build details.
		AssetBundleBuild[] buildMap = new AssetBundleBuild[activeObj.transform.childCount];

		string objectName = "";
		
		// go through each object....
		for (int i = 0; i < activeObj.transform.childCount;i++)
		{

			GameObject child = activeObj.transform.GetChild(i).gameObject;
			string prefabPath = "Assets/Prefabs/"+child.name+".prefab";
			// delete if it exists
			AssetDatabase.DeleteAsset(prefabPath);


			// Save the transform's GameObject as a prefab asset.
			PrefabUtility.CreatePrefab(prefabPath, child);

			// Save the transform's GameObject as a prefab asset.
			buildMap[i].assetBundleName = child.name;
			string[] assets = new string[1];
			assets[0] = prefabPath;
			buildMap[i].assetNames = assets;

		}

		// build bundles
		BuildAssetBundles (buildMap);



		
	}

	static void BuildAssetBundles(AssetBundleBuild[] bundles) 
	{
		// Bring up save panel
		string path = EditorUtility.SaveFolderPanel ("Save Bundles", "", "");
		if (path.Length != 0) 
		{   
			if(EditorPrefs.GetBool("buildPC", false)) 
				BuildBundle(path + "/PC", BuildTarget.StandaloneWindows,  bundles);
			
			if(EditorPrefs.GetBool("buildOSX", false))
				BuildBundle(path + "/OSX", BuildTarget.StandaloneOSXUniversal,  bundles);
			
			if(EditorPrefs.GetBool("buildWeb", false))
				BuildBundle(path + "/Web", BuildTarget.WebPlayer,  bundles);
			
			if(EditorPrefs.GetBool("buildiOS", false))
				BuildBundle(path + "/iOS", BuildTarget.iOS,  bundles);
			
			if(EditorPrefs.GetBool("buildAndroid", false))
				BuildBundle(path + "/Android", BuildTarget.Android,  bundles);          
		}
	}
	
	static void BuildBundle(string path, BuildTarget target, AssetBundleBuild[] bundles)
	{
		Debug.Log ("Building bundles for " + target.ToString ());
		if (!Directory.Exists (path))
			Directory.CreateDirectory (path);
		
		BuildPipeline.BuildAssetBundles (path, bundles, BuildAssetBundleOptions.None, target);
	}
		
		
		/*
		
		
		
		
		
		
		void setMaterials(GameObject activeObj) {
			string objectName = "";
			
			// go through each object....
			for (int i = 0; i < activeObj.transform.childCount;i++)
			{
				
				
				
				GameObject child = activeObj.transform.GetChild(i).gameObject;
				objectName = child.name;
				GameObject objectInsides = child.transform.GetChild(0).gameObject;
				
				// get model location
				MeshFilter mf = objectInsides.GetComponent<MeshFilter>();
				if (mf != null) {
					Mesh sm = mf.sharedMesh;
					string objectPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(sm));
					// Assets/Objects/tunnel-old/Floor012.obj
					// get containing folder...
					setMaterial (objectInsides, objectName, objectPath);
				}
				
			}
			
			
		}
		
		
		
		
		
		
		
		void setMaterial(GameObject obj, string objectName, string path) {
			
			string[] pathContents = Directory.GetFiles(path);
			Directory.CreateDirectory(path+"\\Materials");
			string[] materials = Directory.GetFiles(path+"\\Materials");
			
			Material objectMaterial;
			// check for material
			if (materials.Contains(path+"\\Materials\\"+objectName+".mat")) {
				objectMaterial = (Material)AssetDatabase.LoadAssetAtPath(path+"\\Materials\\"+objectName+".mat", typeof(Material));
				objectMaterial.shader = selectedShader;
				Debug.Log ("exists "+path+"\\Materials\\"+objectName+".mat");
			} else {
				objectMaterial = new Material( selectedShader );
				objectMaterial.name = objectName;
				AssetDatabase.CreateAsset(objectMaterial, path+"\\Materials\\"+objectName+".mat");
				Debug.Log ("created "+path+"\\Materials\\"+objectName+".mat");
			}
			
			// let's work through image types
			for (int i = 0; i < imageTypes.Count(); i++) {
				string fp_base = path+"\\"+objectName+imageFiles[i];
				if (pathContents.Contains(fp_base)) {
					// set texture
					if (objectMaterial.HasProperty(shaderMap[i])) {
						//Debug.Log ("has property: " + shaderMap[i]);
						setTextureImportSettings(fp_base, maxSizes[i], imageFormats[i], imageTypes[i] == "Normal");
						Texture t = (Texture)AssetDatabase.LoadAssetAtPath(fp_base, typeof(Texture));
						objectMaterial.SetTexture(shaderMap[i], t);
					}
					
				}
				
			}
			
			// set height
			if (objectMaterial.HasProperty ("_Parallax"))
				objectMaterial.SetFloat ("_Parallax", parallax);
			
			// now apply to object
			Renderer r = obj.GetComponent<Renderer>();
			
			if (r != null) {
				r.material = objectMaterial;
			}
			
			
			
			
		}
		
		
		void setTextureImportSettings(string path, int size, int compression, bool heightmap) {
			TextureImporter tImporter = AssetImporter.GetAtPath(path) as TextureImporter;
			//tImporter.mipmapEnabled = true;
			if (heightmap) {
				tImporter.textureType = TextureImporterType.Bump;
			} else {
				tImporter.textureType = TextureImporterType.Image;
			}
			switch (compression) {
			case 0: 
				tImporter.textureFormat = TextureImporterFormat.AutomaticCompressed;
				break;
			case 1: 
				tImporter.textureFormat = TextureImporterFormat.Automatic16bit;
				break;
			case 2: 
				tImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
				break;
			}
			
			tImporter.isReadable = true;
			tImporter.maxTextureSize = size;
			//AssetDatabase.ImportAsset( path, ImportAssetOptions.ForceUpdate );
			AssetDatabase.ImportAsset( path, ImportAssetOptions.Default );
			
		}
		
		
		
		void orderObjects(GameObject activeObj) {
			
			List<GameObject> childList = new List<GameObject>();
			List<Vector3> childListScales = new List<Vector3>();
			int numChildrenReordered = 0;
			
			if (activeObj)
			{
				//if (EditorUtility.DisplayDialog("Re-order?", "Re-Order Children of Selection, will reorder all the children of the Scene GameObject, \"" + activeObj.name + "\".\nContinue?", "Continue"))
				//{
				for (int i = activeObj.transform.childCount - 1; i > -1; i--)
				{
					GameObject child = activeObj.transform.GetChild(i).gameObject;
					childList.Add(child);
					childListScales.Add(child.transform.localScale);
					child.transform.parent = null;
				}
				
				List<GameObject> orderedChildList = childList.OrderBy(go => go.name).ToList();
				
				for (int i = 0; i < orderedChildList.Count; i++)
				{
					orderedChildList[i].transform.parent = activeObj.transform;
					Debug.Log(orderedChildList[i].name + " order = " + orderedChildList[i].transform.GetSiblingIndex());
					orderedChildList[i].transform.SetAsLastSibling();
					orderedChildList[i].transform.localScale = childListScales[i];
					numChildrenReordered++;
				}
				//	}
			}
		}
		*/
	}
	
