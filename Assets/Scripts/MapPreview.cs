using UnityEngine;
using System.Collections;

public class MapPreview : MonoBehaviour {

	public Renderer textureRender;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;
	public FalloffGenerator falloffGenerator;

	public enum DrawMode {NoiseMap, Mesh, Quadrant, FalloffMap, IslandMap, SeaMap, TestTexture, TestHeightMap }; 
	public DrawMode drawMode;

	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
	public TextureData textureSettings;
	public FalloffSettings falloffSettings;

	public Material terrainMaterial;
	public Material seaMaterial;

	public Vector2 coord = Vector2.zero;

	public CornorDirection cornorDirection = CornorDirection.NE;

	public Islands.FindMode findMode;

	[Range(0,MeshSettings.numSupportedLODs-1)]
	public int editorPreviewLOD;
	public bool autoUpdate;

	int oldNumVertsPerLine;
	FalloffSettings oldFalloffSettings;

	public void DrawMapInEditor()
	{
		Islands.settings = falloffSettings.islandNoiseSettings; // make sure we are up to date
		textureSettings.ApplyToMaterial(terrainMaterial);
		textureSettings.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
		HeightMap heightMap = GetTestMap(meshSettings.numVertsPerLine);
		Vector2 sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;

		float falloffRange = 2 * ((1 << falloffSettings.islandNoiseSettings.powerLevel) - 1);

		Debug.LogWarningFormat("DrawMapInEditor: centre: {0}, coord: {1}, power: {2}, range: {3}", 
			sampleCentre, coord, falloffSettings.islandNoiseSettings.powerLevel, falloffRange);

		switch (drawMode)
		{
			case DrawMode.NoiseMap:
			case DrawMode.Mesh:
				heightMap = HeightMapGenerator.GenerateHeightMap(
					meshSettings.numVertsPerLine, heightMapSettings, sampleCentre, coord);
				break;
			case DrawMode.SeaMap:
				heightMap = HeightMapGenerator.GenerateSeaMap(
					meshSettings.numVertsPerLine, heightMapSettings, sampleCentre, 0.2f);
				break;
			case DrawMode.IslandMap:
			case DrawMode.Quadrant:
			case DrawMode.FalloffMap:
				coord = Islands.FindAnIsland(coord, findMode);
				break;
		}

		switch (drawMode)
        {
			case DrawMode.NoiseMap: 
				DrawTexture (TextureGenerator.TextureFromHeightMap (heightMap));
				break;
			case DrawMode.Mesh:
				DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
				break;
			case DrawMode.SeaMap:
				DrawMesh(MeshGenerator.GenerateSeaMesh(heightMap.values, meshSettings, editorPreviewLOD));
				break;
			case DrawMode.FalloffMap:
				Anews anews = Islands.LocalNews(coord);
				// Debug.LogFormat("FalloffMap for {0} with anews: {1}", coord, anews);
				float[,] falloffMap = FalloffGenerator.BuildFalloffMap(coord, meshSettings.numVertsPerLine);
				DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(falloffMap, 0, falloffRange)));
				break;
			case DrawMode.Quadrant:
				Anews quadAnews = Islands.LocalNews(coord);
				Corner corner = quadAnews.GetCorner(cornorDirection);
				Debug.LogFormat("Quadrant for {0} with anews: {1} and direction: {2} with corner: {3} and range: 0 - {4}"
					, coord, quadAnews, cornorDirection, corner, falloffRange);
				float[,] quadMap = FalloffGenerator.GetQuadrant(corner, meshSettings.numVertsPerLine);
				//for(int j = 0; j < quadMap.GetLength(0); j += 10)
				//{
				//	string line = "";
				//	for (int i = 0; i < quadMap.GetLength(1); i += 10)
				//	{
				//		line += " " + quadMap[j, i].ToString("F2");
				//	}
				//	Debug.Log(line);
				//}
				DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(quadMap, 0, falloffRange)));
				break;
			case DrawMode.IslandMap:
				/*
				for(Anews.Compass d = Anews.Compass.N; (int) d < Anews.CompassSize; d++)
                {
					Debug.LogFormat("{0} from {1} is {2}", d, coord, Islands.NextDoor(coord, d));
                }
				*/
				DrawTexture(
					TextureGenerator.TextureFromHeightMap(
						new HeightMap(
							Islands.GetIslandMap(
								meshSettings.numVertsPerLine, 
								falloffSettings.islandNoiseSettings, 
								coord
								), 0, Islands.settings.powerLevel
							)
						)
					);
				break;
			case DrawMode.TestTexture:
				HeightMap testMap = GetTestMap(meshSettings.numVertsPerLine);
				DrawTexture(
					TextureGenerator.TextureFromHeightMap(testMap));
				break;
			case DrawMode.TestHeightMap:
				HeightMap testHeightMap = GetTestMap(meshSettings.numVertsPerLine);
				DrawMesh(MeshGenerator.GenerateSeaMesh(testHeightMap.values, meshSettings, editorPreviewLOD));
				break;
		}
	}

	private static HeightMap GetTestMap(int numVertsPerLine)
    {
		// create an arrow pointing SW
		int size = numVertsPerLine;
		var testMap = new float[size, size];
		int thickness = size / 10;
		for (int offset = 0; offset < thickness; offset++)
		{
			for (int index = 0; index < size; index++)
			{
				// bottom line
				testMap[index, offset] = 1f;
				// left line
				testMap[offset, index] = 2f;
			}
		}
		for (int offset = 0; offset < thickness / 2; offset++)
		{
			for (int index = 0; index < size - offset; index++)
			{
				// diagonal
				testMap[index + offset, index] = 3f;
				testMap[index, index + offset] = 3f;
			}
		}
		return new HeightMap(
			testMap, 0f, 3f
			); ;
    }


	public void DrawTexture(Texture2D texture) {
		textureRender.sharedMaterial.mainTexture = texture;
		textureRender.transform.localScale = new Vector3 (texture.width, 1, texture.height) /10f;

		textureRender.gameObject.SetActive (true);
		meshFilter.gameObject.SetActive (false);
	}

	public void DrawMesh(MeshData meshData) {
		meshFilter.sharedMesh = meshData.CreateMesh ();

		textureRender.gameObject.SetActive (false);
		meshFilter.gameObject.SetActive (true);
	}



	void OnValuesUpdated() {
		if (!Application.isPlaying) {
			if (oldFalloffSettings || 
				oldFalloffSettings != falloffSettings || 
				oldNumVertsPerLine == 0 || 
				oldNumVertsPerLine != meshSettings.numVertsPerLine)
            {
				oldFalloffSettings = falloffSettings;
				oldNumVertsPerLine = meshSettings.numVertsPerLine;
				falloffGenerator.GenerateFalloffMaps(meshSettings.numVertsPerLine, falloffSettings);
			}
			DrawMapInEditor();
		}
	}

	void OnTextureValuesUpdated() {
		textureSettings.ApplyToMaterial (terrainMaterial);
	}

	void OnValidate() {


		if (falloffSettings!= null)
		{
			falloffSettings.OnValuesUpdated -= OnValuesUpdated;
			falloffSettings.OnValuesUpdated += OnValuesUpdated;
		}

		if (meshSettings != null)
		{
			meshSettings.OnValuesUpdated -= OnValuesUpdated;
			meshSettings.OnValuesUpdated += OnValuesUpdated;
		}
		if (heightMapSettings != null) {
			heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
			heightMapSettings.OnValuesUpdated += OnValuesUpdated;
		}
		if (textureSettings != null) {
			textureSettings.OnValuesUpdated -= OnTextureValuesUpdated;
			textureSettings.OnValuesUpdated += OnTextureValuesUpdated;
		}

	}

}
