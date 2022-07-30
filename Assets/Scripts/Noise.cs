using UnityEngine;
using System.Collections;

public static class Noise {

	public enum NormalizeMode {Local, Global};

	private static Vector2[] octaveOffsets;
	private static float[] amplitudes;
	private static float[] frequencies;

	public static float maxPossibleHeight;
	private static bool initialised = false;

	public static void InitialiseNoise(NoiseSettings settings)
    {
		if (!initialised)
        {
			initialised = true;
			maxPossibleHeight = 0;
			octaveOffsets = new Vector2[settings.octaves];
			amplitudes = new float[settings.octaves];
			frequencies = new float[settings.octaves];

			System.Random prng = new System.Random(settings.seed);

			float amplitude = 1;
			float frequency = 1;

			for (int o = 0; o < settings.octaves; o++)
			{
				float offsetX = prng.Next(-100000, 100000) + settings.offset.x;
				float offsetY = prng.Next(-100000, 100000) - settings.offset.y;
				octaveOffsets[o] = new Vector2(offsetX, offsetY);

				maxPossibleHeight += amplitude;
				amplitude *= settings.persistance;
				frequency *= settings.lacunarity;
				amplitudes[o] = amplitude;
				frequencies[o] = frequency;
			}
		}
	}

	public static float[,] GenerateNoiseMap(
		int size, 
		NoiseSettings settings, 
		Vector2 sampleCentre,
		bool debug=false) {

		float[,] noiseMap = new float[size,size];

		InitialiseNoise(settings);

		System.Random prng = new System.Random (settings.seed);

		float maxLocalNoiseHeight = float.MinValue;
		float minLocalNoiseHeight = float.MaxValue;

		float half = (size - 1) / 2f;

		for (int x = 0; x < size; x++)
		{
			for (int y = 0; y < size; y++) {

				float noiseHeight = 0;

				for (int o = 0; o < settings.octaves; o++) {
					float sampleX = (x-half + octaveOffsets[o].x + sampleCentre.x) / settings.scale * frequencies[o];
					float sampleY = (y-half + octaveOffsets[o].y + sampleCentre.y) / settings.scale * frequencies[o];

					// float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1; // convert 0<=?<=1 to -1<=?<=1
					float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
					noiseHeight += perlinValue * amplitudes[o];
				}

				if (settings.normalizeMode == NormalizeMode.Global)
				{
					//// this calculation makes no sense!
					//// - maxPossibleHeight < noiseHeight < maxPossibleHeight
					//// so moving it up 1 before lerping it between 0 and 1 does not make sense at all
					//float normalizedHeight = (noiseHeight + 1) / (maxPossibleHeight / 0.9f);
					//noiseHeight = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
					float normalizedHeight = noiseHeight / maxPossibleHeight;
					noiseHeight = Mathf.Clamp(normalizedHeight, 0, 1);
				}
				else // settings.normalizeMode == NormalizeMode.Local
				{
					if (noiseHeight > maxLocalNoiseHeight)
					{
						maxLocalNoiseHeight = noiseHeight;
					}
					if (noiseHeight < minLocalNoiseHeight)
					{
						minLocalNoiseHeight = noiseHeight;
					}
				}
				noiseMap[x, y] = noiseHeight;
			}
		}

		// this is not being used at present, but does not make sense if we are are tiling anyway
		if (settings.normalizeMode == NormalizeMode.Local)
		{
			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y < size; y++) {
					noiseMap [x, y] = Mathf.InverseLerp (minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap [x, y]);
				}
			}
		}
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
