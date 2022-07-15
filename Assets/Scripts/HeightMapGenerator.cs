using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator {
	private static DumpData dumpData;

	public static HeightMap GenerateHeightMap(int width, HeightMapSettings settings, 
		Vector2 sampleCentre, Vector2 coord, bool debug=false) {
		bool useFalloff = settings.useFalloff;
		// as we always use squares, use width for both dimentions!
		float[,] values = Noise.GenerateNoiseMap (width, settings.noiseSettings, sampleCentre);
		float[,] falloff = FalloffGenerator.noFalloff;
		if (useFalloff) // need to modify the map by falloff 
		{
			falloff = FalloffGenerator.BuildFalloffMap(coord);
		}

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
				// first adjust by falloff if required
				values[j, i] *= falloff[j, i]; // remove if 0, else augment if higher

				// This does not make sense as we take the noise value 
				// and multiply it by the animation curve value for itself
				// and then finally the multiplier!
				// should this have been a straight assignment?
				values[j, i] *= heightCurve_threadsafe.Evaluate(values[j, i]);
				values[j, i] *= settings.heightMultiplier;
				if (values [j, i] > maxValue) {
					maxValue = values [j, i];
				}
				if (values [j, i] < minValue) {
					minValue = values [j, i];
				}
			}
		}

		if (debug) dumpData.CaptureValues(values);
		if (debug) dumpData.ToFile();

		return new HeightMap (values, minValue, maxValue);
	}

	public static HeightMap GenerateSeaMap(int width, HeightMapSettings settings,
		Vector2 sampleCentre, float waveHeight = 0.02f, bool debug = false)
	{
		// as we always use squares, use width for both dimentions!
		float[,] values = Noise.GenerateNoiseMap(width, settings.noiseSettings, sampleCentre);

		DumpData dumpData = new DumpData();
		if (debug)
		{
			dumpData.width = width;
			dumpData.sampleCentre = sampleCentre;
			dumpData.CaptureNoise(values);
		}

		for (int j = 0; j < width; j++)
		{
			for (int i = 0; i < width; i++)
			{
				values[j, i] *= waveHeight;
			}
		}

		return new HeightMap(values, 0, waveHeight);
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

