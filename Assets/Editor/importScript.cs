//C# Example
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class importScript : EditorWindow
{
	string materialNameBase = "";
	Shader selectedShader = null;
	GameObject root = null;

	float parallax = 0.005f;

	// image files
	string[] imageTypes = 		new string[] 	{	"Base",			"Normal", 		"Height"};
	string[] shaderMap = 	new string[] 		{	"_MainTex",	 	"_BumpMap", 	"_ParallaxMap"};
	string[] imageFiles = 		new string[] 	{	".jpg",	 		"_N.jpg", 		"_heights.jpg"};
	int[] imageFormats = 		new int[] 		{	0,				0, 				0};
	int[] maxSizes = 		new int[]		 	{	4096, 			2048, 			2048};

	private string[] compLabels = new string[] {"compressed", "16 bit", "Truecolour"};

	// Add menu item named "My Window" to the Window menu
	[MenuItem("TacticalSpace/Import/Set Object Properties", false, 10)]
	public static void ShowWindow()
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(importScript));
	}

	void OnFocus() {
		// initially set shader (although probably better elsewhere
		if (selectedShader == null) selectedShader = Shader.Find ("Standard");
	}

	void OnGUI()
	{
		GUILayout.Label ("Object Root", EditorStyles.boldLabel);



		if (root = (GameObject)EditorGUILayout.ObjectField (root, typeof(GameObject), true)) {
			if (root != null) {
				materialNameBase = root.transform.parent.name + "_" + root.name  + "_";
			}
		}

		GUILayout.Label ("Shader Settings", EditorStyles.boldLabel);



		//root = EditorGUILayout.
		//materialNameBase = EditorGUILayout.TextField ("material Base Name", materialNameBase);


		selectedShader = (Shader)EditorGUILayout.ObjectField (selectedShader, typeof(Shader), true);

		parallax = EditorGUILayout.Slider ("Heightmap value", parallax, 0.005f, 0.08f);

		// resolution, compression type
		for (int i = 0; i < imageTypes.Count(); i++) {
			GUILayout.Label (imageTypes[i]);
			shaderMap[i] = EditorGUILayout.TextField ("map", shaderMap[i]);
			imageFiles[i] = EditorGUILayout.TextField ("file", imageFiles[i]);
			maxSizes[i] = EditorGUILayout.IntField("max size", maxSizes[i]);
			imageFormats[i] = EditorGUILayout.IntSlider(compLabels[imageFormats[i]], imageFormats[i], 0, 2);

		}


		GUILayout.Label ("Tools", EditorStyles.boldLabel);

		EditorGUI.BeginDisabledGroup ((root == null) || (root.transform.childCount < 1));

		if( GUILayout.Button("Set Materials")) {
			setMaterials(root);
		}
		if( GUILayout.Button("Sort")) {
			orderObjects(root);
		}
		EditorGUI.EndDisabledGroup ();

	}









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

}

