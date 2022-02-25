using UnityEngine;

public class SeaChunk : Chunk
{
	readonly float waveHeight;
	public SeaChunk(Vector2 coord, HeightMapSettings heightMapSettings,
		MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex,
		Transform parent, Transform viewer, Material material, float waveHeight = 0.02f):
		base(coord, heightMapSettings,
		meshSettings, detailLevels, colliderLODIndex,
		parent, viewer, material, "Sea Chunk")
	{
		this.waveHeight = waveHeight;
	}

	public override void UpdateCollisionMesh()
    {
		// don't set collider on see mesh
    }


	public override HeightMap GenerateHeightMap()
	{

		return HeightMapGenerator.GenerateSeaMap(
			meshSettings.numVertsPerLine, heightMapSettings, 
			sampleCentre, waveHeight);
	}


}
