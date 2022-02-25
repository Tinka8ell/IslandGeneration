using UnityEngine;

public class TerrainChunk : Chunk
{
    readonly bool useFalloff;

	public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, 
		MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, 
		Transform parent, Transform viewer, Material material) :
		base (coord, heightMapSettings,
		meshSettings, detailLevels, colliderLODIndex,
		parent, viewer, material, "Terrain Chunk")
	{
		useFalloff = heightMapSettings.useFalloff;
	}

	public override HeightMap GenerateHeightMap()
	{
		return HeightMapGenerator.GenerateHeightMap(
				meshSettings.numVertsPerLine, heightMapSettings, sampleCentre,
				coord);

	}

}

