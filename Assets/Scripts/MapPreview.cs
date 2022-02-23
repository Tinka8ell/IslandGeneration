using UnityEngine;
using System.Collections;

public class MapPreview : MonoBehaviour {

	public Renderer textureRender;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;
	public FalloffGenerator falloffGenerator;

	public enum DrawMode {NoiseMap, Mesh, FalloffMap, IslandMap, QuadLerpMap}; 
	public DrawMode drawMode;

	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
	public TextureData textureSettings;
	public FalloffSettings falloffSettings;

	public Material terrainMaterial;

	public Vector2 coord = Vector2.zero;

	public Islands.FindMode findMode;

	[Range(0,MeshSettings.numSupportedLODs-1)]
	public int editorPreviewLOD;
	public bool autoUpdate;

	int oldNumVertsPerLine;
	FalloffSettings oldFalloffSettings; 

public void DrawMapInEditor() {
		Islands.settings = falloffSettings.islandNoiseSettings; // make sure we are up to date
		textureSettings.ApplyToMaterial (terrainMaterial);
		textureSettings.UpdateMeshHeights (terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
		HeightMap heightMap = new HeightMap(new float[1,1], 0f, 1f);
		Vector2 sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;

		switch (drawMode)
		{
			case DrawMode.NoiseMap:
			case DrawMode.Mesh:
				heightMap = HeightMapGenerator.GenerateHeightMap(
					meshSettings.numVertsPerLine, heightMapSettings, sampleCentre,
					heightMapSettings.useFalloff, coord);
				break;
			case DrawMode.IslandMap:
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
				DrawMesh (MeshGenerator.GenerateTerrainMesh (heightMap.values,meshSettings, editorPreviewLOD));
				break;
			case DrawMode.FalloffMap:
				Anews anews = Islands.LocalNews(coord);
				Debug.LogFormat("FalloffMap for {0} with anews: {1}", coord, anews);
				if (!FalloffGenerator.falloffMaps.ContainsKey(anews.ToIndex()))
					Debug.LogError("Missing falloutmap number: " + anews.ToIndex()
						+ " of " + FalloffGenerator.falloffMaps.Keys.Count + " for anews: " + anews);
				float[,] falloffMap = FalloffGenerator.BuildFalloffMap(coord); 
				DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(falloffMap, 0, 1)));
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
								), 0, 1
							)
						)
					);
				break;
			case DrawMode.QuadLerpMap:
				DrawTexture(
					TextureGenerator.TextureFromHeightMap(
						new HeightMap(
							new QuadLerpMap(meshSettings.numVertsPerLine, falloffSettings, true, false, false, true).values, 0, 1
							)
						)
					);
				break;
		}
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
