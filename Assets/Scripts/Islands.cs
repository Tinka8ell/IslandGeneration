using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class Islands
{
	public static IslandNoiseSettings settings;

	/// <summary>
	/// Get level of this coordinate.
	/// </summary>
	/// Use perlin noise to get a value for this coord.
	/// If greater than IslandNoiseSettings.threshold
	///    return value from 1 to IslandNoiseSettings.levels (island height)
	/// else
	///    return 0 (sea)
	/// <param name="coord">Where to test</param>
	/// <param name="debug">If to debug</param>
	/// <returns>int level between 0 and IslandNoiseSettings.levels</returns>
	public static int GetLevel(Vector2 coord, bool debug = false)
	{
		int maxLevel = 2 ^ settings.powerLevel;
		int level = 0;
		if (settings.useFixed)
        {
			// for testing we use a known fixed set of values
			if (Mathf.Abs(coord.x) <= settings.fixedIslands.GetLength(1) / 2 &&
				Mathf.Abs(coord.y) <= settings.fixedIslands.GetLength(0) / 2)
			{
				int col = settings.fixedIslands.GetLength(0) / 2 + (int)coord.x;
				int row = settings.fixedIslands.GetLength(1) / 2 + (int)coord.y;
				//Debug.LogFormat("GetLevel: row = {0}, col = {1}", row, col);
				level = settings.fixedIslands[col, row];
			}
		}
		else
        {
			System.Random prng = new System.Random(settings.seed);
			float offsetX = prng.Next(-100000, 100000) + coord.x + settings.offset.x;
			float offsetY = prng.Next(-100000, 100000) + coord.y + settings.offset.y;
			float sampleX = offsetX / settings.scale;
			float sampleY = offsetY / settings.scale;
			float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
			if (perlinValue > settings.threshold)
			{
				level = Mathf.CeilToInt(Mathf.Lerp(0, maxLevel, (perlinValue - settings.threshold) / (1 - settings.threshold)));
			}
			if (debug) Debug.LogFormat("IsIsland for {0}, offset=({1}, {2}), sample=({3},{4}), perlin={5}, level={6}",
				coord, offsetX, offsetY, sampleX, sampleY, perlinValue, level);
		}
		return (level == 0) ? 0 : level - 1 + maxLevel;
	}

	public static Vector2 NextDoor(Vector2 coord, Compass compass)
    {
		// North change away (+) and back (-1) - up and down
		// East change right (+1) and left (-) - right and left
		Vector2 change = Vector2.zero;
        switch (compass)
        {
			case Compass.N:
				change = Vector2.up;
				break;
			case Compass.NE:
				change = Vector2.up;
				change += Vector2.right;
				break;
			case Compass.E:
				change = Vector2.right;
				break;
			case Compass.SE:
				change = Vector2.down;
				change += Vector2.right;
				break;
			case Compass.S:
				change = Vector2.down;
				break;
			case Compass.SW:
				change = Vector2.down;
				change += Vector2.left;
				break;
			case Compass.W:
				change = Vector2.left;
				break;
			case Compass.NW:
				change = Vector2.up;
				change += Vector2.left;
				break;
		}
		return coord + change;

	}

	public static Anews LocalNews(Vector2 coord)
	{
		Anews anews = new(
			GetLevel(NextDoor(coord, Compass.NW)),
			GetLevel(NextDoor(coord, Compass.N)), 
			GetLevel(NextDoor(coord, Compass.NE)), 
			GetLevel(NextDoor(coord, Compass.W)),
			GetLevel(coord),
			GetLevel(NextDoor(coord, Compass.E)),
			GetLevel(NextDoor(coord, Compass.SW)),
			GetLevel(NextDoor(coord, Compass.S)),
			GetLevel(NextDoor(coord, Compass.SE))
			);
		//Debug.LogFormat("LocalNews: coord = {0}, anews = {1}", coord, anews);
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

		//float minR = width * width;
		//Vector2 nearest = centre;
		// [0, 0] = bottom left => centre + half * DOWN + half * LEFT
		Vector2 bottomLeft = new Vector2(centre.x - half, centre.y - half); 

		// [j, i] => top left + Down * j + Right * i

		for (int j = 0; j < width; j++) // bottom to top (S to N) => +dz
		{
			for (int i = 0; i < width; i++) // left to right (W to E) => +dx
			{
				Vector2 test = new Vector2(bottomLeft.x + i, bottomLeft.y + j);
				values[i, j] = GetLevel(test);
				//if (values[j, i] > 0) // found an island!
				//{
				//	// Debug.LogFormat("Found island at: {0}", test);
				//	float r = Vector2.Distance(test, centre);
				//	if (r < minR)
    //                {
				//		minR = r;
				//		nearest = test;
				//	}
				//}
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
		while (GetLevel(test) == 0) // not found one yet
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

	public static int levels = 2^2; // highest possible level
	public static int maxLevel = (levels + 1) * 4; 
	[Range(1, 7)]
	public int powerLevel = 1;

	public bool useFixed = false;

	public int[,] fixedIslands = new int[3, 3]{ // note it is not layed out the way it looks!
		{0, 1, 0},
		{1, 2, 1},
		{0, 1, 0}
	};

	public void ValidateValues()
	{
		scale = Mathf.Max(scale, 0.01f);  // so we don't divide by 0!
	}

}

