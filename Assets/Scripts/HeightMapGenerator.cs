using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator {

	public static HeightMap GenerateHeightMap(int width, HeightMapSettings settings, 
		Vector2 sampleCentre, bool useFalloff, Vector2 coord) {
		// as we always use squares, use width for both dimentions!
		float[,] values = Noise.GenerateNoiseMap (width, width, settings.noiseSettings, sampleCentre);

		AnimationCurve heightCurve_threadsafe = new AnimationCurve (settings.heightCurve.keys);

		float minValue = float.MaxValue;
		float maxValue = float.MinValue;

		for (int i = 0; i < width; i++) {
			for (int j = 0; j < width; j++) {

				values[i, j] *= heightCurve_threadsafe.Evaluate (values [i, j]) * settings.heightMultiplier;
				if (values [i, j] > maxValue) {
					maxValue = values [i, j];
				}
				if (values [i, j] < minValue) {
					minValue = values [i, j];
				}
			}
		}

		if (useFalloff) // need to modify the map by falloff in range minValue to maxValue
		{
			float normRange = maxValue - minValue;
			Anews anews = Islands.LocalNews(coord);
			if (!FalloffGenerator.falloffMaps.ContainsKey(anews.ToIndex()))
				Debug.LogError("Missing falloutmap number: " + anews.ToIndex()
					+ " of " + FalloffGenerator.falloffMaps.Keys.Count + " for anews: " + anews);
			float[,] falloff = FalloffGenerator.BuildFalloffMap(coord);
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < width; j++)
				{
					float normValue = (values[i, j] - minValue) / normRange;
					normValue = Mathf.Clamp01(normValue - falloff[i, j]);
					values[i, j] = normValue * normRange + minValue;
				}
			}
		}

		return new HeightMap (values, minValue, maxValue);
	}

}

public struct HeightMap {
	public readonly float[,] values;
	public readonly float minValue;
	public readonly float maxValue;

	public HeightMap (float[,] values, float minValue, float maxValue)
	{
		this.values = values;
		this.minValue = minValue;
		this.maxValue = maxValue;
	}
}

