using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class Islands
{
	public enum FindMode { None, Nearest, DriftEast }

	public static Vector2 FindAnIsland(Vector2 start, IslandNoiseSettings settings, FindMode findMode)
    {
		Debug.Log("FindAnIsland: start = " + start + ", findMode = " + findMode);
		Vector2 test = start;
        switch (findMode)
        {
			case FindMode.Nearest:
				test = NearestIsland(test, settings);
				break;
			case FindMode.DriftEast:
				test = DriftEastToIsland(test, settings);
				break;
			default:
				// Just return original
				break;
		}
		Debug.Log("FindAnIsland: returning: " + test);
		return test;
    }

	public static Vector2 NearestIsland(Vector2 start, IslandNoiseSettings settings)
	{
		Debug.Log("NearestIsland: start = " + start);
		Vector2 test = start;
		int offset = 0;
		int radius = 0;
		int maxOffset = -1;
		while (!Check(test, settings, true)) // not found one yet
		{
			offset++;
			if (offset < maxOffset)
            {
				switch (offset / 4)
				{
					case 0: // left to right
						test = new Vector2(test.x + 1, test.y);
						break;
					case 1: // top to bottom
						test = new Vector2(test.x, test.y + 1);
						break;
					case 2: // right to left
						test = new Vector2(test.x - 1, test.y);
						break;
					case 3: // bottom to top
						test = new Vector2(test.x, test.y - 1);
						break;
				}
			}
			else
			{
				radius++; // try further away
				Debug.Log("trying further away: radius = " + radius);
				offset = 0;
				maxOffset = radius * 2 * 4;
				test = new Vector2(start.x - radius, start.y - radius);
			}
		}
		return test;
	}

	public static Vector2 DriftEastToIsland(Vector2 start, IslandNoiseSettings settings)
	{
		Debug.Log("DriftEastToIsland: start = " + start);
		Vector2 test = start;
		return test;
	}

	public static bool IsIsland(Vector2 coord, IslandNoiseSettings settings, bool debug = false)
	{
		System.Random prng = new System.Random(settings.seed);
		float offsetX = prng.Next(-100000, 100000) + settings.offset.x + coord.x;
		float offsetY = prng.Next(-100000, 100000) - settings.offset.y - coord.y;
		float sampleX = offsetX / settings.scale;
		float sampleY = offsetY / settings.scale;
		float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
		return perlinValue > settings.threshold;
	}

	public static bool IsFixedIsland(Vector2 coord, float xAdjust = 1f, float yAdjust = 1f, bool debug = false)
	{
		const int radix = 3;
		const int slice = 2;
		int pos = Mathf.RoundToInt(coord.x * xAdjust + coord.y * yAdjust);
		if (debug) Debug.Log("IsFixed: coord = " + coord + ", pos = " + pos + ", mod = " + pos % radix);
		pos %= radix;
		if (pos < 0) pos += radix;
		return pos < slice;
	}

	public static bool EveryOther(Vector2 coord)
    {
		return ((coord.x + coord.y) % 2) == 0;
    }

	static bool Check(Vector2 coord, IslandNoiseSettings settings, bool debug = false)
	{
		bool isLand = false;
		isLand = IsIsland(coord, settings, debug); 
		// isLand = IsFixedIsland(coord, 1f, 1f, debug);
		// isLand = EveryOther(coord);
		// if (debug) Debug.Log("Checking: " + coord + " returning: " + isLand);
		return isLand;
	}

	public static Anews LocalNews(Vector2 coord, IslandNoiseSettings settings, bool debug = false)
	{
		bool nw = false;
		bool n = false;
		bool ne = false;
		bool w = false;
		bool e = false;
		bool sw = false;
		bool s = false;
		bool se = false;
		bool isIsland = Check(coord, settings, debug);
		if (isIsland)
        {
			nw = Check(coord + new Vector2(-1, -1), settings, debug);
			n = Check(coord + new Vector2(0, -1), settings, debug);
			ne = Check(coord + new Vector2(1, -1), settings, debug);
			w = Check(coord + new Vector2(-1, 0), settings, debug);
			e = Check(coord + new Vector2(1, 0), settings, debug);
			sw = Check(coord + new Vector2(-1, 1), settings, debug);
			s = Check(coord + new Vector2(0, 1), settings, debug);
			se = Check(coord + new Vector2(1, 1), settings, debug);
		}
		Anews anews = new Anews(nw, n, ne, w, isIsland, e, sw, s, se);
		if (debug) Debug.Log("LocalNews for: " + coord + " is " + anews + " with index: " + anews.ToIndex());
		return anews;
	}


	public static float[,] GetIslandMap(int width, IslandNoiseSettings islandSettings, Vector2 centre)
	{
		// as we always use squares, use width for both dimentions!
		Debug.Log("GetIslandMap: width = " + width + ", centre = " + centre);
		float[,] values = new float[width, width];
		int halfX = (width + 1) / 2;
		int halfY = (width - 1) / 2;

		float minR = width * width;
		Vector2 nearest = centre;

		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < width; j++)
			{
				Vector2 test = new Vector2(i - halfX, halfY - j);
				values[i, j] = Check(test, islandSettings) ? 1f : 0f;
				if (values[i, j] > 0)
				{
					// Debug.Log("Created island at: " + test);
					float r = Vector2.Distance(test, centre);
					if (r < minR)
                    {
						minR = r;
						nearest = test;
						// Debug.Log("Created nearer island at: " + nearest);
					}
				}
			}
		}
		Debug.Log("GetIslandMap returns with: " + values.Length + ", and nearest: " + nearest);
		return values;
	}
}

[System.Serializable]
public class IslandNoiseSettings {
	public Noise.NormalizeMode normalizeMode;

	public float scale = 50;

	public int seed;
	public Vector2 offset;

	[Range(0, 1)]
	public float threshold = .95f;

	public void ValidateValues()
	{
		scale = Mathf.Max(scale, 0.01f);  // so we don't divide by 0!
	}

}

