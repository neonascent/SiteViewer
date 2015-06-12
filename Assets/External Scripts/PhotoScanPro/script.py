import PhotoScan, os, sys

cmdargs = str(sys.argv)

PhotoScan.app.messageBox("Texturing and exporting parts")
chunk = PhotoScan.app.document.chunk
partPath = PhotoScan.app.getExistingDirectory("Select untextured parts folder")

texPartPath = partPath + '/../2. textured OBJ'
if not os.path.exists(texPartPath): 
	os.makedirs(texPartPath)
	

for filename in os.listdir(partPath):
	if filename[-4:] != ".obj":
		continue
	chunk.importModel(partPath + "/" + filename)
	chunk.buildUV(mapping=PhotoScan.GenericMapping) #AdaptiveOrthophotoMapping
	chunk.buildTexture(blending=PhotoScan.MosaicBlending, size=8192)  # 4096 8192
	chunk.exportModel(texPartPath+"/"+filename, binary=True, precision=6, texture_format='jpg', texture=True, normals=True, colors=True, cameras=False)
	


