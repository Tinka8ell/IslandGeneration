using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class Islands
{
	public static IslandNoiseSettings settings;

	public enum FindMode { None, Nearest, DriftEast }

	public static Vector2 FindAnIsland(Vector2 start, FindMode findMode)
    {
		Vector2 test = start;
        switch (findMode)
        {
			case FindMode.Nearest:
				test = NearestIsland(test);
				break;
			case FindMode.DriftEast:
				test = DriftEastToIsland(test);
				break;
			default:
				// Just return original
				break;
		}
		return test;
    }

	public static Vector2 NearestIsland(Vector2 start)
	{
		Vector2 test = start;
		int offset = 0;
		int radius = 0;
		int maxOffset = -1;
		while (!Check(test)) // not found one yet
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
				offset = 0;
				maxOffset = radius * 2 * 4;
				test = new Vector2(start.x - radius, start.y - radius);
			}
		}
		return test;
	}

	public static Vector2 DriftEastToIsland(Vector2 start)
	{
		Vector2 test = start;
		return test;
	}

	public static bool IsIsland(Vector2 coord, bool debug = false)
	{
		System.Random prng = new System.Random(settings.seed);
		float offsetX = prng.Next(-100000, 100000) + settings.offset.x + coord.x;
		float offsetY = prng.Next(-100000, 100000) - settings.offset.y - coord.y;
		float sampleX = offsetX / settings.scale;
		float sampleY = offsetY / settings.scale;
		float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
		if (debug) Debug.LogFormat("IsIsland for {0}, offset=({1}, {2}), sample=({3},{4}), perlin={5}",
			coord, offsetX, offsetY, sampleX, sampleY, perlinValue);
		return perlinValue > settings.threshold;
	}

	public static bool IsFixedIsland(Vector2 coord, float xAdjust = 1f, float yAdjust = 1f)
	{
		const int radix = 3;
		const int slice = 2;
		int pos = Mathf.RoundToInt(coord.x * xAdjust + coord.y * yAdjust);
		pos %= radix;
		if (pos < 0) pos += radix;
		return pos < slice;
	}

	public static bool EveryOther(Vector2 coord)
    {
		return ((coord.x + coord.y) % 2) == 0;
    }

	static bool Check(Vector2 coord, bool debug = false)
	{
		bool isLand = false;
		isLand = IsIsland(coord, debug); 
		// isLand = IsFixedIsland(coord, 1f, 1f);
		// isLand = EveryOther(coord);
		return isLand;
	}

	public static Vector2 NextDoor(Vector2 coord, Anews.Compass compass)
    {
		float dx = 0;
		float dy = 0;
        switch (compass)
        {
			case Anews.Compass.N:
				dy = -1;
				break;
			case Anews.Compass.NE:
				dx = +1;
				dy = -1;
				break;
			case Anews.Compass.E:
				dx = +1;
				break;
			case Anews.Compass.SE:
				dx = +1;
				dy = +1;
				break;
			case Anews.Compass.S:
				dy = +1;
				break;
			case Anews.Compass.SW:
				dx = -1;
				dy = +1;
				break;
			case Anews.Compass.W:
				dx = -1;
				break;
			case Anews.Compass.NW:
				dx = -1;
				dy = -1;
				break;
		}
		return new Vector2(coord.x + dx, coord.y + dy);

	}

	public static Anews LocalNews(Vector2 coord)
	{
		bool nw = false;
		bool n = false;
		bool ne = false;
		bool w = false;
		bool e = false;
		bool sw = false;
		bool s = false;
		bool se = false;
		bool isIsland = Check(coord);
		if (isIsland)
        {
			nw = Check(NextDoor(coord, Anews.Compass.NW));
			n = Check(NextDoor(coord, Anews.Compass.N));
			ne = Check(NextDoor(coord, Anews.Compass.NE));
			w = Check(NextDoor(coord, Anews.Compass.W));
			e = Check(NextDoor(coord, Anews.Compass.E));
			sw = Check(NextDoor(coord, Anews.Compass.SW));
			s = Check(NextDoor(coord, Anews.Compass.S));
			se = Check(NextDoor(coord, Anews.Compass.SE));
		}
		Anews anews = new Anews(nw, n, ne, w, isIsland, e, sw, s, se);
		return anews;
	}


	public static float[,] GetIslandMap(int width, IslandNoiseSettings islandSettings, Vector2 centre)
	{
		settings = islandSettings;
		// as we always use squares, use width for both dimentions!
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
				values[i, j] = Check(test) ? 1f : 0f;
				if (values[i, j] > 0)
				{
					// Debug.LogFormat("Found island at: {0}", test);
					float r = Vector2.Distance(test, centre);
					if (r < minR)
                    {
						minR = r;
						nearest = test;
					}
				}
			}
		}
		// Debug.LogFormat("Nearest island at: {0} and is {1} from {2}", nearest, minR, centre);
		return values;
	}
}

[System.Serializable]
public class IslandNoiseSettings {
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

