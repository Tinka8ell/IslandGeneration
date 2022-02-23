using UnityEngine;

public class TerrainChunk {
	
//	const float colliderGenerationDistanceThreshold = 5; 
	public event System.Action<TerrainChunk, bool> OnVisibilityChanged;
	public Vector2 coord;

    readonly GameObject meshObject;
	Vector2 sampleCentre;
	Bounds bounds;

    readonly MeshRenderer meshRenderer;
    readonly MeshFilter meshFilter;
    readonly MeshCollider meshCollider;
    
	readonly LODInfo[] detailLevels;
    readonly LODMesh[] lodMeshes;
    readonly int colliderLODIndex;

	HeightMap heightMap;
	bool heightMapReceived;
	int previousLODIndex = -1;
	bool hasSetCollider;
    readonly float maxViewDst;

    readonly HeightMapSettings heightMapSettings;
    readonly MeshSettings meshSettings;
    readonly Transform viewer;

    readonly bool useFalloff;
//    readonly FalloffGenerator falloffGenerator;

	public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, 
		MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, 
		Transform parent, Transform viewer, Material material) {
		this.coord = coord;
		this.detailLevels = detailLevels;
		this.colliderLODIndex = colliderLODIndex;
		this.heightMapSettings = heightMapSettings;
		this.meshSettings = meshSettings;
		this.viewer = viewer;

		sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
		Vector2 position = coord * meshSettings.meshWorldSize ;
		bounds = new Bounds(position,Vector2.one * meshSettings.meshWorldSize );

		useFalloff = heightMapSettings.useFalloff;
		// TODO: this.falloffGenerator = falloffGenerator;

		meshObject = new GameObject("Terrain Chunk");
		meshRenderer = meshObject.AddComponent<MeshRenderer>();
		meshFilter = meshObject.AddComponent<MeshFilter>();
		meshCollider = meshObject.AddComponent<MeshCollider>();
		meshRenderer.material = material;

		meshObject.transform.position = new Vector3(position.x,0,position.y);
		meshObject.transform.parent = parent;
		SetVisible(false);

		lodMeshes = new LODMesh[detailLevels.Length];
		for (int i = 0; i < detailLevels.Length; i++) {
			lodMeshes[i] = new LODMesh(detailLevels[i].lod);
			lodMeshes[i].UpdateCallback += UpdateTerrainChunk;
			if (i == colliderLODIndex) {
				lodMeshes[i].UpdateCallback += UpdateCollisionMesh;
			}
		}

		maxViewDst = detailLevels [detailLevels.Length - 1].visibleDstThreshold;

	}

	public void Load() {
		bool debug = false;
		ThreadedDataRequester.RequestData(
			() => HeightMapGenerator.GenerateHeightMap (
				meshSettings.numVertsPerLine, heightMapSettings, sampleCentre, 
				useFalloff, coord, debug), OnHeightMapReceived);
	}



	void OnHeightMapReceived(object heightMapObject) {
		this.heightMap = (HeightMap)heightMapObject;
		heightMapReceived = true;

		UpdateTerrainChunk ();
	}

	Vector2 ViewerPosition {
		get {
			return new Vector2 (viewer.position.x, viewer.position.z);
		}
	}


	public void UpdateTerrainChunk() {
		if (heightMapReceived) {
			float viewerDstFromNearestEdge = Mathf.Sqrt (bounds.SqrDistance (ViewerPosition));

			bool wasVisible = IsVisible ();
			bool visible = viewerDstFromNearestEdge <= maxViewDst;

			if (visible) {
				int lodIndex = 0;

				for (int i = 0; i < detailLevels.Length - 1; i++) {
					if (viewerDstFromNearestEdge > detailLevels [i].visibleDstThreshold) {
						lodIndex = i + 1;
					} else {
						break;
					}
				}

				if (lodIndex != previousLODIndex) {
					LODMesh lodMesh = lodMeshes [lodIndex];
					if (lodMesh.hasMesh) {
						previousLODIndex = lodIndex;
						meshFilter.mesh = lodMesh.mesh;
					} else if (!lodMesh.hasRequestedMesh) {
						lodMesh.RequestMesh (heightMap, meshSettings);
					}
				}


			}

			if (wasVisible != visible) {
				
				SetVisible (visible);
				if (OnVisibilityChanged != null) {
					OnVisibilityChanged (this, visible);
				}
			}
		}
	}

	public void UpdateCollisionMesh() {
        if (!hasSetCollider) {
			float sqrDstFromViewerToEdge = bounds.SqrDistance (ViewerPosition);
			if (sqrDstFromViewerToEdge < detailLevels [colliderLODIndex].SqrVisibleDstThreshold) {
				if (!lodMeshes [colliderLODIndex].hasRequestedMesh) {
					lodMeshes [colliderLODIndex].RequestMesh (heightMap, meshSettings);
				}
			}

			// TODO: should this be permenantly removed?
			// if (sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold) {
				if (lodMeshes [colliderLODIndex].hasMesh) {
					meshCollider.sharedMesh = lodMeshes [colliderLODIndex].mesh;
					hasSetCollider = true;
				}
			// }
		}
	}

	public void SetVisible(bool visible) {
		meshObject.SetActive (visible);
	}

	public bool IsVisible() {
		return meshObject.activeSelf;
	}

}

class LODMesh {

	public Mesh mesh;
	public bool hasRequestedMesh;
	public bool hasMesh;
    readonly int lod;
	public event System.Action UpdateCallback;

	public LODMesh(int lod) {
		this.lod = lod;
	}

	void OnMeshDataReceived(object meshDataObject) {
		mesh = ((MeshData)meshDataObject).CreateMesh ();
		hasMesh = true;

		UpdateCallback ();
	}

	public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings) {
		hasRequestedMesh = true;
		ThreadedDataRequester.RequestData(
			() => MeshGenerator.GenerateTerrainMesh (heightMap.values, meshSettings, lod), 
			OnMeshDataReceived);
	}

}
