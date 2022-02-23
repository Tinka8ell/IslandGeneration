using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class Islands
{
	public static IslandNoiseSettings settings;

	public static bool Check(Vector2 coord, bool debug = false)
	{
		System.Random prng = new System.Random(settings.seed);
		float offsetX = prng.Next(-100000, 100000) + coord.x + settings.offset.x;
		float offsetY = prng.Next(-100000, 100000) + coord.y + settings.offset.y;
		float sampleX = offsetX / settings.scale;
		float sampleY = offsetY / settings.scale;
		float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
		if (debug) Debug.LogFormat("IsIsland for {0}, offset=({1}, {2}), sample=({3},{4}), perlin={5}",
			coord, offsetX, offsetY, sampleX, sampleY, perlinValue);
		return perlinValue > settings.threshold;
	}

	public static Vector2 NextDoor(Vector2 coord, Anews.Compass compass)
    {
		// North change away (+) and back (-1) - up and down
		// East change right (+1) and left (-) - right and left
		Vector2 change = Vector2.zero;
        switch (compass)
        {
			case Anews.Compass.N:
				change = Vector2.up;
				break;
			case Anews.Compass.NE:
				change = Vector2.up;
				change += Vector2.right;
				break;
			case Anews.Compass.E:
				change = Vector2.right;
				break;
			case Anews.Compass.SE:
				change = Vector2.down;
				change += Vector2.right;
				break;
			case Anews.Compass.S:
				change = Vector2.down;
				break;
			case Anews.Compass.SW:
				change = Vector2.down;
				change += Vector2.left;
				break;
			case Anews.Compass.W:
				change = Vector2.left;
				break;
			case Anews.Compass.NW:
				change = Vector2.up;
				change += Vector2.left;
				break;
		}
		return coord + change;

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
		/* As we always use squares, use width for both dimentions!
		 * coord (1, 0) => (E 0) and (0, 1) => (0, N), etc
		 * Map TopLeft is (W, N): N = floor(centre.y / (width/2f)), W = - floor(centre.x / (witdh/2f)
		 * Map BottomRight is (E, S): S = - floor(centre.y / (width/2f)), E = floor(centre.x / (witdh/2f)
		 * Assuming width is odd: N - S = width - 1
		 *                   and: E - W = width - 1
		 */
		float[,] values = new float[width, width];
		float half = Mathf.Floor(width / 2f);

		float minR = width * width;
		Vector2 nearest = centre;
		// [0, 0] = tl => centre + half * UP + half * LEFT
		Vector2 topLeft = new Vector2(centre.x + half, centre.y + half); 

		// [j, i] => top left + Down * j + Right * i

		for (int j = 0; j < width; j++) // top to bottom (N to S) => -dz
		{
			for (int i = 0; i < width; i++) // left to right (W to E) => +dx
			{
				Vector2 test = new Vector2(topLeft.x - i, topLeft.y - j);
				values[j, i] = Check(test) ? 1f : 0f;
				if (values[j, i] > 0) // found an island!
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

