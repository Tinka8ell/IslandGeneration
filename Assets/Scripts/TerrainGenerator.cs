using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TerrainGenerator : MonoBehaviour {

	const float viewerMoveThresholdForChunkUpdate = 25f;
	const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

	FalloffGenerator falloffGenerator;

	public int colliderLODIndex;
	public LODInfo[] detailLevels;

	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
	public TextureData textureSettings;
	public FalloffSettings falloffSettings;

	public Transform viewer;
	public Material mapMaterial;
	public Material seaMaterial;

	Vector2 viewerPosition;
	Vector2 viewerPositionOld;

	float meshWorldSize;
	int chunksVisibleInViewDst;

	float waveHeight = 0.02f;

	readonly Dictionary<Vector2, Chunk> terrainChunkDictionary = new Dictionary<Vector2, Chunk>();
	readonly Dictionary<Vector2, Chunk> seaChunkDictionary = new Dictionary<Vector2, Chunk>();
	readonly List<Chunk> visibleChunks = new List<Chunk>();

	private void Awake()
	{
		MapPreview mapPreview = GetComponentInParent<MapPreview>();
		if (mapPreview)
		{
			meshSettings = mapPreview.meshSettings;
			heightMapSettings = mapPreview.heightMapSettings;
			textureSettings = mapPreview.textureSettings;
			falloffSettings = mapPreview.falloffSettings;
			falloffGenerator = mapPreview.falloffGenerator;
		}
		if (textureSettings.layers.Length > 0)
        {
			waveHeight = textureSettings.layers[1].blendStrength + textureSettings.layers[1].startHeight; // get the height at which land colour starts
			waveHeight *= heightMapSettings.heightMultiplier / 5;
		}
	}

	void Start() {

		textureSettings.ApplyToMaterial (mapMaterial);
		textureSettings.UpdateMeshHeights (mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

		float maxViewDst = detailLevels [detailLevels.Length - 1].visibleDstThreshold;
		meshWorldSize = meshSettings.meshWorldSize;
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);
		falloffGenerator.GenerateFalloffMaps(meshSettings.numVertsPerLine, falloffSettings);

// for debugging!
		viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
//		int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
//		int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);
//		Vector2 coord = new Vector2(currentChunkCoordX, currentChunkCoordY);
//		Anews anews = Islands.LocalNews(coord); 
		UpdateVisibleChunks();
		viewerPositionOld = new Vector2(meshWorldSize * meshWorldSize, meshWorldSize * meshWorldSize); // some way away!
	}

	void Update() {
		viewerPosition = new Vector2 (viewer.position.x, viewer.position.z); 

		if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
			foreach (Chunk chunk in visibleChunks)
			{
				chunk.UpdateCollisionMesh();
			}
			viewerPositionOld = viewerPosition;
			UpdateVisibleChunks();
		}
	}

	private void UpdateVisibleChunks()
    {
        HashSet<Vector2> alreadyUpdatedTerrainChunkCoords = new HashSet<Vector2>();
        HashSet<Vector2> alreadyUpdatedSeaChunkCoords = new HashSet<Vector2>();
        for (int i = visibleChunks.Count - 1; i >= 0; i--)
        {
            Chunk visibleChunk = visibleChunks[i];
            if (visibleChunk is TerrainChunk)
            {
                alreadyUpdatedTerrainChunkCoords.Add(visibleChunk.coord);
            }
            else
            {
                alreadyUpdatedSeaChunkCoords.Add(visibleChunk.coord);
            }
            visibleChunk.UpdateChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);
        FindNewlyVisible(currentChunkCoordX, currentChunkCoordY, terrainChunkDictionary, alreadyUpdatedTerrainChunkCoords, false);
		FindNewlyVisible(currentChunkCoordX, currentChunkCoordY, seaChunkDictionary, alreadyUpdatedSeaChunkCoords, true);
	}

	private void FindNewlyVisible(int currentChunkCoordX, int currentChunkCoordY, Dictionary<Vector2, Chunk> chunkDictionary, HashSet<Vector2> alreadyUpdatedChunkCoords, bool isSeaChunk)
    {
        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    if (chunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        chunkDictionary[viewedChunkCoord].UpdateChunk();
                    }
                    else
                    {
                        Chunk newChunk;
                        if (isSeaChunk)
                        {
                            newChunk = new SeaChunk(
                                viewedChunkCoord, heightMapSettings, meshSettings, detailLevels,
                                colliderLODIndex, transform, viewer, seaMaterial, waveHeight) as Chunk;
                        }
                        else
                        {
                            newChunk = new TerrainChunk(
                                viewedChunkCoord, heightMapSettings, meshSettings, detailLevels,
                                colliderLODIndex, transform, viewer, mapMaterial) as Chunk;
                        }
                        chunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.OnVisibilityChanged += OnChunkVisibilityChanged;
                        newChunk.Load();
                    }
                }

            }
        }
    }

    void OnChunkVisibilityChanged(Chunk chunk, bool isVisible) {
		if (isVisible) {
			visibleChunks.Add (chunk);
		} else {
			visibleChunks.Remove (chunk);
		}
	}

}

[System.Serializable]
public struct LODInfo {
	[Range(0,MeshSettings.numSupportedLODs-1)]
	public int lod;
	public float visibleDstThreshold;


	public float SqrVisibleDstThreshold {
		get {
			return visibleDstThreshold * visibleDstThreshold;
		}
	}
}
