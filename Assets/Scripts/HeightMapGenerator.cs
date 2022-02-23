using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator {
	private static DumpData dumpData;

	public static HeightMap GenerateHeightMap(int width, HeightMapSettings settings, 
		Vector2 sampleCentre, bool useFalloff, Vector2 coord, bool debug=false) {
		// as we always use squares, use width for both dimentions!
		float[,] values = Noise.GenerateNoiseMap (width, settings.noiseSettings, sampleCentre);
		
		DumpData dumpData = new DumpData();
		if (debug)
        {
			dumpData.width = width;
			dumpData.sampleCentre = sampleCentre;
			dumpData.useFalloff = useFalloff;
			dumpData.coord = coord;
			dumpData.CaptureNoise(values);
		}

		AnimationCurve heightCurve_threadsafe = new AnimationCurve (settings.heightCurve.keys);

		float minValue = float.MaxValue;
		float maxValue = float.MinValue;

		for (int j = 0; j < width; j++) {
			for (int i = 0; i < width; i++) {

				values[j, i] *= heightCurve_threadsafe.Evaluate (values [j, i]) * settings.heightMultiplier;
				if (values [j, i] > maxValue) {
					maxValue = values [j, i];
				}
				if (values [j, i] < minValue) {
					minValue = values [j, i];
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
			if (debug) dumpData.CaptureFalloffMap(falloff);
			for (int j = 0; j < width; j++)
			{
				for (int i = 0; i < width; i++)
				{
					/*
					float normValue = (values[j, i] - minValue) / normRange;
					normValue = Mathf.Clamp01(normValue - falloff[j, i]);
					values[j, i] = normValue * normRange + minValue;
					*/
					//values[j, i] = Mathf.Clamp(values[j, i] - normRange * falloff[j, i], minValue, maxValue);
					values[j, i] -= settings.heightMultiplier * falloff[j, i];
				}
			}
		}
		if (debug) dumpData.CaptureValues(values);
		if (debug) dumpData.ToFile();

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

