using UnityEngine;
using System.Collections;

public static class Noise {

	public enum NormalizeMode {Local, Global};

	public static float[,] GenerateNoiseMap(
		int size, 
		NoiseSettings settings, 
		Vector2 sampleCentre,
		bool debug=false) {
		float[,] noiseMap = new float[size,size];

		System.Random prng = new System.Random (settings.seed);
		Vector2[] octaveOffsets = new Vector2[settings.octaves];
		float[] amplitudes = new float[settings.octaves];
		float[] frequencies = new float[settings.octaves];

		float maxPossibleHeight = 0;
		float amplitude = 1;
		float frequency = 1;

		for (int o = 0; o < settings.octaves; o++) {
			float offsetX = prng.Next (-100000, 100000) + settings.offset.x + sampleCentre.x;
			float offsetY = prng.Next (-100000, 100000) - settings.offset.y - sampleCentre.y;
			octaveOffsets [o] = new Vector2 (offsetX, offsetY);

			maxPossibleHeight += amplitude;
			amplitude *= settings.persistance;
			frequency *= settings.lacunarity;
			amplitudes[o] = amplitude;
			frequencies[o] = frequency;
		}

		float maxLocalNoiseHeight = float.MinValue;
		float minLocalNoiseHeight = float.MaxValue;

		float half = (size - 1) / 2f;

		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {

				float noiseHeight = 0;

				for (int o = 0; o < settings.octaves; o++) {
					float sampleX = (x-half + octaveOffsets[o].x) / settings.scale * frequencies[o];
					float sampleY = (y-half + octaveOffsets[o].y) / settings.scale * frequencies[o];

					float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
					noiseHeight += perlinValue * amplitudes[o];
				}

				if (noiseHeight > maxLocalNoiseHeight) {
					maxLocalNoiseHeight = noiseHeight;
				} 
				if (noiseHeight < minLocalNoiseHeight) {
					minLocalNoiseHeight = noiseHeight;
				}

				if (settings.normalizeMode == NormalizeMode.Global) {
					float normalizedHeight = (noiseHeight + 1) / (maxPossibleHeight / 0.9f);
					noiseHeight = Mathf.Clamp (normalizedHeight, 0, int.MaxValue);
				}

				noiseMap[y, x] = noiseHeight;
			}
		}

		if (debug)
        {
			int y = 0;
			int x = 0;
			Vector2 tl = new Vector2(
				(x - half + octaveOffsets[0].x) / settings.scale, 
				(y - half + octaveOffsets[0].y) / settings.scale);
			y = size - 1;
			x = size - 1;
			Vector2 br = new Vector2(
				(x - half + octaveOffsets[0].x) / settings.scale, 
				(y - half + octaveOffsets[0].y) / settings.scale);
			Debug.LogFormat("GenerateNoiseMap: edges for: {2}, {0}, {1}", tl, br, sampleCentre);
		}

		if (settings.normalizeMode == NormalizeMode.Local)
		{
			for (int y = 0; y < size; y++) {
				for (int x = 0; x < size; x++) {
					noiseMap [x, y] = Mathf.InverseLerp (minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap [x, y]);
				}
			}
		}
		/*
		noiseMap = new float[size, size];
		for (int y = 0; y < 10; y++)
		{
			for (int x = 0; x < 10; x++)
			{
				noiseMap[y, x] = .9f;
			}
		}
		*/
		return noiseMap;
	}

}

[System.Serializable]
public class NoiseSettings {
	public Noise.NormalizeMode normalizeMode;

	public float scale = 50;

	public int octaves = 6;
	[Range(0,1)]
	public float persistance =.6f;
	public float lacunarity = 2;

	public int seed;
	public Vector2 offset;

	public void ValidateValues() {
		scale = Mathf.Max (scale, 0.01f);
		octaves = Mathf.Max (octaves, 1);
		lacunarity = Mathf.Max (lacunarity, 1);
		persistance = Mathf.Clamp01 (persistance);
	}
}
