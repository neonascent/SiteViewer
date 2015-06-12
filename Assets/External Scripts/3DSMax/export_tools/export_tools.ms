/*

XNormal remote control for 3ds Max

Created:	2014-06-01
Author:		Andreas Hackel
Web page:	http://www.andreashackel.de

Version:	1.0

*/

Rollout rolMain "Export tools" 
(
	subRollout scriptBin width:200 height:500 
	
	rollout SegmentingTools "Segmenting Tools" (
		label shiftPos "shift directions"
		spinner stdnum_x "X" pos:[40,26] width:25 height:16 range:[-100,100,0] type:#float fieldwidth:40
		spinner stdnum_y "Y" pos:[100,26] width:25 height:16 range:[-100,100,0] type:#float fieldwidth:40
		spinner stdnum_z "Z" pos:[160,26] width:25 height:16 range:[-100,100,0] type:#float fieldwidth:40
		button dupShiftButton "Copy and Shift" width:180 toolTip:"Copy envelope object and shift";
		button expandButton "Expand" width:180 toolTip:"Expand selected envelopes ready for high-poly cutting";
		button booleanButton "Boolean & Export (low poly)" width:180 toolTip:"Use selected envelopes to cut and export low-poly object";
		button cutButton "Cut & Export (high poly)" width:180 toolTip:"Use selected envelopes to cut and export high-poly object";
		button loadAllButton "load all parts" width:180 toolTip:"Load all objs (with object names) from a directory";
		
		on SegmentingTools rolledUp bState do
        (
            if (bState == true) then
            (
                rolMain.height += SegmentingTools.height
                rolMain.height += 4
            )
            else
            (
                rolMain.height -= SegmentingTools.height
                rolMain.height -= 4
            )
			scriptBin.height = rolMain.height-10;
        )
		
		on expandButton pressed do
		(
			--Loop through the array of object it returns
			for i = 1 to selection.count do
			(
				-- centre pivot and scale
				selection[i].pivot = selection[i].center
				in coordsys local selection[i].scale= [1.1, 1.1, 1.1]
				selection[i].name = "high_" + selection[i].name
			)
		)	
		
		on dupShiftButton pressed do
			(
			--clone the selected objects
			maxOps.cloneNodes (selection as array) cloneType:#instance newNodes:&nnl #nodialog

			--Loop through the array of object it returns
			for i = 1 to nnl.count do
			(
				--rename the objects to a unique name
				--nnl[i].name = uniqueName "changethis" numDigits:3
				in coordsys local nnl[i].pos += [stdnum_x.value,stdnum_y.value,stdnum_z.value] -- moves selected object 10 units along its local X axis
			)
			select nnl
			)
		
		on booleanButton pressed do
	  (
		
		thePath = getSavepath()
		mainObject  = getnodebyname "default"
		envelopes = selection as array
		-- select export format OBJ
		theClasses = exporterPlugin.classes
		_objIdx = findItem theClasses ObjExp
		
		-- set up export properties
		-- sellect first envelope box
		select selection[1]
		exportFile (thePath + "/settings.obj" ) selectedOnly:on using:theClasses[_objIdx]
		deleteFile (thePath + "/settings.obj" )
		
		  
		-- get 
		  
		if mainObject != undefined then (
			
			if envelopes.count > 0 then (
				--Loop through the array of object it returns
				for i = 1 to envelopes.count do
				(
					-- boolObj.createBooleanObject <operand_A> [ <operand_B> <add_method> <mat_method> ]  
					boolObj.createBooleanObject envelopes[i] 
					--boolObj.setOperandB <bool_obj> <operand_B> <add_method> <mat_method> 
					boolObj.setOperandB envelopes[i] mainObject 3 5
					boolObj.setBoolOp envelopes[i] 2
					
					
					-- export
					theFile= envelopes[i].name
					print  (thePath + "/" + theFile)
					select envelopes[i]
					-- with *.OBJ export dialog
					--exportFile (thePath + "/" + theFile ) selectedOnly:on using:theClasses[_objIdx]
					-- without *.OBJ export dialog
					exportFile (thePath + "/" + theFile ) #noprompt selectedOnly:on using:theClasses[_objIdx]
					delete selection
				)
				messageBox ("Export done")
			) else (
				messageBox ("no parts envelopes selected")
			)
		) else (
			messageBox ("default object not present")
		)
	  )
	  
		on cutButton pressed do
		(

		thePath = getSavepath()
		mainObject  = getnodebyname "default"
		envelopes = selection as array
		-- select export format OBJ
		theClasses = exporterPlugin.classes
		_objIdx = findItem theClasses ObjExp

		-- set up export properties
		-- sellect first envelope box
		select selection[1]
		exportFile (thePath + "/settings.obj" ) selectedOnly:on using:theClasses[_objIdx]
		deleteFile (thePath + "/settings.obj" )

		  
		-- get 
		  
		if mainObject != undefined then (
			
			if envelopes.count > 0 then (
				--Loop through the array of object it returns
				for i = 1 to envelopes.count do
				(
					-- apply modifier to cut out sections
					addModifier mainObject (volumeselect level:1 type:1 invert:true method:1 volume:3 node:envelopes[i])
					addModifier mainObject (edit_Poly currentOperation:10 useStackSelection:true)
					
					-- export
					theFile= envelopes[i].name
					print  (thePath + "/" + theFile)
					select mainObject
					-- with *.OBJ export dialog
					--exportFile (thePath + "/" + theFile ) selectedOnly:on using:theClasses[_objIdx]
					-- without *.OBJ export dialog
					exportFile (thePath + "/" + theFile ) #noprompt selectedOnly:on using:theClasses[_objIdx]
					
					-- remove modifier 
					deleteModifier mainObject 1
					deleteModifier mainObject 1

				)
				messageBox ("Export done")
			) else (
				messageBox ("no parts envelopes selected")
			)
		) else (
			messageBox ("default object not present")
		)
		)
		
		on loadAllButton pressed do
		(


		  
			-- get objs
			thePath = getSavepath("Select Textured OBJ path")
			listFiles = getFiles (thePath+"/*.obj")
			  
			if listFiles.count > 0 then (
				--Loop through the array of object it returns
				for i = 1 to listFiles.count do
				(
					done = importFile (listFiles[i]) #noPrompt
					objName = getFilenameFile listFiles[i]
					selection[1].name = objName
				)
				 
			) else (
				messageBox ("no objs in selected directory")
			)
		)

	)
	
	rollout xNormalBatch "xNormal Batch" (
		--label lblLowPoly "" align:#left;
		--label lblHighPoly "" align:#left;
		dropdownlist ddlSize "" items:#("128", "256", "512", "1024", "2048", "4096") selection:6 tooltip:"Bitmap Size";
		dropdownlist ddlFormat "" items:#("tga", "tif", "jpg", "png", "bmp") selection:3 tooltip:"Format";
		dropdownlist ddlAntiAliasing "" items:#("x1", "x2", "x4") selection:2 tooltip:"Antialiasing";
		dropdownlist ddlPadding "" items:#("0", "2", "4", "8", "16", "32") selection:5 tooltip:"Padding";
		label lblSpacer3 "";
		checkbox chkBaseColor "Color Map" checked:true;
		checkbox chkNormal "Normal Map" checked:true;
		checkbox chkHeight "Height Map" checked:true;
		checkbox chkOSNormal "Object Space Normal Map";
		checkbox chkAO "Ambient Occlusion Map";
		--checkbox chkCavity "Cavity Map";
		--checkbox chkConvexity "Convexity Map";
		--checkbox chkVC "Bake Vertex Colors";
		--checkbox chkThickness "Thickness Map";
		--button btnAssignMaps "Assign maps" width:140 tooltip:"Assign selected maps to low poly mesh";
		label lblSpacer4 "";
		label lblDistances "Ray Distances"
		spinner spnMaxRayDistanceFront "Max Front" width:25 height:16 range:[0,5,0.10] type:#float fieldwidth:40
		spinner spnMaxRayDistanceBack  "Max Back " width:25 height:16 range:[0,5,0.10] type:#float fieldwidth:40
		
		
		--checkbox chkUseCage "Use Cage" tooltip:"Use a cage instead of a fixed ray distance"; --  checked: true;
		checkbox chkDeleteExportedModels "Delete Exported Models" checked: true tooltip:"Delete temporary FBX files after baking";
		--button btnOpenFolder "Open Folder" width:140 tooltip: "Open folder with baked files";
		button btnBake "Bake" width:180 toolTip:"Bake selected maps in xNormal";
		
		on xNormalBatch rolledUp bState do
        (
            if (bState == true) then
            (
                rolMain.height += xNormalBatch.height
                rolMain.height += 4
            )
            else
            (
                rolMain.height -= xNormalBatch.height
                rolMain.height -= 4
            )
			scriptBin.height = rolMain.height-10;
        )
		
		struct mapStruct (name, desc, param, enabled);
		local maps = #(
			(mapStruct name:"base" desc:"Base Color Map" param:"%gXNGenBaseTex" enabled:"true"),
			(mapStruct name:"normal" desc:"Normal Map" param:"%gXNGenNormals" enabled:"true"),
			(mapStruct name:"height" desc:"Height Map" param:"%gXNGenHeights" enabled:"true"),
			(mapStruct name:"occlusion" desc:"Ambient Occlusion Map" param:"%gXNGenAO" enabled:"false"));
			
		local scriptPath = getFilenamePath(getThisScriptFilename());
		local defaultConfigFileName = scriptPath + "xnormal_default.xml";
		local configFileName = scriptPath + "xnormal_settings.xml";
		local appPath = "C:\\Program Files\\S.Orgaz\\xNormal 3.19.1\\x64\\";
		local appExe = "xNormal.exe";
		local lowPolyMesh;
		local targetPath; 
		local highPolyMesh;
		local baking = false;
			
		local settings = #();
			
		fn selectDropdownList list value = (
			list.selection = findItem list.items value;
		)
	
		fn exportObjectsFBX objs fileName highpoly:false cage:false =
		(
			with redraw off
			(
				local FBX_path = targetPath; --@"C:\FBX\";
				--makeDir FBX_path all:true;
				
				FbxExporterSetParam "Animation" false;
				FbxExporterSetParam "ASCII" false;
				FbxExporterSetParam "AxisConversionMethod" "None";
				FbxExporterSetParam "Cameras" false;
				FbxExporterSetParam "CAT2HIK" false;
				FbxExporterSetParam "EmbedTextures" false;
				FbxExporterSetParam "FileVersion" "FBX201300";
				FbxExporterSetParam "GeomAsBone" false;
				FbxExporterSetParam "GenerateLog" false;
				FbxExporterSetParam "Lights" false;
				FbxExporterSetParam "NormalsPerPoly" false;
				FbxExporterSetParam "PointCache" false;
				FbxExporterSetParam "Preserveinstances" false;
				FbxExporterSetParam "ScaleFactor" 1;
				FbxExporterSetParam "SelectionSetExport" false;
				FbxExporterSetParam "Shape" false;
				FbxExporterSetParam "Skin" false;
				FbxExporterSetParam "ShowWarnings" true;
				FbxExporterSetParam "SmoothingGroups" true; --(not (highpoly or cage));
				FbxExporterSetParam "SmoothMeshExport" false;
				FbxExporterSetParam "TangentSpaceExport" true; --false; --(not (highpoly or cage));
				FbxExporterSetParam "Triangulate" true;
				FbxExporterSetParam "UpAxis" "Z";
				FbxExporterSetParam "PreserveEdgeOrientation" true;

		
				local original_selection = getCurrentSelection();
				
				--local original_pos = obj.pos;
				--obj.pos = [0, 0, 0];
				select (for o in objs collect o);
				--selectmore o.children;

				local triangulate;
				if highpoly == false then (
					triangulate = Turn_To_Poly limitPolySize:on maxPolySize:3 name:"triangulate";
					addModifier $ triangulate;
				)

				exportFile fileName #noPrompt selectedOnly:true;
				
				if highpoly == false then (
					deleteModifier $ triangulate;
				)
				
				print ("exported " + fileName); 
				--obj.pos = original_pos;
				select original_selection;
			)
			return fileName;
		)	

		fn getSetting id = (
			for s in settings do (
				if s[1] == id then
					return s[2];
			)
			return "";
		)
		
		fn setSetting id value = (
			for s in settings do (
				if s[1] == id then (
					s[2] = value;
					return true;
				)
			)
			append settings #(id, value);
		)
		
		fn getSettings = (
			local name = "";
			if lowPolymesh != undefined then
				name = lowPolyMesh.name;
			
			-- Default values:		
			append settings #("%gXNHighPolyMesh", "");
			append settings #("%gXNHighBaseTex", "");
			append settings #("%gXNLowPolyMesh", "");
			append settings #("%gXNUseCage", "true");
			append settings #("%gXNCageFile", "");
			append settings #("%gXNBitmapX", "1024");
			append settings #("%gXNBitmapY", "1024");
			append settings #("%gXNEdgePadding", "16");
			append settings #("%gXNTexture", "T_" + name);
			append settings #("%gXNTangentSpace", "true");
			append settings #("%gXNDeleteExportedModels", "true");
			append settings #("%gXNAA", "2");
			append settings #("%gXNMaxRayDistanceFront", "0.1");
			append settings #("%gXNMaxRayDistanceBack", "0.1");
			
			
			for map in maps do
				append settings #(map.param, map.enabled);
			
			local fileFormat = trimLeft (getFileNameType (getSetting "%gXNTexture")) ".";
			if fileFormat == "" then
				fileFormat = "tga";
		)

		fn storeSettings = (
			
			setSetting "%gXNBitmapX" ddlSize.selected;
			setSetting "%gXNBitmapY" ddlSize.selected;
			setSetting "%gXNAA" ddlAntialiasing.selected[2];
			setSetting "%gXNEdgePadding" ddlPadding.selected;
			setSetting "%gXNGenBaseTex" (chkBaseColor.checked as string);
			setSetting "%gXNGenNormals" ((chkNormal.checked or chkOSNormal.checked) as string);
			setSetting "%gXNGenHeights" (chkHeight.checked as string);
			setSetting "%gXNTangentSpace" ((not chkOSNormal.checked) as string);
			setSetting "%gXNGenAO" (chkAO.checked as string);
			setSetting "%gXNUseCage" "false";--(chkUseCage.checked as string);
			setSetting "%gXNDeleteExportedModels" (chkDeleteExportedModels.checked as string);
			setSetting "%gXNTexture" (targetPath +  lowPolymesh.name + "." + ddlFormat.selected);
			setSetting "%gXNMaxRayDistanceFront" ((spnMaxRayDistanceFront.value) as string);
			setSetting "%gXNMaxRayDistanceBack" ((spnMaxRayDistanceBack.value) as string);

			local lowPolyFile = (exportObjectsFBX #(lowPolyMesh) (targetPath + lowPolyMesh.name + ".fbx"));
			local highPolyFile = (exportObjectsFBX #(highPolyMesh) (targetPath + lowPolyMesh.name + "_highpoly.fbx") highpoly:true);
			local cageFile = "";

			local highPolyModel = highPolyMesh;
			local highBaseTex = "";
			if highPolyModel != undefined and highpolyModel.mat != undefined and classOf highpolyModel.mat == standard and highpolyModel.mat.diffuseMap != undefined then
				highBaseTex = highpolyModel.mat.diffuseMap.filename;
			
			setSetting "%gXNHighBaseTex" highBaseTex;
			
			-- export cage:
			if (getSetting "%gXNUseCage") == "true" then (
				local oldSelection = getCurrentSelection();
				select lowPolymesh;
				local cageName = lowPolyMesh.name + "_cage";
				projectionModifier.exportCage cageName;
				local cage = getNodeByName cageName;
				if cage != undefined then (
					cageFile = (exportObjectsFBX #(cage) (targetPath + cageName + ".fbx") cage:true);
					delete cage;
				)
				select oldSelection;
			)
			
			setSetting "%gXNHighPolyMesh" highPolyFile;
			setSetting "%gXNLowPolyMesh" lowPolyFile;
			setSetting "%gXNCageFile" cageFile;
		)

		fn readConfigFile fileName = (
			local file = openFile fileName;
			seek file #eof
			local maxlen = filepos file
			seek file 0
			local stream = readChars file maxlen errorAtEOF:false;
			close file;
			return stream;
		)

		fn writeConfigFile fileName stream = (
			local file = openFile fileName mode:"w"
			format "%" stream to:file;
			close file;
		)
		
		fn replaceParams &stream params = (
			for param in params do (
				local pos = findString stream param[1];
				if pos != undefined then (
					stream = replace stream pos param[1].count param[2];
				)
			)
		)
		
		fn getMapFilename oldSuffix old:true withPath:false = (
			local baseName = getSetting "%gXNTexture";
			local fileName = getFilenameFile baseName;
			local ext = getFilenameType baseName;
			local path = "";
			if withPath then
				path = getFilenamePath baseName;
			
			if old then
				return path + fileName + oldSuffix + ext
			else (
				local newSuffix = "";
			
				--if oldSuffix == "_baseTexBaked" then newSuffix = "";
				if oldSuffix == "_normals" then (
					if getSetting "%gXNTangentSpace" == "true" then
						newSuffix = "_N"
					else
						newSuffix = "_OSN";
				)
				if oldSuffix == "_occlusion" then newSuffix = "_AO";
				
				return path + fileName + newSuffix + ext;
			)
		)

		fn renameOutputFile oldSuffix = (
			local oldPath = getMapFilename oldSuffix withPath:true;
			local newPath = getMapFilename oldSuffix withPath:true old:false;

			if doesFileExist oldPath then (
				if doesFileExist newPath then
					deleteFile newPath;
				renameFile oldPath newPath;
			)
		)

		fn renameFiles = (
			if getSetting "%gXNGenBaseTex" == "true" then
				renameOutputFile "_baseTexBaked";
			if getSetting "%gXNGenNormals" == "true" then
				renameOutputFile "_normals";
			if getSetting "%gXNGenAO" == "true" then
				renameOutputFile "_occlusion";
			if getSetting "%gXNBakeHighpolyVCols" == "true" then
				renameOutputFile "_vcols";
		)

		fn deleteExportedModels = (
			deleteFile (getSetting "%gXNHighPolyMesh");
			deleteFile (getSetting "%gXNLowPolyMesh");
			deleteFile (getSetting "%gXNCageFile");
		)
		
		fn launchXNormal params = (
			local stream = readConfigFile defaultConfigFileName;
			replaceParams &stream params;
			writeConfigFile configFileName stream;
			
			local launchParam = appExe + " " + configFileName;
			hiddenDOSCommand launchParam startPath:appPath;
			print ("Started with : " + launchparam);
			renameFiles();
			if (getSetting "%gXNDeleteExportedModels") == "true" then
				deleteExportedModels();
		)

		
		fn bakeXNormal highPolymesh lowPolyMesh = (
			baking = true;
			setWaitCursor();
			print "storing settings...";
			storeSettings();
			launchXNormal settings;
			setArrowCursor();
			baking = false;
		)
		
		--fn enableItems enable = (
		--	for control in XNormalRollout.controls do
		--		control.enabled = enable;
		--)
		
		
		fn assignMaps = (
			if lowPolyMesh == undefined then return();
			print "assigning maps...";
			
			local mat = lowPolyMesh.material;
			if mat == undefined then
				mat = Standard name:("M_" + lowPolyMesh.name);
			lowPolyMesh.material = mat;
			
			local normalMapFile = getMapFilename "_normals" withPath:true old:false;
			local hasNormalMap = (getSetting "%gXNGenNormals") == "true" and doesFileExist normalMapFile;
			mat.bumpMapEnable = hasNormalMap;
			mat.bumpMapAmount = 100;
			if hasNormalMap then (
				if mat.bumpMap == undefined then
					mat.bumpMap = normal_bump name:(lowPolyMesh.name + "_normal_bump");
				if mat.bumpMap.normal_map == undefined then
					mat.bumpMap.normal_map = bitmapTexture name:(fileNameFromPath normalMapFile);
				mat.bumpMap.normal_map.filename = fileNameFromPath normalMapFile;
			) else
				mat.bumpMap = undefined;
			
			local AOMapFile = getMapFilename "_occlusion" withPath:true old:false;
			local hasAOMap = (getSetting "%gXNGenAO") == "true" and doesFileExist AOMapFile;
			mat.diffuseMapEnable = hasAOMap;
			if hasAOMap then (
				if mat.diffuseMap == undefined then
					mat.diffuseMap = bitmapTexture name:(fileNameFromPath AOMapFile);
				mat.diffuseMap.filename = fileNameFromPath AOMapFile;
			) else
				mat.diffuseMap = undefined;
			
			showTextureMap mat on;
		)

		on btnBake pressed do (
			if doesFileExist (appPath + appExe) == false then (
				messageBox ("'" + appPath + appExe + "' not found. Please install xNormal.");
				return false;
			)
			
			-- get objs
			lowPath = (getSavepath caption:"Select Low-poly textured OBJ path")
			listFiles = getFiles (lowPath+"/*.obj")
			
			highPath = (getSavepath caption:"Select High-poly textured OBJ path") + "\\"
			targetPath = (getSavepath caption:"Select target path") + "\\"

			if listFiles.count > 0 then (
				--Loop through the array of object it returns
				for i = 1 to listFiles.count do
				(
					-- import low
					importFile (listFiles[i]) #noPrompt
					objName = getFilenameFile listFiles[i]
					selection[1].name = objName
					lowPolymesh = getnodebyname objName

					importFile (highPath + "high_" + objName + ".obj") #noPrompt
					selection[1].name = ("high_" + objName)
					highPolyMesh = getnodebyname ("high_" + objName)
					
					bakeXNormal highPolyMesh lowPolymesh;
					
					
					delete highPolyMesh
					assignMaps()
				)
				 
			) else (
				messageBox ("no objs in selected directory")
			)

			-- load up file and set projection modifier then run.
			
			
			
			
		)
		)
	
	
		
	on rolMain open do
    (
        addSubRollout scriptBin SegmentingTools  rolledUp:false
        addSubRollout scriptBin xNormalBatch rolledUp:true
		rolMain.height = SegmentingTools.height + 60;
		scriptBin.height = rolMain.height-10;
    )
	
	
)

CreateDialog rolMain 220 400
