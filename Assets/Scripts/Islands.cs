using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Islands 
{
	public static bool IsIsland(Vector2 coord, float threshold, float offset = 1f)
	{
		Vector2 movedCoord = coord * offset;
		float perlinValue = Mathf.PerlinNoise(movedCoord.x, movedCoord.y) * 2 - 1;
		return perlinValue > threshold;
	}

	public static bool IsFixedIsland(Vector2 coord, float xAdjust = 1f, float yAdjust = 1f)
	{
		const int radix = 3;
		const int slice = 2;
		int pos = Mathf.FloorToInt(coord.x * xAdjust + coord.y * yAdjust);
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
		// isLand = IsIsland(coord, .95f);
		isLand = IsFixedIsland(coord, 1f, 1f);
		// isLand = EveryOther(coord);
		if (debug) Debug.Log("Checking: " + coord + " returning: " + isLand);
		return isLand;
	}

	public static Anews LocalNews(Vector2 coord, bool debug=false)
	{
		Anews anews = null;
		bool isIsland = Check(coord, debug);
		if (isIsland)
        {
			Vector2 nw = coord + new Vector2(-1, -1);
			Vector2 n = coord + new Vector2(0, -1);
			Vector2 ne = coord + new Vector2(1, -1);
			Vector2 w = coord + new Vector2(-1, 0);
			Vector2 e = coord + new Vector2(1, 0);
			Vector2 sw = coord + new Vector2(-1, 1);
			Vector2 s = coord + new Vector2(0, 1);
			Vector2 se = coord + new Vector2(1, 1);
			// anews = new Anews(Check(nw, debug), Check(n, debug), Check(ne, debug),
			//	Check(e, debug), Check(w, debug),
			//	Check(se, debug), Check(s, debug), Check(sw, debug));
			anews = new Anews(isIsland, Check(n, debug), Check(e, debug), Check(s, debug), Check(w, debug));
		}
		if (debug)
        {
			if (anews != null) Debug.Log("LocalNews for: " + coord + " is " + anews + " with index: " + anews.ToIndex());
			else Debug.Log("LocalNews for: " + coord + " is " + anews + " (no index)");
		}
		return anews;
	}


	public static float[,] GetIslandMap(int width)
	{
		// as we always use squares, use width for both dimentions!
		float[,] values = new float[width, width];
		int halfX = (width + 1) / 2;
		int halfY = (width - 1) / 2;

		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < width; j++)
			{

				values[i, j] = Check(new Vector2(i - halfX, halfY - j))? 1f: 0f; 
			}
		}
		return values;
	}

}

