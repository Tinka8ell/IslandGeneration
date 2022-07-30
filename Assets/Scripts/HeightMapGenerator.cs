using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator {
	private static DumpData dumpData;

	public static HeightMap GenerateHeightMap(int width, HeightMapSettings settings, 
		Vector2 sampleCentre, Vector2 coord, bool debug=false) {
		// as we always use squares, use width for both dimentions!
		float[,] values = Noise.GenerateNoiseMap (width, settings.noiseSettings, sampleCentre);

		//float[,] falloff = FalloffGenerator.noFalloff;
		float[,] falloff = new float[width, width];
		// generated noise map is from 0 to 1;
		float minValue = 0;
		float maxValue = 1f;

		float heightMultiplier = settings.heightMultiplier;
		heightMultiplier = 1f;

		if (settings.useFalloff) // need to modify the map by falloff 
		{
			falloff = FalloffGenerator.BuildFalloffMap(coord, width);
			// extend by the falloff range
			maxValue += IslandNoiseSettings.maxLevel;
		}

		Debug.LogFormat("GenerateHeightMap({0}, settings, {1}, {2}), falloff width: {3}, maxValue: {4}",
			width, sampleCentre, coord, falloff.GetLength(0), maxValue);

		//DumpData dumpData = new DumpData();
		//if (debug)
		//      {
		//	dumpData.width = width;
		//	dumpData.sampleCentre = sampleCentre;
		//	dumpData.useFalloff = useFalloff;
		//	dumpData.coord = coord;
		//	dumpData.CaptureNoise(values);
		//}

		AnimationCurve heightCurve_threadsafe = new AnimationCurve (settings.heightCurve.keys);

		float minV = float.MaxValue;
		float maxV = float.MinValue;

		for (int j = 0; j < width; j++) {
			for (int i = 0; i < width; i++) {
				// first adjust by falloff if required
				values[i, j] += falloff[i, j]; // remove if 0, else augment if higher

				//// This does not make sense as we take the noise value 
				//// and multiply it by the animation curve value for itself
				//// and then finally the multiplier!
				//// should this have been a straight assignment?
				//values[j, i] *= heightCurve_threadsafe.Evaluate(values[j, i]);

				values[i, j] *= heightMultiplier;

                if (values[i, j] > maxV)
                {
                    maxV = values[i, j];
                }
                if (values[i, j] < minV)
                {
                    minV = values[i, j];
                }
            }
		}

		//if (debug) dumpData.CaptureValues(values);
		//if (debug) dumpData.ToFile();

		Debug.LogFormat("GenerateHeightMap: minValue = {0}, maxValue = {1}, minV = {2}, maxV = {3}",
			minValue * heightMultiplier, maxValue * heightMultiplier, minV, maxV);

		return new HeightMap (values, 1f * minValue * heightMultiplier, 1f * maxValue * heightMultiplier);
	}

	public static HeightMap GenerateNewHeightMap(int width, HeightMapSettings settings, 
		Vector2 sampleCentre, Vector2 coord, bool debug=false) {
		// as we always use squares, use width for both dimentions!
		float[,] values = Noise.GenerateNoiseMap (width, settings.noiseSettings, sampleCentre);

		//float[,] falloff = FalloffGenerator.noFalloff;
		float[,] falloff = new float[width, width];
		// generated noise map is from 0 to 1;
		float minValue = 0;
		float maxValue = 1f;

		float heightMultiplier = settings.heightMultiplier;
		heightMultiplier = 1f;

		if (settings.useFalloff) // need to modify the map by falloff 
		{
			falloff = FalloffGenerator.BuildFalloffMap(coord, width);
			// extend by the falloff range
			maxValue += IslandNoiseSettings.maxLevel;
		}

		Debug.LogFormat("GenerateHeightMap({0}, settings, {1}, {2}), falloff width: {3}, maxValue: {4}",
			width, sampleCentre, coord, falloff.GetLength(0), maxValue);

		//DumpData dumpData = new DumpData();
		//if (debug)
		//      {
		//	dumpData.width = width;
		//	dumpData.sampleCentre = sampleCentre;
		//	dumpData.useFalloff = useFalloff;
		//	dumpData.coord = coord;
		//	dumpData.CaptureNoise(values);
		//}

		AnimationCurve heightCurve_threadsafe = new AnimationCurve (settings.heightCurve.keys);

		float minV = float.MaxValue;
		float maxV = float.MinValue;

		for (int j = 0; j < width; j++) {
			for (int i = 0; i < width; i++) {
				// first adjust by falloff if required
				values[i, j] += falloff[i, j]; // remove if 0, else augment if higher

				//// This does not make sense as we take the noise value 
				//// and multiply it by the animation curve value for itself
				//// and then finally the multiplier!
				//// should this have been a straight assignment?
				//values[j, i] *= heightCurve_threadsafe.Evaluate(values[j, i]);

				values[i, j] *= heightMultiplier;

                if (values[i, j] > maxV)
                {
                    maxV = values[i, j];
                }
                if (values[i, j] < minV)
                {
                    minV = values[i, j];
                }
            }
		}

		//if (debug) dumpData.CaptureValues(values);
		//if (debug) dumpData.ToFile();

		Debug.LogFormat("GenerateHeightMap: minValue = {0}, maxValue = {1}, minV = {2}, maxV = {3}",
			minValue * heightMultiplier, maxValue * heightMultiplier, minV, maxV);

		return new HeightMap (values, 1f * minValue * heightMultiplier, 1f * maxValue * heightMultiplier);
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

